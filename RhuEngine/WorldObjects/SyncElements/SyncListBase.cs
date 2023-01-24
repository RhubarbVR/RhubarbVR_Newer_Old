using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public interface ISyncList : INetworkedObject, IEnumerable<ISyncObject>, IChangeable, ISyncMember
	{
		int Count { get; }
		object Lock { get; }

		event Action OnReorderList;

		void DestroyAtIndex(int index);
		void DisposeAtIndex(int index);
	}
	public abstract class SyncListBase<T> : SyncObject, ISyncList, INetworkedObject, IEnumerable<ISyncObject>, IChangeable, ISyncMember where T : class, ISyncObject
	{
		private readonly List<T> _syncObjects = new(5);

		public event Action<IChangeable> ChildChanged;

		private void UpdateChildChanged(IChangeable changeable) {
			ChildChanged?.Invoke(changeable);
		}

		public event Action<IChangeable> Changed;
		[Exposed]
		public int IndexOf(T value) {
			return _syncObjects.IndexOf(value);
		}

		public void ChildElementOnChanged(IChangeable changeable) {
			Changed?.Invoke(changeable);
		}
		[Exposed]
		public T GetValue(int index) {
			return _syncObjects[index];
		}
		[Exposed]
		public void Clear() {
			var sendData = new DataNodeGroup();
			sendData.SetValue("type", new DataNode<int>(3));
			World.BroadcastDataToAll(this, sendData, LiteNetLib.DeliveryMethod.ReliableOrdered);
			foreach (var item in _syncObjects.ToArray()) {
				item.Dispose();
			}
			_syncObjects.Clear();
		}
		[Exposed]
		public T this[int i] => _syncObjects[i];

		public T this[NetPointer pointer] {
			get {
				lock (Lock) {
					foreach (var item in _syncObjects) {
						if(item.Pointer== pointer) {
							return item;
						}
					}
					return null;
				}
			}
		}

		public int Count => _syncObjects.Count;

		public bool NoSync { get; set; }

		public object Lock => _syncObjects;

		public void DisposeAtIndex(int index) {
			_syncObjects[index].Dispose();
		}
		public void DestroyAtIndex(int index) {
			_syncObjects[index].Destroy();
		}
		public virtual void OnAddedElement(T element) {

		}

		public virtual void OnElementRemmoved(T element) {

		}
		public void AddInternal(T newElement) {
			OnAddedElement(newElement);
			newElement.OnDispose += NewElement_OnDispose;
			var offsetindex = 0;
			if (typeof(IOffsetableElement).IsAssignableFrom(newElement.GetType())) {
				var offset = (IOffsetableElement)newElement;
				offset.OffsetChanged += ReorderList;
				offsetindex = offset.Offset;
			}
			if (typeof(IChangeable).IsAssignableFrom(newElement.GetType())) {
				var offset = (IChangeable)newElement;
				offset.Changed += UpdateChildChanged;
			}
			lock (_syncObjects) {
				var hasAdded = false;
				for (var i = 0; i < _syncObjects.Count; i++) {
					var elementOffset = 0;
					if (_syncObjects[i] is IOffsetableElement ielementOffset) {
						elementOffset = ielementOffset.Offset;
					}
					if (!hasAdded) {
						if (offsetindex > elementOffset) {
							_syncObjects.Insert(i, newElement);
							newElement.ChangeName(i.ToString());
							for (var e = 0; e < _syncObjects.Count - i; e++) {
								_syncObjects[e + i].ChangeName((e + i).ToString());
							}
							hasAdded = true;
						}
					}
				}
				if (!hasAdded) {
					newElement.ChangeName(Count.ToString());
					_syncObjects.Add(newElement);
				}
			}
			Changed?.Invoke(this);
			OnReorderList?.Invoke();
		}

		private void FixAllNames() {
			lock (_syncObjects) {
				for (var i = 0; i < Count; i++) {
					_syncObjects[i].ChangeName(i.ToString());
				}
			}
		}

		private void NewElement_OnDispose(object obj) {
			if (obj is T castedObject) {
				RemoveInternal(castedObject);
				BroadcastRemove(castedObject);
			}
		}

		public void RemoveInternal(T value) {
			if (IsRemoved) {
				return;
			}
			OnElementRemmoved(value);
			value.OnDispose -= NewElement_OnDispose;
			if (typeof(IChangeable).IsAssignableFrom(value.GetType())) {
				var offset = (IChangeable)value;
				offset.Changed -= UpdateChildChanged;
			}
			lock (_syncObjects) {
				_syncObjects.Remove(value);
				FixAllNames();
			}
			Changed?.Invoke(this);
			OnReorderList?.Invoke();
		}

		public event Action OnReorderList;

		private void ReorderList() {
			if (IsRemoved) {
				return;
			}
			lock (_syncObjects) {				
				var newOrder = _syncObjects.OrderBy(x => typeof(IOffsetableElement).IsAssignableFrom(x.GetType()) ? ((IOffsetableElement)x).Offset : 0).ThenBy(x => x.Pointer._id).ToArray();
				for (int i = 0; i < newOrder.Length; i++) {
					newOrder[i].ChangeName(i.ToString());
					_syncObjects.Remove(newOrder[i]);
					_syncObjects.Insert(i, newOrder[i]);
				}
			}
			Changed?.Invoke(this);
			OnReorderList?.Invoke();
		}

		internal void BroadcastAdd(T data) {
			if (IsRemoved || IsDestroying) {
				return;
			}
			if (NoSync) {
				return;
			}
			var sendData = new DataNodeGroup();
			sendData.SetValue("type", new DataNode<int>(1));
			sendData.SetValue("ElementData", SaveElement(data));
			World.BroadcastDataToAll(this, sendData, LiteNetLib.DeliveryMethod.ReliableOrdered);
		}

		internal void BroadcastRemove(T data) {
			if (IsRemoved || IsDestroying) {
				return;
			}
			if (NoSync) {
				return;
			}
			var sendData = new DataNodeGroup();
			sendData.SetValue("type", new DataNode<int>(2));
			sendData.SetValue("ref", new DataNode<NetPointer>(data.Pointer));
			World.BroadcastDataToAll(this, sendData, LiteNetLib.DeliveryMethod.ReliableOrdered);
		}

		public abstract T LoadElement(IDataNode data);

		public abstract IDataNode SaveElement(T val);

		public void Received(Peer sender, IDataNode data) {
			if (NoSync) {
				return;
			}
			var nodeGroup = (DataNodeGroup)data;
			switch (((DataNode<int>)nodeGroup.GetValue("type")).Value) {
				case 1:
					AddInternal(LoadElement(nodeGroup["ElementData"]));
					break;
				case 2:
					var targetID = ((DataNode<NetPointer>)nodeGroup["ref"]).Value;
					var objecte = this[targetID];
					if(objecte is null) {
						RLog.Err($"Did not have value in list ID:{targetID}");
						var worldobject = World.GetWorldObject((DataNode<NetPointer>)nodeGroup["ref"]);
						if (worldobject is not null) {
							RLog.Info($"Try to fix error by: removeing world object ID:{targetID}");
							worldobject.Dispose();
						}
						else {
							RLog.Info($"Object also was not in the world ID:{targetID}");
						}
					}
					else {
						RLog.Info($"Removed net ID:{targetID}");
						RemoveInternal(objecte);
						objecte.Destroy();
					}
					break;
				case 3:
					lock (_syncObjects) {
						var startAmount = _syncObjects.Count;
						for (var i = 0; i < startAmount; i++) {
							_syncObjects[0].IsDestroying = true;
							_syncObjects[0].Dispose();
						}
					}
					break;
				default:
					break;
			}
		}

		protected override void InitializeMembers(bool networkedObject, bool deserialize, NetPointerUpdateDelegate func) {
		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			throw new NotImplementedException();
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			throw new NotImplementedException();
		}

		IEnumerator<ISyncObject> IEnumerable<ISyncObject>.GetEnumerator() {
			for (var i = 0; i < _syncObjects.Count; i++) {
				yield return _syncObjects[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable<IWorldObject>)this).GetEnumerator();
		}
		public override void Dispose() {
			IsDestroying = true;
			lock (_syncObjects) {
				var startAmount = _syncObjects.Count;
				Changed = null;
				ChildChanged = null;
				for (var i = 0; i < startAmount; i++) {
					_syncObjects[0].IsDestroying = true;
					_syncObjects[0].Dispose();
				}
				if (_syncObjects.Count != 0) {
					RLog.Err($"Sync object list {Pointer} did not remove all data so might ram leak");
					_syncObjects.Clear();
				}
			}
			base.Dispose();
			GC.SuppressFinalize(this);
		}

	}
}
