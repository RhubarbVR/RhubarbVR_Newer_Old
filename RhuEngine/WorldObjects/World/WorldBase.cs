using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using RhuEngine.Datatypes;
using RhuEngine.Components;
using BepuPhysics;
using BepuUtilities.Memory;
using RhuEngine.Physics;
using System.Numerics;

namespace RhuEngine.WorldObjects
{
	public sealed partial class World : IWorldObject
	{

		public bool IsLoading => (IsDeserializing || IsLoadingNet) & !HasError;
		public bool IsOverlayWorld => worldManager.OverlayWorld == this;

		public bool IsPersonalSpace { get; private set; }

		public readonly WorldManager worldManager;
		public Engine Engine => worldManager.Engine;

		public PhysicsSimulation PhysicsSimulation { get; private set; }
		//constructor is added at code Gen
		public World(WorldManager worldManager) : this() {
			this.worldManager = worldManager;
		}

		public string LoadMsg = "Starting to LoadWorld";

		public void Initialize(bool networkedWorld, bool networkedObject, bool deserialize, bool isPersonalSpace) {
			IsPersonalSpace = isPersonalSpace;
			try {
				PhysicsSimulation = new PhysicsSimulation();
				PhysicsSimulation.Init(this);
			}
			catch (Exception e) {
				RLog.Err("Failed to start PhysicsSimulation for world Error:" + e.ToString());
				throw;
			}
			try {
				//Method added at code Gen
				InitializeMembers(networkedObject, deserialize, null);
			}
			catch (Exception ex) {
				RLog.Err("Failed to InitializeMembers" + ex.ToString());
				throw new Exception("Failed to InitializeMembers", ex);
			}
			StartTime.Value = DateTime.UtcNow;
			WorldGravity.Value = new Vector3f(0, -10, 0);
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

		internal FocusLevel _startFocus = FocusLevel.Background;


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

		[NoSyncUpdate]
		public readonly Sync<string> WorldID;

		[NoSave]
		[Default("New Session")]
		[OnChanged(nameof(SessionInfoChanged))]
		public readonly Sync<string> SessionName;

		[NoSave]
		[Default("")]
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

		public readonly Sync<Vector3f> WorldGravity;

		[NoSyncUpdate]
		[NoSave]
		public readonly SyncObjList<User> Users;

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
		public double stepTime = 0f;


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

		public GrabbableHolder GetGrabHolder(Handed handed) {
			return handed switch {
				Handed.Left => LeftGrabbableHolder,
				Handed.Right => RightGrabbableHolder,
				Handed.Max => HeadGrabbableHolder,
				_ => null,
			};
		}
		public void RenderStep() {
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
						if (!ent.IsDestroying) {
							ent.RenderStep();
						}
					}
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to update entities for session {WorldDebugName}. Error: {e}");
			}
			try {
				if (Engine.EngineLink.CanRender) {
					foreach (var item in _worldLinkComponents) {
						item.RunRender();
					}
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to build render queue for session {WorldDebugName}. Error {e}");
			}
		}

		private readonly List<IGrouping<uint, Entity>> _sorrtedEntity = new();

		private bool _sortEntitys;

		public void Step() {
			_netManager?.NatPunchModule.PollEvents();
			WorldThreadSafty.MethodCalls = 0;
			if (IsLoading | (_focus == FocusLevel.Background)) {
				return;
			}
			try {
				PhysicsSimulation.Update(RTime.Elapsed);
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
						if (!ent.IsDestroying) {
							ent.Step();
						}
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

		private IDisposable[] _disposables = Array.Empty<IDisposable>();

		public void AddDisposable(IDisposable disposable) {
			lock (disposable) {
				Array.Resize(ref _disposables, _disposables.Length + 1);
				_disposables[_disposables.Length - 1] = disposable;
			}
		}

		public void Dispose() {
			if (IsDisposed) {
				return;
			}
			IsDisposed = true;
			IsDisposeing?.Invoke(this);
			Task.Run(async () => {
				RLog.Info($"Closeing Session");
				try {
					foreach (var item in _disposables.ToArray()) {
						if (item is SyncObject @object) {
							@object.IsDestroying = true;
						}
						item?.Dispose();
					}
					_disposables = Array.Empty<IDisposable>();
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
					catch (Exception e) {
						RLog.Err($"Failed to CleanUP User On leave Session Error:{e}");
					}
					if (IsNetworked) {
						try {
							await Engine.netApiManager.Client.LeaveSession(Guid.Parse(SessionID.Value));
						}
						catch (Exception e) {
							RLog.Err($"Failed to leave Session Error:{e}");
						}
						try {
							_netManager?.DisconnectAll();
							_netManager?.Stop();
							_netManager = null;
						}
						catch (Exception e) {
							RLog.Err($"Failed to CleanUp Nat Error:{e}");
						}
					}
					try {
						worldManager.RemoveWorld(this);
					}
					catch { }
					var data = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
					foreach (var item in data) {
						if (typeof(SyncObject).IsAssignableFrom(item.FieldType)) {
							item.SetValue(this, null);
						}
					}
					PhysicsSimulation?.Dispose();
					PhysicsSimulation = null;
					GC.Collect();
					RLog.Info($"Session Closed");
				}
				catch (Exception e) {
					RLog.Err($"Session Failed to Close Error:{e}");
				}
			});
		}
	}
}
