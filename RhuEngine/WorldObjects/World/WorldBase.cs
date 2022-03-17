using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.AssetSystem;
using StereoKit;
using RhuEngine.Components;

namespace RhuEngine.WorldObjects
{
	public partial class World : IWorldObject
	{

		public bool IsLoading => IsDeserializing || IsLoadingNet;

		public bool IsPersonalSpace { get; private set; }

		public readonly AssetSession assetSession;
		public readonly WorldManager worldManager;
		public Engine Engine => worldManager.Engine;

		public World(WorldManager worldManager) {
			this.worldManager = worldManager;
			assetSession = new AssetSession(worldManager.Engine.assetManager,this);
		}

		public string LoadMsg = "Starting to LoadWorld";

		public void Initialize(bool networkedWorld, bool networkedObject, bool deserialize, bool isPersonalSpace) {
			IsPersonalSpace = isPersonalSpace;
			foreach (var item in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)) {
				if (typeof(SyncObject).IsAssignableFrom(item.FieldType) && item.GetCustomAttribute<NoLoadAttribute>() is null) {
					var instance = (SyncObject)Activator.CreateInstance(item.FieldType);
					instance.Initialize(this, this, item.Name, networkedObject, deserialize);
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
			if (isPersonalSpace | !networkedWorld) {
				IsDeserializing = false;
				IsLoadingNet = false;
				AddLocalUser();
			}
		}


		public enum FocusLevel
		{
			Background,
			Focused,
			Overlay,
			PrivateOverlay
		}

		private FocusLevel _focus = FocusLevel.Background;
		public DateTime LastFocusChange { get; private set; }

		public FocusLevel Focus
		{
			get => _focus;
			set {
				if (_focus != value) {
					_focus = value;
					if (value == FocusLevel.Focused) {
						if (worldManager.FocusedWorld != null) {
							worldManager.FocusedWorld._focus = FocusLevel.Background;
						}
						worldManager.FocusedWorld = this;
						if (GetLocalUser() is not null) {
							GetLocalUser().isPresent.Value = true;
						}
					}
					else {
						if (GetLocalUser() is not null) {
							GetLocalUser().isPresent.Value = false;
						}
					}
					LastFocusChange = DateTime.UtcNow;
					UpdateFocus();
				}
			}
		}
		[NoSync]
		[NoSave]
		[NoShow]
		[UnExsposed]
		[NoLoad]
		public ScriptBuilder FocusedScriptBuilder = null;

		[NoShow]
		public Entity RootEntity;

		[NoSync]
		[NoSave]
		public Sync<string> SessionID;

		[NoSave]
		[Default("New Session")]
		[OnChanged(nameof(SessionNameChanged))]
		public Sync<string> SessionName;

		[Default("New World")]
		public Sync<string> WorldName;
		[Exsposed]
		public string WorldDebugName => $"{(IsPersonalSpace ? "P" : "")}{((worldManager.LocalWorld == this) ? "L" : "")} {SessionName.Value}";

		private void UpdateFocus() {
		}

		/// <summary>
		/// Time in seconds the most recent Step call took.
		/// </summary>
		public float stepTime = 0f;

		public object RenderLock = new();

		public void Step() {
			_netManager?.PollEvents();
			_netManager?.NatPunchModule.PollEvents();
			WorldThreadSafty.MethodCalls = 0;
			UpdateCoroutine();
			if (IsLoading | (_focus == FocusLevel.Background)) {
				return;
			}
			try {
				foreach (var item in _globalStepables) {
					item.Step();
				}
			}
			catch (Exception e) {
				Log.Err($"Failed to update global stepables for session {WorldDebugName}. Error: {e}");
			}
			try {
				var sortedUpdatingEntities = from ent in _updatingEntities.AsParallel()
											 group ent by ent.CachedDepth;
				var sorted = from groupe in sortedUpdatingEntities
							 orderby groupe.Key ascending
							 select groupe;
				foreach (var item in sorted) {
					foreach (var ent in item) {
						ent.Step();
					}
				}
			}
			catch (Exception e) {
				Log.Err($"Failed to update entities for session {WorldDebugName}. Error: {e}");
			}

			try {
				lock (RenderLock) {
					foreach (var item in _renderingComponents) {
						item.Render();
					}
				}
			}
			catch (Exception e) {
				Log.Err($"Failed to build render queue for session {WorldDebugName}. Error {e}");
			}
		}

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			if (IsDisposed) {
				return;
			}
			IsDisposed = true;
			assetSession.Dispose();
			try {
				worldManager.RemoveWorld(this);
			}
			catch { }
			try {
				if (!IsLoading) {
					GetLocalUser()?.userRoot.Target?.Entity.Dispose();
					if (_netManager is not null) {
						for (var i = 0; i < 3; i++) {
							_netManager.PollEvents();
							Thread.Sleep(100);
						}
					}
				}
			}
			catch { }
			try {
				_client?.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).Wait();
			}
			catch { }
			try {
				_client?.Dispose();
			}
			catch { }
			try {
				_netManager?.DisconnectAll();
			}
			catch { }
			GC.Collect();
		}
	}
}
