using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using RhuEngine.Datatypes;
using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public sealed partial class World
	{
		private readonly ConcurrentDictionary<NetPointer, IWorldObject> _worldObjects = new();

		private readonly ConcurrentDictionary<NetPointer, INetworkedObject> _networkedObjects = new();

		private readonly HashSet<Entity> _entities = new();

		private readonly HashSet<Entity> _updatingEntities = new();

		private readonly List<LinkedWorldComponent> _worldLinkComponents = new();

		private readonly HashSet<IGlobalStepable> _globalStepables = new();

		private readonly object _buildRefIDLock = new();

		public ulong ItemIndex { get; private set; } = 1;
		[Exposed]
		public int EntityCount => _entities.Count;
		[Exposed]
		public int UpdatingEntityCount => _updatingEntities.Count;
		[Exposed]
		public int RenderingComponentsCount => _worldLinkComponents.Count;
		[Exposed]
		public int GlobalStepableCount => _globalStepables.Count;

		public IWorldObject[] AllWorldObjects => _worldObjects.Values.ToArray();

		[Exposed]
		public int WorldObjectsCount => _worldObjects.Count;
		[Exposed]
		public int NetworkedObjectsCount => _networkedObjects.Count;
		public NetPointer NextRefID() {
			NetPointer netPointer;
			lock (_buildRefIDLock) {
				netPointer = NetPointer.BuildID(ItemIndex, LocalUserID);
				ItemIndex++;
			}
			return netPointer;
		}

		public NetPointer NextLocalRefID() {
			NetPointer netPointer;
			lock (_buildRefIDLock) {
				netPointer = NetPointer.BuildID(ItemIndex, 0);
				ItemIndex++;
			}
			return netPointer;
		}

		public void RegisterWorldObject(IWorldObject worldObject) {
			if (!_worldObjects.TryAdd(worldObject.Pointer, worldObject)) {
				RLog.Warn($"World Object Failed To add {worldObject.Pointer.HexString()} typeof {worldObject.GetType().GetFormattedName()}");
			}
			else {
				if (typeof(INetworkedObject).IsAssignableFrom(worldObject.GetType())) {
					if (!_networkedObjects.TryAdd(worldObject.Pointer, (INetworkedObject)worldObject)) {
						RLog.Warn($"INetworkedObject Failed To add {worldObject.Pointer.HexString()} typeof {worldObject.GetType().GetFormattedName()}");
					}
				}
			}
		}
		public void UnRegisterWorldObject(IWorldObject worldObject) {
			if (!_worldObjects.TryRemove(worldObject.Pointer, out _)) {
				RLog.Warn($"World Object Failed To remove {worldObject.Pointer.HexString()} typeof {worldObject.GetType().GetFormattedName()}");
			}
			else {
				if (typeof(INetworkedObject).IsAssignableFrom(worldObject.GetType())) {
					if (!_networkedObjects.TryRemove(worldObject.Pointer, out _)) {
						RLog.Warn($"INetworkedObject Failed To remove {worldObject.Pointer.HexString()} typeof {worldObject.GetType().GetFormattedName()}");
					}
				}
			}
		}
		public IWorldObject GetWorldObject(NetPointer value) {
			return _worldObjects.TryGetValue(value, out var data) ? data : null;
		}

		public void RegisterGlobalStepable(IGlobalStepable data) {
			lock (_globalStepables) {
				_globalStepables.Add(data);
			}
		}
		public void UnregisterGlobalStepable(IGlobalStepable data) {
			lock (_globalStepables) {
				_globalStepables.Remove(data);
			}
		}

		public void RegisterWorldLinkObject(LinkedWorldComponent data) {
			lock (_worldLinkComponents) {
				_worldLinkComponents.Add(data);
			}
		}
		public void UnregisterWorldLinkObject(LinkedWorldComponent data) {
			lock (_worldLinkComponents) {
				_worldLinkComponents.Remove(data);
			}
		}

		public void RegisterEntity(Entity data) {
			lock (_entities) {
				_entities.Add(data);
			}
		}
		public void UnregisterEntity(Entity data) {
			lock (_entities) {
				_entities.Remove(data);
			}
		}

		public void RegisterUpdatingEntity(Entity data) {
			lock (_updatingEntities) {
				_sortEntitys = true;
				_updatingEntities.Add(data);
			}
		}
		public void UnregisterUpdatingEntity(Entity data) {
			lock (_updatingEntities) {
				_sortEntitys = true;
				_updatingEntities.Remove(data);
			}
		}

	}
}
