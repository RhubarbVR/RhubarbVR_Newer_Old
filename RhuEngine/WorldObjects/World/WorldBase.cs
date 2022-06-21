using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.AssetSystem;
using RhuEngine.Components;
using SharedModels.Session;
using SharedModels.UserSession;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RNumerics;

namespace RhuEngine.WorldObjects
{
	public partial class World : IWorldObject {
		public PhysicsSim PhysicsSim { get; set; }

		public bool IsLoading => IsDeserializing || IsLoadingNet;

		public bool IsPersonalSpace { get; private set; }

		public readonly AssetSession assetSession;
		public readonly WorldManager worldManager;
		public Engine Engine => worldManager.Engine;

		public World(WorldManager worldManager) {
			PhysicsSim = new PhysicsSim();
			this.worldManager = worldManager;
			assetSession = new AssetSession(worldManager.Engine.assetManager, this);
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
								RLog.Err($"Method {startValue.Data} not found");
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
									RLog.Err($"Cannot call method {startValue.Data} on type {GetType().GetFormattedName()}");
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
			StartTime.Value = DateTime.UtcNow;
			if (isPersonalSpace | !networkedWorld) {
				IsDeserializing = false;
				AddLocalUser();
				IsLoadingNet = false;
				LoadMsg = "Done loading none networked world";
			}
		}


		public enum FocusLevel {
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
							worldManager.FocusedWorld.Focus = FocusLevel.Background;
						}
						worldManager.FocusedWorld = this;
						if (GetLocalUser() is not null) {
							GetLocalUser().isPresent.Value = true;
						}
						if (Engine.netApiManager.UserStatus is not null) {
							Engine.netApiManager.UserStatus.CurrentSession = SessionID.Value;
							Engine.netApiManager.UserStatus = Engine.netApiManager.UserStatus;
						}
					}
					else {
						if (GetLocalUser() is not null) {
							GetLocalUser().isPresent.Value = false;
						}
					}
					LastFocusChange = DateTime.UtcNow;
					UpdateFocus();
					worldManager.OnWorldUpdateTaskBar?.Invoke();
				}
			}
		}
		[NoSync]
		[NoSave]
		[NoShow]
		[UnExsposed]
		[NoLoad]
		public ScriptBuilder FocusedScriptBuilder = null;
		
		[Exposed]
		[NoShow]
		public readonly Entity RootEntity;

		[NoSync]
		[NoSave]
		[NoSyncUpdate]
		public readonly Sync<string> SessionID;

		[NoSave]
		[Default("New Session")]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<string> SessionName;

		[NoSave]
		[Default("https://rhubarbvr.net/images/RhubarbVR2.png")]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<string> ThumNail;

		[NoSave]
		[Default(false)]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<bool> IsHidden;

		[NoSave]
		[Default(null)]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<string> AssociatedGroup;

		[NoSave]
		[Default(SessionAccessLevel.Public)]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<SessionAccessLevel> AccessLevel;

		[NoSave]
		[Default(30)]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<int> MaxUserCount;

		[NoSave]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly SyncValueList<string> SessionTags;

		[NoSave]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly SyncValueList<string> Admins;

		[NoSave]
		public readonly Sync<DateTime> StartTime;

		public double WorldTime => (DateTime.UtcNow - StartTime).TotalSeconds;

		[Default("New World")]
		public readonly Sync<string> WorldName;
		[Exposed]
		public string WorldDebugName => $"{(IsPersonalSpace ? "P" : "")}{((worldManager.LocalWorld == this) ? "L" : "")} {SessionName.Value}";

		public event Action FoucusChanged;

		private void UpdateFocus() {
			FoucusChanged?.Invoke();
		}

		/// <summary>
		/// Time in seconds the most recent Step call took.
		/// </summary>
		public float stepTime = 0f;

		public object RenderLock = new();


		[NoSync]
		[NoSave]
		[NoShow]
		[UnExsposed]
		[NoLoad]
		public GrabbableHolder LeftGrabbableHolder;
		[NoSync]
		[NoSave]
		[NoShow]
		[UnExsposed]
		[NoLoad]
		public GrabbableHolder RightGrabbableHolder;
		[NoSync]
		[NoSave]
		[NoShow]
		[UnExsposed]
		[NoLoad]
		public GrabbableHolder HeadGrabbableHolder;

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
				RLog.Err($"Failed to update global stepables for session {WorldDebugName}. Error: {e}");
			}
			PhysicsSim.UpdateSim(RTime.Elapsedf);
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
				RLog.Err($"Failed to update entities for session {WorldDebugName}. Error: {e}");
			}

			try {
				if (Engine.EngineLink.CanRender) {
					lock (RenderLock) {
						foreach (var item in _renderingComponents) {
							item.Render();
						}
					}
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to build render queue for session {WorldDebugName}. Error {e}");
			}
		}

		public bool IsDisposed { get; private set; }
		public bool HasError { get; internal set; }

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
			if (IsNetworked) {
				if (!HasError) {
					Engine.netApiManager.SendDataToSocked(new SessionRequest { ID = SessionID.Value, RequestData = SessionID.Value, RequestType = RequestType.LeaveSession });
				}
			}
			try {
				_netManager?.DisconnectAll();
			}
			catch { }
			GC.Collect();
		}
	}
}
