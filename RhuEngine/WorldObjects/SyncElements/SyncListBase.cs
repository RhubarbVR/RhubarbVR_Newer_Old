using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public abstract class SyncListBase<T> : SyncObject, INetworkedObject, IEnumerable<ISyncObject> , IChangeable where T : ISyncObject
	{
		private readonly SynchronizedCollection<T> _syncObjects = new(5);

		public event Action<IChangeable> Changed;
		public T this[int i] => _syncObjects[i];

		public int Count => _syncObjects.Count;

		public bool NoSync { get; set; }

		private readonly object _locker = new();
		public void RemoveAtIndex(int index) {
			_syncObjects[index].Destroy();
		}

		public void AddInternal(T newElement) {
			var offsetindex = 0;
			if (typeof(IOffsetableElement).IsAssignableFrom(newElement.GetType())) {
				var offset = (IOffsetableElement)newElement;
				offset.OffsetChanged += ReorderList;
				offsetindex = offset.Offset;
			}
			lock (_locker) {
				var hasAdded = false;
				for (var i = 0; i < _syncObjects.Count; i++) {
					var elementOffset = 0;
					if(_syncObjects[i] is IOffsetableElement ielementOffset){
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
		}


		public void RemoveInternal(object value) {
			((T)value).OnDispose -= RemoveInternal;
			lock (_locker) {
				_syncObjects.Remove((T)value);
			}
			Changed?.Invoke(this);
		}

		private void ReorderList() {
			var newOrder = from e in _syncObjects.AsParallel()
						   orderby (typeof(IOffsetableElement).IsAssignableFrom(e.GetType()) ? ((IOffsetableElement)e).Offset : 0) ascending
						   select e;
			var index = 0;
			foreach (var item in newOrder) {
				item.ChangeName(index.ToString());
				_syncObjects.Remove(item);
				_syncObjects.Insert(index, item);
			}
			Changed?.Invoke(this);
		}

		internal void BroadcastAdd(T data) {
			if (NoSync) {
				return;
			}
			var sendData = new DataNodeGroup();
			sendData.SetValue("type", new DataNode<int>(1));
			sendData.SetValue("ElementData", SaveElement(data));
			World.BroadcastDataToAll(Pointer, sendData, LiteNetLib.DeliveryMethod.ReliableOrdered);
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
				default:
					break;
			}
		}

		public override void InitializeMembers(bool networkedObject, bool deserialize, Func<NetPointer> func) {
		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			throw new NotImplementedException();
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			throw new NotImplementedException();
		}

		IEnumerator<ISyncObject> IEnumerable<ISyncObject>.GetEnumerator() {
			foreach (var item in _syncObjects) {
				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable<IWorldObject>)this).GetEnumerator();
		}


	}
}
