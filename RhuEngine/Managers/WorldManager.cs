using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.DataStructure;
using RhuEngine.WorldObjects;

using SharedModels;
using SharedModels.GameSpecific;

using RNumerics;
using RhuEngine.Linker;
using DataModel.Enums;
using Esprima;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Managers
{
	public sealed class WorldManager : IManager
	{
		public Action OnWorldUpdateTaskBar;
		public Engine Engine { get; private set; }

		public SynchronizedCollection<World> worlds = new();

		public World PrivateOverlay { get; private set; }
		public World OverlayWorld { get; private set; }
		public World LocalWorld { get; private set; }

		private World _focusedWorld;

		public World FocusedWorld { get => _focusedWorld; set { _focusedWorld = value; FocusedWorldChange(); } }

		public bool SaveLocalWorld { get; set; } = true;
		public bool LoadLocalWorld { get; set; } = true;

		public float TotalStepTime { get; private set; }

		public PrivateSpaceManager PrivateSpaceManager { get; internal set; }

		private readonly Stopwatch _stepStopwatch = new();

		public event Action<World> WorldChanged;

		private void FocusedWorldChange() {
			WorldChanged?.Invoke(FocusedWorld);
		}

		public void Dispose() {
			if (SaveLocalWorld) {
				RLog.Info("Saving Local World");
				try {
					var data = LocalWorld.Serialize(new SyncObjectSerializerObject(false));
					File.WriteAllBytes(Engine.BaseDir + "LocalWorldTest.RWorld", new DataSaver(data).SaveStore());
				}
				catch (Exception ex) {
					RLog.Err($"Failed to save world {ex}");
				}
			}
			for (var i = worlds.Count - 1; i >= 0; i--) {
				try {
					worlds[i].Dispose();
				}
				catch (Exception ex) {
					RLog.Err($"Failed to dispose world {worlds[i].WorldDebugName}. Error: {ex}");
				}
			}
			OnWorldUpdateTaskBar?.Invoke();
		}

		private readonly Stack<World> _isRunning = new();

		private Task ShowLoadingFeedback(World world, World.FocusLevel focusLevel) {
			return Task.Run(async () => {
				_isRunning.Push(world);
				while (world.IsLoading && !world.IsDisposed) {
					await Task.Delay(100);
				}
				if (world.IsDisposed || world.HasError) {
					RLog.Err($"Failed to start world {world.WorldDebugName} Error {world.LoadMsg}");
					await Task.Delay(3000);
					_isRunning.Pop();
					world.Dispose();
				}
				else {
					RLog.Info($"Done loading world {world.WorldDebugName}");
					world.Focus = focusLevel;
					_isRunning.Pop();
				}
			});
		}

		public World GetWorldBySessionID(Guid sessionID) {
			for (var i = worlds.Count - 1; i >= 0; i--) {
				try {
					if (worlds[i].SessionID.Value == sessionID.ToString()) {
						return worlds[i];
					}
				}
				catch {
				}
			}
			return null;
		}

		public World CreateNewWorld(World.FocusLevel focusLevel, bool localWorld = false, string sessionName = null) {
			var world = new World(this) {
				Focus = World.FocusLevel.Background
			};
			world.Initialize(!localWorld, false, false, focusLevel == World.FocusLevel.PrivateOverlay);
			world.RootEntity.name.Value = "Root";
			world.RootEntity.AttachComponent<SimpleSpawn>();
			world.SessionName.Value = sessionName;
			if ((focusLevel != World.FocusLevel.PrivateOverlay) && !localWorld) {
				Task.Run(async () => await world.StartNetworking(true));
			}
			else {
				world.WaitingForWorldStartState = false;
			}
			worlds.Add(world);
			OnWorldUpdateTaskBar?.Invoke();
			ShowLoadingFeedback(world, focusLevel);
			return world;
		}

		public World CreateNewWorld(World.FocusLevel focusLevel, string sessionName, AccessLevel accessLevel, int maxUsers, bool isHiden, Guid? assosiatedGroup = null) {
			var world = new World(this) {
				Focus = World.FocusLevel.Background
			};
			world.Initialize(true, false, false, false);
			world.RootEntity.name.Value = "Root";
			world.RootEntity.AttachComponent<SimpleSpawn>();
			world.SessionName.Value = sessionName;
			world.AssociatedGroup.Value = assosiatedGroup?.ToString();
			world.AccessLevel.Value = accessLevel;
			world.MaxUserCount.Value = maxUsers;
			world.IsHidden.Value = isHiden;
			Task.Run(async () => await world.StartNetworking(true));
			worlds.Add(world);
			OnWorldUpdateTaskBar?.Invoke();
			ShowLoadingFeedback(world, focusLevel);
			return world;
		}

		public World JoinNewWorld(Guid sessionID, World.FocusLevel focusLevel, string sessionName = null) {
			var world = new World(this) {
				Focus = World.FocusLevel.Background
			};
			world.Initialize(true, true, true, false);
			world.SessionID.SetValueNoOnChangeAndNetworking(sessionID.ToString());
			world.SessionName.SetValueNoOnChangeAndNetworking(sessionName);
			Task.Run(async () => await world.StartNetworking(false));
			worlds.Add(world);
			OnWorldUpdateTaskBar?.Invoke();
			ShowLoadingFeedback(world, focusLevel);
			return world;
		}

		public World LoadWorldFromBytes(World.FocusLevel focusLevel, byte[] data, bool localWorld = false) {
			return LoadWorldFromDataNodeGroup(focusLevel, (DataNodeGroup)new DataReader(data).Data, localWorld);
		}
		public World LoadWorldFromDataNodeGroup(World.FocusLevel focusLevel, DataNodeGroup data, bool localWorld = false) {
			var world = new World(this) {
				Focus = World.FocusLevel.Background
			};
			world.Initialize(!localWorld, false, true, focusLevel == World.FocusLevel.PrivateOverlay);
			var loader = new SyncObjectDeserializerObject(true);
			world.Deserialize(data, loader);
			foreach (var item in loader.onLoaded) {
				item?.Invoke();
			}
			if ((focusLevel != World.FocusLevel.PrivateOverlay) & !localWorld) {
				Task.Run(async () => await world.StartNetworking(true));
			}
			worlds.Add(world);
			ShowLoadingFeedback(world, focusLevel);
			OnWorldUpdateTaskBar?.Invoke();
			return world;
		}
		private TextLabel3D _loadingText;
		public void Init(Engine engine) {
			Engine = engine;
			Engine.IntMsg = "Creating Personal Space World";
			PrivateOverlay = CreateNewWorld(World.FocusLevel.PrivateOverlay);
			Engine.IntMsg = "Creating Private Space Manager";
			PrivateOverlay.RootEntity.AddChild("PrivateSpace").AttachComponent<PrivateSpaceManager>();
			Engine.IntMsg = "Creating Loading Text";
			_loadingText = PrivateOverlay.RootEntity.AddChild("LoadingText").AttachComponent<TextLabel3D>();
			//_loadingText.Size.Value = 0.35f; Todo fix size
			Engine.IntMsg = "Creating Overlay World";
			OverlayWorld = CreateNewWorld(World.FocusLevel.Overlay, true);
			Engine.IntMsg = "Creating Overlay World Manager";
			OverlayWorld.RootEntity.AddChild("OverlayWorldManager").AttachComponent<OverlayWorldManager>();
			Engine.IntMsg = "Loading Local World";
			var loaddedData = false;
			if (LoadLocalWorld && File.Exists(Engine.BaseDir + "LocalWorldTest.RWorld")) {
				try {
					Engine.IntMsg = "Loading Local World From File ";
					LocalWorld = LoadWorldFromBytes(World.FocusLevel.Focused, File.ReadAllBytes(Engine.BaseDir + "LocalWorldTest.RWorld"), true);
					LocalWorld.SessionName.Value = "Local World";
					LocalWorld.WorldName.Value = "Local World";
					Engine.IntMsg = "Loaded Local World From File";
					loaddedData = true;
				}
				catch {
					loaddedData = false;
					Engine.IntMsg = "Failed loading Local World From Flie ";
					RLog.Err("Failed loading Local World From Flie");
				}
			}
			if (!loaddedData) {
				RLog.Info("Building Local World");
				Engine.IntMsg = "Making Local World";
				LocalWorld = CreateNewWorld(World.FocusLevel.Focused, true);
				Engine.IntMsg = "Loading Local World Data";
				LocalWorld.SessionName.Value = "Local World";
				LocalWorld.WorldName.Value = "Local World";
				Engine.IntMsg = "Building Local World";
				LocalWorld.BuildLocalWorld();
				Engine.IntMsg = "Local World Made";
			}
			while (LocalWorld.IsLoading) {
				Engine.IntMsg = LocalWorld.LoadMsg;
				Thread.Sleep(10);
			}
			engine.netApiManager.Client.HasGoneOfline += NetApiManager_HasGoneOfline;
		}

		private void NetApiManager_HasGoneOfline() {
			if (LocalWorld != null) {
				LocalWorld.Focus = World.FocusLevel.Focused;
			}
			foreach (var item in worlds) {
				if (!(item.IsPersonalSpace || LocalWorld == item)) {
					Task.Run(() => item.Dispose());
				}
			}
			OnWorldUpdateTaskBar?.Invoke();
		}
		public void Step() {
			float totalStep = 0;
			for (var i = worlds.Count - 1; i >= 0; i--) {
				try {
					_stepStopwatch.Restart();
					worlds[i].Step();
				}
				catch (Exception ex) {
					RLog.Err($"Failed to game step world {worlds[i].WorldDebugName}. Error: {ex}");
				}
				finally {
					_stepStopwatch.Stop();
					worlds[i].stepTime = (float)_stepStopwatch.Elapsed.TotalSeconds;
					totalStep += (float)_stepStopwatch.Elapsed.TotalSeconds;
				}
			}
			TotalStepTime = totalStep;
		}

		public void RenderStep() {
			var hasRan = false;
			for (var i = worlds.Count - 1; i >= 0; i--) {
				if ((worlds[i].IsOverlayWorld || worlds[i].IsPersonalSpace) && !hasRan) {
					hasRan = true;
					UpdateCameraPos();
				}
				try {
					worlds[i].RenderStep();
				}
				catch (Exception ex) {
					RLog.Err($"Failed to render step world {worlds[i].WorldDebugName}. Error: {ex}");
				}
			}
			UpdateJoinMessage();
		}

		private Vector3f _loadingPos = Vector3f.Zero;

		private void UpdateJoinMessage() {
			if (_loadingText is null) {
				return;
			}
			if (PrivateOverlay.GetLocalUser() is null) {
				return;
			}
			_loadingText.Entity.enabled.Value = _isRunning.Count != 0;
			try {
				if (_isRunning.Count != 0) {
					var world = _isRunning.Peek();
					var textpos = Matrix.T(Vector3f.Forward * 0.35f) * Matrix.T(0, -0.1f, 0) * Engine.inputManager.ScreenHeadMatrix;
					_loadingPos += (textpos.Translation - _loadingPos) * Math.Min(RTime.Elapsedf * 3.5f, 1);
					var userPOS = PrivateOverlay.GetLocalUser().userRoot.Target?.Entity.GlobalTrans ?? Matrix.Identity;
					if (world.IsLoading && !world.IsDisposed) {
						_loadingText.Text.Value = $"{Engine.localisationManager.GetLocalString("Common.LoadingWorld")}\n {Engine.localisationManager.GetLocalString(world.LoadMsg)}";
						_loadingText.Entity.GlobalTrans = Matrix.R(Quaternionf.Yawed180) * Matrix.TR(_loadingPos, Quaternionf.LookAt(Engine.EngineLink.CanInput ? Engine.inputManager.ScreenHeadMatrix.Translation : Vector3f.Zero, _loadingPos)) * userPOS;
					}
					if (!world.HasError) {
						_loadingText.Text.Value = $"{Engine.localisationManager.GetLocalString("Common.LoadedWorld")}";
						_loadingText.Entity.GlobalTrans = Matrix.R(Quaternionf.Yawed180) * Matrix.TR(_loadingPos, Quaternionf.LookAt(Engine.EngineLink.CanInput ? Engine.inputManager.ScreenHeadMatrix.Translation : Vector3f.Zero, _loadingPos)) * userPOS;
					}
					else {
						var errorMsg = world.IsNetworked && world.IsJoiningSession
							? Engine.localisationManager.GetLocalString("Common.FailedToJoinWorld")
							: Engine.localisationManager.GetLocalString("Common.FailedToLoadWorld");
						_loadingText.Text.Value = $"{errorMsg} {(Engine.netApiManager.Client.User?.UserName == null ? ", JIM" : "")}\n {Engine.localisationManager.GetLocalString(world.LoadMsg)}";
						_loadingText.Entity.GlobalTrans = Matrix.R(Quaternionf.Yawed180) * Matrix.TR(_loadingPos, Quaternionf.LookAt(Engine.EngineLink.CanInput ? Engine.inputManager.ScreenHeadMatrix.Translation : Vector3f.Zero, _loadingPos)) * userPOS;
					}
				}
			}
			catch (Exception ex) {
				RLog.Err("Failed to update joining msg text Error: " + ex.ToString());
			}
		}

		private void UpdateCameraPos() {
			if (FocusedWorld?.GetLocalUser()?.userRoot.Target is not null) {
				if (FocusedWorld is null) {
					return;
				}
				var focusUserRoot = FocusedWorld.GetLocalUser().userRoot.Target;
				var Entity = PrivateOverlay.GetLocalUser()?.userRoot.Target?.Entity;
				var EntityTwo = OverlayWorld.GetLocalUser()?.userRoot.Target?.Entity;
				if (Engine.EngineLink.CanRender) {
					RRenderer.CameraRoot = focusUserRoot.Entity.GlobalTrans;
				}
				CopyPosToWorld(Entity, focusUserRoot);
				CopyPosToWorld(EntityTwo, focusUserRoot);
			}
		}

		private void CopyPosToWorld(Entity Entity, UserRoot focusUserRoot) {
			if (Entity is null) {
				return;
			}
			if (focusUserRoot is not null) {
				Entity.GlobalTrans = focusUserRoot.Entity.GlobalTrans;
			}
		}

		public void RemoveWorld(World world) {
			if (FocusedWorld == world) {
				LocalWorld.Focus = World.FocusLevel.Focused;
			}
			worlds.Remove(world);
			world.Dispose();
			OnWorldUpdateTaskBar?.Invoke();
		}

		public void WorldCycling() {
			RLog.Info("WorldCycling");
			var currentIndex = worlds.IndexOf(FocusedWorld) + 1;
			for (var i = currentIndex; i < worlds.Count; i++) {
				if (worlds[i].Focus is World.FocusLevel.Background or World.FocusLevel.Focused) {
					worlds[i].Focus = World.FocusLevel.Focused;
					return;
				}
			}
			for (var i = 0; i < worlds.Count; i++) {
				if (worlds[i].Focus is World.FocusLevel.Background or World.FocusLevel.Focused) {
					worlds[i].Focus = World.FocusLevel.Focused;
					return;
				}
			}
		}
	}
}
