using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Managers;

using StereoKit;

namespace RhuEngine.WorldObjects
{
	public class SyncObject : ISyncObject
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
			Task.Run(() => {
				try {
					IsDestroying = true;
					Dispose();
				}
				catch (Exception e) {
					Log.Err($"Error When Destroying {Name} {Pointer} type:{GetType().GetFormattedName()} Error:{e}");
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
				FirstCreation();
				OnLoaded();
			}
			if (typeof(IGlobalStepable).IsAssignableFrom(GetType())) {
				world.RegisterGlobalStepable((IGlobalStepable)this);
			}
		}

		public virtual void OnSave() {
		}


		public virtual void InitializeMembers(bool networkedObject, bool deserialize, Func<NetPointer> netPointer) {
			var data = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var item in data) {
				if (typeof(SyncObject).IsAssignableFrom(item.FieldType) && !((item.GetCustomAttribute<NoSaveAttribute>() != null) && (item.GetCustomAttribute<NoSyncAttribute>() != null))) {
					var instance = (SyncObject)Activator.CreateInstance(item.FieldType);
					instance.Initialize(World, this, item.Name, networkedObject, deserialize,netPointer);
					AddDisposable(instance);
					if (typeof(ISync).IsAssignableFrom(item.FieldType)) {
						var startValue = item.GetCustomAttribute<DefaultAttribute>();
						if (startValue != null) {
							((ISync)instance).SetValue(startValue.Data);
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
								Log.Err($"Method {startValue.Data} not found");
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
									Log.Err($"Cannot call method {startValue.Data} on type {GetType().GetFormattedName()}");
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
