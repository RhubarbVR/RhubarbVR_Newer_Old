using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public delegate NetPointer NetPointerUpdateDelegate();

	public abstract class SyncObject : ISyncObject
	{
		private IDisposable[] _disposables = Array.Empty<IDisposable>();

		public void AddDisposable(IDisposable disposable) {
			lock (disposable) {
				Array.Resize(ref _disposables, _disposables.Length + 1);
				_disposables[_disposables.Length - 1] = disposable;
			}
		}

		public UserProgramManager ProgramManager => WorldManager?.PrivateSpaceManager?._ProgramManager;
		public PrivateSpaceManager PrivateSpaceManager => WorldManager?.PrivateSpaceManager;
		public User LocalUser => World.GetLocalUser();
		public User MasterUser => World.GetMasterUser();
		public InputManager InputManager => Engine.inputManager;
		public bool IsDestroying
		{
			get; set;
		}

		public bool IsRemoved
		{
			get; private set;
		}


		public NetPointer Pointer
		{
			get; set;
		}

		public IWorldObject Parent
		{
			get; private set;
		}

		public Engine Engine => WorldManager?.Engine;

		public WorldManager WorldManager => World?.worldManager;

		public World World
		{
			get; private set;
		}

		public virtual string Name
		{
			get; private set;
		}

		public virtual bool Persistence => true;

		public event Action<object> OnDispose;
		[Exposed]
		public virtual void Destroy() {
			if (IsDestroying) {
				return;
			}
			if (Parent is not ISyncList) {
				return;
			}
			IsDestroying = true;
			Task.Run(() => {
				try {
					Dispose();
				}
				catch (Exception e) {
					RLog.Err($"Error When Destroying {Name} {Pointer} type:{GetType().GetFormattedName()} Error:{e}");
				}
			});
		}

		public virtual void Dispose() {
			if (IsRemoved) {
				return;
			}
			IsRemoved = true;
			IsDestroying = true;
			OnDispose?.Invoke(this);
			if (this is IGlobalStepable global) {
				World.UnregisterGlobalStepable(global);
			}
			foreach (var item in _disposables.ToArray()) {
				if (item is SyncObject @object) {
					@object.IsDestroying = true;
				}
				item?.Dispose();
			}
			_disposables = Array.Empty<IDisposable>();
			World?.UnRegisterWorldObject(this);
			OnDispose = null;
			Parent = null;
			ClearAllSyncMembers();
			GC.SuppressFinalize(this);
		}

		protected virtual void FirstCreation() {

		}

		protected virtual void OnInitialize() {

		}

		public class NotVailedGenaric : Exception
		{
			public override string Message => "Generic given is invalid";
		}

		public bool IsInitialized { get; private set; } = false;

		protected virtual void SaftyChecks() { }

		public void Initialize(World world, IWorldObject parent, string name, bool networkedObject, bool deserialize, NetPointerUpdateDelegate netPointer = null) {
			try {
				if (GetType().GetCustomAttribute<OverlayOnlyAttribute>(true) != null && !(world.IsOverlayWorld || world.IsPersonalSpace)) {
					throw new InvalidOperationException("This SyncObject is OverlayOnly");
				}
				if (GetType().GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) != null && !world.IsPersonalSpace) {
					throw new InvalidOperationException("This SyncObject is PrivateSpaceOnly");
				}
				SaftyChecks(); 
				var arguments = GetType().GetGenericArguments();
				foreach (var arguiminet in arguments) {
					var isVailed = false;
					var types = GetType().GetCustomAttributes<GenericTypeConstraintAttribute>(true);
					if (!types.Any()) {
						isVailed = true;
					}
					foreach (var item in types) {
						foreach (var typ in item.Data) {
							if (typ == typeof(Enum)) {
								if (arguiminet.IsEnum) {
									isVailed = true;
								}
							}
							if (arguiminet.IsAssignableFrom(typ)) {
								isVailed = true;
							}
						}
						var LoopGroups = item.Groups switch {
							TypeConstGroups.Serializable => TypeCollections.StandaredTypes,
							_ => Array.Empty<Type>(),
						};
						foreach (var typ in LoopGroups) {
							if (typ == typeof(Enum)) {
								if (arguiminet.IsEnum) {
									isVailed = true;
								}
							}
							if (arguiminet.IsAssignableFrom(typ)) {
								isVailed = true;
							}
						}
					}
					if (!isVailed) {
						throw new NotVailedGenaric();
					}
				}
				Name = name;
				World = world;
				Parent = parent;
				if (!networkedObject) {
					Pointer = netPointer is null ? World.NextRefID() : netPointer.Invoke();
					World.RegisterWorldObject(this);
				}
				InitializeMembers(networkedObject, deserialize, netPointer);
				try {
					OnInitialize();
					if (!deserialize) {
						OnLoaded();
					}
				}
				catch (Exception ex) {
					throw new Exception("Failed to load", ex);
				}
				if (typeof(IGlobalStepable).IsAssignableFrom(GetType())) {
					world.RegisterGlobalStepable((IGlobalStepable)this);
				}
				IsInitialized = true;
			}
			catch(Exception e) {
				try {
					Dispose();
				}
				catch { }
				throw;
			}
		}

		public void RunOnSave() {
			OnSave();
		}

		protected virtual void OnSave() {
		}

		protected virtual void ClearAllSyncMembers() {
			var data = GetType().FastGetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			foreach (var item in data) {
				if (typeof(SyncObject).IsAssignableFrom(item.FieldType)) {
					item.SetValue(this, null);
				}
			}
		}

		protected virtual void InitializeMembers(bool networkedObject, bool deserialize, NetPointerUpdateDelegate netPointer) {
			try {
				var data = GetType().FastGetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				foreach (var item in data) {
					if ((item.Attributes & FieldAttributes.InitOnly) == 0) {
						continue;
					}
					if ((item.GetCustomAttribute<NoLoadAttribute>() == null) && typeof(SyncObject).IsAssignableFrom(item.FieldType) && !((item.GetCustomAttribute<NoSaveAttribute>() != null) && (item.GetCustomAttribute<NoSyncAttribute>() != null))) {
						var instance = (SyncObject)Activator.CreateInstance(item.FieldType);
						instance.Initialize(World, this, item.Name, networkedObject, deserialize, netPointer);
						AddDisposable(instance);
						if (typeof(ISyncProperty).IsAssignableFrom(item.FieldType)) {
							var startValue = item.GetCustomAttribute<BindPropertyAttribute>();
							if (startValue != null) {
								((ISyncProperty)instance).Bind(startValue.Data, this);
							}
						}
						if (typeof(ISync).IsAssignableFrom(item.FieldType)) {
							var startValue = item.GetCustomAttribute<DefaultAttribute>();
							if (startValue != null) {
								((ISync)instance).SetValueForce(startValue.Data);
							}
							else {
								((ISync)instance).SetStartingObject();
							}
						}
						if (typeof(IAssetRef).IsAssignableFrom(item.FieldType)) {
							var startValue = item.GetCustomAttribute<OnAssetLoadedAttribute>();
							if (startValue != null) {
								((IAssetRef)instance).BindMethod(startValue.Data, this);
							}
						}
						if (typeof(IChangeable).IsAssignableFrom(item.FieldType)) {
							//RLog.Info($"Loaded Change Field {GetType().GetFormattedName()} , {item.Name} type {item.FieldType.GetFormattedName()}");
							var startValue = item.GetCustomAttribute<OnChangedAttribute>();
							if (startValue != null) {
								var method = GetType().FastGetMethods(startValue.Data, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
								if (method is null) {
									throw new Exception($"Method {startValue.Data} not found");
								}
								else {
									var prams = method.GetParameters();
									if (prams.Length == 0) {
										((IChangeable)instance).Changed += (obj) => method.Invoke(this, Array.Empty<object>());
									}
									else if (prams[0].ParameterType == typeof(IChangeable)) {
										((IChangeable)instance).Changed += (obj) => method.Invoke(this, new object[1] { obj });
									}
									else {
										throw new Exception($"Cannot call method {startValue.Data} on type {GetType().GetFormattedName()}");
									}
								}
							}
						}
						if (typeof(INetworkedObject).IsAssignableFrom(item.FieldType)) {
							var startValue = item.GetCustomAttribute<NoSyncUpdateAttribute>();
							if (startValue != null) {
								((INetworkedObject)instance).NoSync = true;
							}
						}
						item.SetValue(this, instance);
					}
				}
			}
			catch (Exception ex) {
				RLog.Err("Failed to InitializeMembers" + ex.ToString());
				throw new Exception("Failed to InitializeMembers", ex);
			}
		}
		public virtual IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return syncObjectSerializerObject.CommonWorkerSerialize(this);
		}

		public virtual void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.Deserialize((DataNodeGroup)data, this);
		}

		protected virtual void OnLoaded() {
		}

		public event Action<string> NameChange;

		public void ChangeName(string name) {
			Name = name;
			NameChange?.Invoke(name);
		}

		void ISyncObject.CallFirstCreation() {
			FirstCreation();
		}

		void ISyncObject.RunOnLoad() {
			OnLoaded();
		}
	}
}
