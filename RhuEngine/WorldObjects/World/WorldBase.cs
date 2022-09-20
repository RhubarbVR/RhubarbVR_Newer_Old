﻿using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.AssetSystem;
using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RNumerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public sealed partial class World : IWorldObject
	{
		public PhysicsSim PhysicsSim { get; set; }

		public bool IsLoading => (IsDeserializing || IsLoadingNet) & !HasError;

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
			try {
				var data = typeof(World).GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var item in data) {
					if ((item.Attributes & FieldAttributes.InitOnly) == 0) {
						continue;
					}
					if ((item.GetCustomAttribute<NoLoadAttribute>() == null) && typeof(SyncObject).IsAssignableFrom(item.FieldType) && !((item.GetCustomAttribute<NoSaveAttribute>() != null) && (item.GetCustomAttribute<NoSyncAttribute>() != null))) {
						var instance = (SyncObject)Activator.CreateInstance(item.FieldType);
						instance.Initialize(this, this, item.Name, networkedObject, deserialize, null);
						_disposables.Add(instance);
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
			StartTime.Value = DateTime.UtcNow;
			if (isPersonalSpace | !networkedWorld) {
				IsDeserializing = false;
				AddLocalUser();
				IsLoadingNet = false;
				LoadMsg = "Done loading none networked world";
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
							worldManager.FocusedWorld.Focus = FocusLevel.Background;
						}
						worldManager.FocusedWorld = this;
						if (GetLocalUser() is not null) {
							GetLocalUser().isPresent.Value = true;
						}
						//ToDO: set focus on net api
						//if (Engine.netApiManager.UserStatus is not null) {
						//	Engine.netApiManager.UserStatus.CurrentSession = Guid.Parse(SessionID.Value);
						//	Engine.netApiManager.UserStatus = Engine.netApiManager.UserStatus;
						//}
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

		[Exposed]
		[NoShow]
		public readonly Entity RootEntity;

		[NoSave]
		[NoSyncUpdate]
		public readonly Sync<string> SessionID;

		[NoSave]
		[NoSyncUpdate]
		public readonly Sync<string> WorldID;

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
		[Default(DataModel.Enums.AccessLevel.Public)]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<DataModel.Enums.AccessLevel> AccessLevel;

		[NoSave]
		[Default(30)]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<int> MaxUserCount;

		[NoSave]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly SyncValueList<string> SessionTags;

		[NoSave]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly SyncObjList<SyncRef<User>> Admins;

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

		public void RenderStep() {
			_netManager?.PollEvents();
			_netManager?.NatPunchModule.PollEvents();
			WorldThreadSafty.MethodCalls = 0;
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
			try {
				var sortedUpdatingEntities = from ent in _updatingEntities.AsParallel()
											 group ent by ent.Depth;
				var sorted = from groupe in sortedUpdatingEntities
							 orderby groupe.Key ascending
							 select groupe;
				foreach (var item in sorted) {
					foreach (var ent in item) {
						ent.RenderStep();
					}
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to update entities for session {WorldDebugName}. Error: {e}");
			}
			try {
				if (Engine.EngineLink.CanRender) {
					lock (_worldLinkComponents) {
						foreach (var item in _worldLinkComponents) {
							item.RunRender();
						}
					}
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to build render queue for session {WorldDebugName}. Error {e}");
			}
		}

		private readonly List<IGrouping<uint, Entity>> _sorrtedEntity = new();

		private  bool _sortEntitys;

		public void Step() {
			_netManager?.PollEvents();
			_netManager?.NatPunchModule.PollEvents();
			WorldThreadSafty.MethodCalls = 0;
			if (IsLoading | (_focus == FocusLevel.Background)) {
				return;
			}
			try {
				PhysicsSim.UpdateSim(RTime.Elapsedf);
			}
			catch (Exception e) {
				RLog.Err($"Failed To update PhysicsSim Error:{e}");
			}
			try {
				if (_sortEntitys) {
					lock (_updatingEntities) {
						var sortedUpdatingEntities = from ent in _updatingEntities.AsParallel()
													 group ent by ent.Depth;
						_sorrtedEntity.Clear();
						_sorrtedEntity.AddRange(from groupe in sortedUpdatingEntities
												orderby groupe.Key ascending
												select groupe);
						_sortEntitys = false;
					}
				}
				foreach (var item in _sorrtedEntity) {
					foreach (var ent in item) {
						ent.Step();
					}
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to update entities for session {WorldDebugName}. Error: {e}");
			}
		}
		public event Action<World> IsDisposeing;
		public bool IsDisposed { get; private set; }
		public bool HasError { get; internal set; }

		internal readonly List<IDisposable> _disposables = new();

		public void Dispose() {
			if (IsDisposed) {
				return;
			}
			IsDisposed = true;
			IsDisposeing?.Invoke(this);
			Task.Run(async () => {
				foreach (var item in _disposables) {
					item.Dispose();
				}
				assetSession.Dispose();
				try {
					worldManager.RemoveWorld(this);
				}
				catch { }
				try {
					if (!IsLoading) {
						GetLocalUser()?.userRoot.Target?.Entity.Dispose();
						if (_netManager is not null) {
							for (var i = 0; i < 10; i++) {
								_netManager.PollEvents();
								await Task.Delay(100);
							}
						}
					}
				}
				catch { }
				if (IsNetworked) {
					if (!HasError) {
						try {
							await Engine.netApiManager.Client.LeaveSession(Guid.Parse(SessionID.Value));
						}
						catch { }
					}
				}
				try {
					_netManager?.DisconnectAll();
				}
				catch { }
				GC.Collect();
			});
		}
	}
}
