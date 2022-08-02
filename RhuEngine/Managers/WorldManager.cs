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

namespace RhuEngine.Managers
{
	public class WorldManager : IManager
	{
		public Action OnWorldUpdateTaskBar;
		public Engine Engine { get; private set; }

		public SynchronizedCollection<World> worlds = new();

		public World PrivateOverlay { get; private set; }

		public World LocalWorld { get; private set; }

		private World _focusedWorld;

		public World FocusedWorld { get => _focusedWorld; set { _focusedWorld = value; FocusedWorldChange(); } }

		public bool SaveLocalWorld { get; set; } = true;

		public float TotalStepTime { get; private set; }

		public PrivateSpaceManager PrivateSpaceManager { get; internal set; }

		private readonly Stopwatch _stepStopwatch = new();

		private void FocusedWorldChange() {
		}

		public void Dispose() {
			if (SaveLocalWorld) {
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
			return Task.Run(() => {
				_isRunning.Push(world);
				while (world.IsLoading && !world.IsDisposed) {
					Thread.Sleep(100);
				}
				if (world.IsDisposed) {
					RLog.Err($"Failed to start world {world.WorldDebugName} Error {world.LoadMsg}");
					Thread.Sleep(3000);
					_isRunning.Pop();
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
			if (focusLevel != World.FocusLevel.PrivateOverlay) {
				world.RootEntity.AttachComponent<ClipBoardImport>();
			}
			world.SessionName.Value = sessionName;
			if ((focusLevel != World.FocusLevel.PrivateOverlay) && !localWorld) {
				Task.Run(() => world.StartNetworking(true));
			}
			else {
				world.WaitingForWorldStartState = false;
			}
			worlds.Add(world);
			OnWorldUpdateTaskBar?.Invoke();
			ShowLoadingFeedback(world, focusLevel);
			return world;
		}

		public World JoinNewWorld(string sessionID, World.FocusLevel focusLevel, string sessionName = null) {
			var world = new World(this) {
				Focus = World.FocusLevel.Background
			};
			world.Initialize(true, true, true, false);
			world.SessionID.SetValueNoOnChangeAndNetworking(sessionID);
			world.SessionName.SetValueNoOnChangeAndNetworking(sessionName);
			Task.Run(() => world.StartNetworking(false));
			worlds.Add(world);
			OnWorldUpdateTaskBar?.Invoke();
			ShowLoadingFeedback(world, focusLevel);
			return world;
		}
		
		public World LoadWorldFromBytes(World.FocusLevel focusLevel, byte[] data, bool localWorld = false) {
			return LoadWorldFromDataNodeGroup(focusLevel,(DataNodeGroup)new DataReader(data).Data, localWorld);
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
				Task.Run(() => world.StartNetworking(true));
			}
			worlds.Add(world);
			ShowLoadingFeedback(world, focusLevel);
			OnWorldUpdateTaskBar?.Invoke();
			return world;
		}

		public void Init(Engine engine) {
			Engine = engine;
			Engine.IntMsg = "Creating Personal Space";
			PrivateOverlay = CreateNewWorld(World.FocusLevel.PrivateOverlay);
			Engine.IntMsg = "Done Creating Personal Space";
			PrivateOverlay.RootEntity.AddChild("PrivateSpace").AttachComponent<PrivateSpaceManager>();
			Engine.IntMsg = "Creating Local World";
			LocalWorld = CreateNewWorld(World.FocusLevel.Focused, true);
			Engine.IntMsg = "Loading Local World Data";
			LocalWorld.SessionName.Value = "Local World";
			LocalWorld.WorldName.Value = "Local World";
			Engine.IntMsg = "Building Local World";
			LocalWorld.BuildLocalWorld();
			Engine.IntMsg = "Local World Made";
			while (LocalWorld.IsLoading) {
				Engine.IntMsg = LocalWorld.LoadMsg;
				Thread.Sleep(10);
			}
			engine.netApiManager.HasGoneOfline += NetApiManager_HasGoneOfline;
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
			for (var i = worlds.Count - 1; i >= 0; i--) {
				try {
					worlds[i].RenderStep();
				}
				catch (Exception ex) {
					RLog.Err($"Failed to render step world {worlds[i].WorldDebugName}. Error: {ex}");
				}
			}
			UpdateJoinMessage();
		}

		private Vector3f _oldPlayerPos = Vector3f.Zero;
		private Vector3f _loadingPos = Vector3f.Zero;
		private void UpdateJoinMessage() {
			try {
				if (_isRunning.Count != 0) {
					var world = _isRunning.Peek();
					var textpos = Matrix.T(Vector3f.Forward * 0.25f) * RInput.Head.HeadMatrix;
					var playerPos = RRenderer.CameraRoot.Translation;
					_loadingPos += playerPos - _oldPlayerPos;
					_loadingPos += (textpos.Translation - _loadingPos) * Math.Min(RTime.Elapsedf * 5f, 1);
					_oldPlayerPos = playerPos;
					if (world.IsLoading && !world.IsDisposed) {
						//RText.Add($"Loading World: \n{world.LoadMsg}", Matrix.TR(_loadingPos, Quaternionf.LookAt(Engine.EngineLink.CanInput ? RInput.Head.Position : Vector3f.Zero, _loadingPos)));
					}
					else {
						//RText.Add($"Failed to load world{(Engine.netApiManager.User?.UserName == null ? ", JIM": "")}\nError {world.LoadMsg}", Matrix.TR(_loadingPos, Quaternionf.LookAt(Engine.EngineLink.CanInput ? RInput.Head.Position : Vector3f.Zero, _loadingPos)));
					}
				}
			}
			catch (Exception ex) {
				RLog.Err("Failed to update joining msg text Error: " + ex.ToString());
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
	}
}
