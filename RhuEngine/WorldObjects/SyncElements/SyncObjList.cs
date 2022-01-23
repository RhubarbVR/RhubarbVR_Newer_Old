using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface ISyncObjectList<T> : ISyncObject
	{
		public T Add(bool networkedObject = false, bool deserialize = false);
	}
	public class SyncObjList<T> : SyncObject, ISyncObjectList<T>, INetworkedObject, IEnumerable<ISyncObject> where T : ISyncObject, new()
	{
		private readonly SynchronizedCollection<T> _syncObjects = new(5);

		public T this[int i] => _syncObjects[i];

		public int Count => _syncObjects.Count;

		public bool NoSync { get; set; }

		public void AddInternal(T newElement) {
			_syncObjects.Add(newElement);
			if (typeof(IOffsetableElement).IsAssignableFrom(newElement.GetType())) {
				var offset = (IOffsetableElement)newElement;
				offset.OffsetChanged += ReorderList;
			}
			ReorderList();
		}


		public void RemoveInternal(T value) {
			_syncObjects.Remove(value);
		}

		public T AddWithCustomRefIds(bool networkedObject = false, bool deserialize = false,Func<NetPointer> func = null) {
			var newElement = new T();
			newElement.Initialize(World, this, _syncObjects.Count.ToString(), networkedObject, deserialize, func);
			AddInternal(newElement);
			return newElement;
		}

		public T Add(bool networkedObject = false, bool deserialize = false) {
			var newElement = new T();
			newElement.Initialize(World, this, _syncObjects.Count.ToString(), networkedObject, deserialize);
			if (!networkedObject) {
				BroadcastAdd(newElement);
			}
			AddInternal(newElement);
			return newElement;
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
		}
		private void BroadcastAdd(T data) {
			if (NoSync) {
				return;
			}
			var sendData = new DataNodeGroup();
			sendData.SetValue("type", new DataNode<int>(1));
			sendData.SetValue("ElementData", data.Serialize(new SyncObjectSerializerObject(true)));
			World.BroadcastDataToAll(Pointer, sendData, LiteNetLib.DeliveryMethod.ReliableOrdered);
		}

		public void Received(Peer sender, IDataNode data) {
			if (NoSync) {
				return;
			}
			var nodeGroup = (DataNodeGroup)data;
			switch (((DataNode<int>)nodeGroup.GetValue("type")).Value) {
				case 1:
					var objrc = Add(true);
					objrc.Deserialize(nodeGroup.GetValue("ElementData"), new SyncObjectDeserializerObject(false));
					break;
				default:
					break;
			}
		}

		public override void InitializeMembers(bool networkedObject, bool deserialize, Func<NetPointer> func) {
		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return syncObjectSerializerObject.CommonListSerialize(this, this);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.ListDeserialize((DataNodeGroup)data, this);
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
