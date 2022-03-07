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

using StereoKit;

using World = RhuEngine.WorldObjects.World;

namespace RhuEngine.Managers
{
	public class WorldManager : IManager
	{
		public Engine Engine { get; private set; }

		public SynchronizedCollection<World> worlds = new();

		public World PrivateOverlay { get; private set; }

		public World LocalWorld { get; private set; }

		private World _focusedWorld;

		public World FocusedWorld { get => _focusedWorld; set { _focusedWorld = value; FocusedWorldChange(); } }

		public bool SaveLocalWorld { get; set; } = true;

		public float TotalStepTime { get; private set; }

		private readonly Stopwatch _stepStopwatch = new();

		private void FocusedWorldChange() {
		}

		public void Dispose() {
			if (SaveLocalWorld) {
				var data = LocalWorld.Serialize(new SyncObjectSerializerObject(false));
				var json = MessagePack.MessagePackSerializer.ConvertToJson(data.GetByteArray(), Serializer.Options);
				File.WriteAllText(Engine.BaseDir + "LocalWorldTest.json", json);
			}
			for (var i = worlds.Count - 1; i >= 0; i--) {
				try {
					worlds[i].Dispose();
				}
				catch (Exception ex) {
					Log.Err($"Failed to dispose world {worlds[i].WorldDebugName}. Error: {ex}");
				}
			}
		}

		private readonly Stack<World> _isRunngin = new();

		private Task ShowLoadingFeedback(World world, World.FocusLevel focusLevel) {
			return Task.Run(() => {
				_isRunngin.Push(world);
				while (world.IsLoading && !world.IsDisposed) {
					Thread.Sleep(100);
				}
				if (world.IsDisposed) {
					Log.Err($"Failed to start world {world.WorldDebugName}");
					Thread.Sleep(3000);
					_isRunngin.Pop();
				}
				else {
					Log.Info($"Done loading world {world.WorldDebugName}");
					world.Focus = focusLevel;
					_isRunngin.Pop();
				}
			});
		}

		public World CreateNewWorld(World.FocusLevel focusLevel, bool localWorld = false,string sessionName = null) {
			var world = new World(this) {
				Focus = World.FocusLevel.Background
			};
			world.Initialize(!localWorld, false, false, focusLevel == World.FocusLevel.PrivateOverlay);
			world.RootEntity.name.Value = "Root";
			world.RootEntity.AttachComponent<SimpleSpawn>();
			if(focusLevel != World.FocusLevel.PrivateOverlay) {
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
			ShowLoadingFeedback(world, focusLevel);
			return world;
		}

		public World JoinNewWorld(string sessionID, World.FocusLevel focusLevel,string sessionName = null) {
			var world = new World(this) {
				Focus = World.FocusLevel.Background
			};
			world.Initialize(true, true, true, false);
			world.SessionID.SetValueNoOnChangeAndNetworking(sessionID);
			world.SessionName.SetValueNoOnChangeAndNetworking(sessionName);
			Task.Run(() => world.StartNetworking(false));
			worlds.Add(world);
			ShowLoadingFeedback(world, focusLevel);
			return world;
		}


		public World LoadWorldFromJson(World.FocusLevel focusLevel, string json, bool localWorld = false) {
			return LoadWorldFromDataNodeGroup(focusLevel, new DataNodeGroup(json), localWorld);
		}

		public World LoadWorldFromBytes(World.FocusLevel focusLevel, byte[] data, bool localWorld = false) {
			return LoadWorldFromDataNodeGroup(focusLevel, new DataNodeGroup(data), localWorld);
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
			return world;
		}

		public void Init(Engine engine) {
			Engine = engine;
			PrivateOverlay = CreateNewWorld(World.FocusLevel.PrivateOverlay);
			PrivateOverlay.RootEntity.AddChild("PrivateSpace").AttachComponent<PrivateSpaceManager>();
			LocalWorld = CreateNewWorld(World.FocusLevel.Focused, true);
			LocalWorld.SessionName.Value = "Local World";
			LocalWorld.WorldName.Value = "Local World";
			LocalWorld.BuildLocalWorld();
			LocalWorld.SessionName.Value = "LocalWorld";
			engine.netApiManager.HasGoneOfline += NetApiManager_HasGoneOfline;
		}

		private void NetApiManager_HasGoneOfline() {
			if(LocalWorld != null) {
				LocalWorld.Focus = World.FocusLevel.Focused;
			}
			foreach (var item in worlds) {
				if(!(item.IsPersonalSpace || LocalWorld == item)) {
					Task.Run(() => item.Dispose());
				}
			}
		}
		public Vec3 LoadingPos = Vec3.One;
		public void Step() {
			float totalStep = 0;
			for (var i = worlds.Count - 1; i >= 0; i--) {
				try {
					_stepStopwatch.Restart();
					worlds[i].Step();
				}
				catch (Exception ex) {
					Log.Err($"Failed to step world {worlds[i].WorldDebugName}. Error: {ex}");
				}
				finally {
					_stepStopwatch.Stop();
					worlds[i].stepTime = (float)_stepStopwatch.Elapsed.TotalSeconds;
					totalStep += (float)_stepStopwatch.Elapsed.TotalSeconds;
				}
			}

			try {
				if (_isRunngin.Count != 0) {
					var world = _isRunngin.Peek();
					var textpos = Matrix.T(Vec3.Forward * 0.25f) * Input.Head.ToMatrix();
					LoadingPos = LoadingPos + ((textpos.Translation - LoadingPos) * Math.Min(Time.Elapsedf * 5f, 1));
					if (world.IsLoading && !world.IsDisposed) {
						Text.Add($"Loading World: \n{world.LoadMsg}", new Pose(LoadingPos, Quat.LookAt(LoadingPos, Input.Head.position)).ToMatrix());
					}
					else {
						Text.Add($"World is dead {Engine.netApiManager.User?.UserName ?? "JIM"}", new Pose(LoadingPos, Quat.LookAt(LoadingPos, Input.Head.position)).ToMatrix());
					}
				}
			}
			catch (Exception ex) {
				Log.Err("Failed to update joining msg text Error: " + ex.ToString());
			}

			TotalStepTime = totalStep;
		}

		public void RemoveWorld(World world) {
			if (FocusedWorld == world) {
				LocalWorld.Focus = World.FocusLevel.Focused;
			}
			worlds.Remove(world);
			world.Dispose();
		}
	}
}
