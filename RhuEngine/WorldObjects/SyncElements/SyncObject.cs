using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.WorldObjects
{
	public abstract class SyncObject : ISyncObject
	{
		private readonly HashSet<IDisposable> _disposables = new();

		public void AddDisposable(IDisposable disposable) {
			lock (disposable) {
				_disposables.Add(disposable);
			}
		}

		public void RemoveDisposable(IDisposable disposable) {
			lock (disposable) {
				_disposables.Remove(disposable);
			}
		}

		public User LocalUser => World.GetLocalUser();

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

		public Engine Engine => WorldManager.Engine;

		public WorldManager WorldManager => World.worldManager;

		public World World
		{
			get; private set;
		}

		public virtual string Name
		{
			get; private set;
		}

		public EditLevel LocalEditLevel
		{
			get; set;
		}

		public EditLevel EditLevel => (LocalEditLevel == EditLevel.None) ? Parent?.EditLevel ?? EditLevel.None : LocalEditLevel;

		public virtual bool Persistence => true;

		public event Action<object> OnDispose;

		public virtual void Destroy() {
			if (IsDestroying) {
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
			OnDispose?.Invoke(this);
			IsRemoved = true;
			if (typeof(IGlobalStepable).IsAssignableFrom(GetType())) {
				World.UnregisterGlobalStepable((IGlobalStepable)this);
			}
			foreach (var item in _disposables.ToArray()) {
				if(item is SyncObject @object) {
					@object.IsDestroying = true;
				}
				item?.Dispose();
			}
			World.UnRegisterWorldObject(this);
		}

		public virtual void FirstCreation() {

		}

		public virtual void OnInitialize() {

		}

		public void Initialize(World world, IWorldObject parent, string name, bool networkedObject, bool deserialize,Func<NetPointer> netPointer = null) {
			if (GetType().GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) != null && !world.IsPersonalSpace) {
				throw new InvalidOperationException("This SyncObject is PrivateSpaceOnly");
			}
			Name = name;
			World = world;
			Parent = parent;
			if (!networkedObject) {
				Pointer = netPointer is null ? World.NextRefID() : netPointer.Invoke();
				World.RegisterWorldObject(this);
			}
			InitializeMembers(networkedObject, deserialize,netPointer);
			OnInitialize();
			if (!deserialize) {
				OnLoaded();
			}
			if (typeof(IGlobalStepable).IsAssignableFrom(GetType())) {
				world.RegisterGlobalStepable((IGlobalStepable)this);
			}
		}

		public virtual void OnSave() {
		}


		public virtual void InitializeMembers(bool networkedObject, bool deserialize, Func<NetPointer> netPointer) {
			try {
				var data = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var item in data) {
					if((item.Attributes & FieldAttributes.InitOnly) == 0) {
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
								((ISync)instance).SetValue(startValue.Data);
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
							var startValue = item.GetCustomAttribute<OnChangedAttribute>();
							if (startValue != null) {
								var method = GetType().GetMethod(startValue.Data, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
								if (method is null) {
									throw new Exception($"Method {startValue.Data} not found");
								}
								else {
									var prams = method.GetParameters();
									if (prams.Length == 0) {
										((IChangeable)instance).Changed += (obj) => method.Invoke(this, new object[0] { });
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

		public virtual void OnLoaded() {
		}

		public void ChangeName(string name) {
			Name = name;
		}
	}
}
