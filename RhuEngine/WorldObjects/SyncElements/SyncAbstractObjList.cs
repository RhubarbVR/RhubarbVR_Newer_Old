using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface IAbstractObjList<T> : ISyncObject
	{
		T Add(Type type, bool networkedObject = false, bool deserialize = false);
	}
	public class SyncAbstractObjList<T> : SyncObject, IAbstractObjList<T>, INetworkedObject, IChangeable, IEnumerable<ISyncObject> where T : ISyncObject
	{
		private readonly SynchronizedCollection<T> _syncObjects = new(5);

		public event Action<IChangeable> Changed;

		public int Count => _syncObjects.Count;

		public bool NoSync { get; set; }

		public T this[int i] => _syncObjects[i];

		public void AddInternal(T newElement) {
			_syncObjects.Add(newElement);
			newElement.OnDispose += RemoveInternal;
			if (typeof(IOffsetableElement).IsAssignableFrom(newElement.GetType())) {
				var offset = (IOffsetableElement)newElement;
				offset.OffsetChanged += ReorderList;
			}
			// TODO: optimize this. don't reorder on every element add.
			ReorderList();
		}

		public void RemoveAtIndex(int index) {
			_syncObjects[index].Destroy();
		}

		public void RemoveInternal(object value) {
			((T)value).OnDispose -= RemoveInternal;
			_syncObjects.Remove((T)value);
		}

		public W Add<W>(bool networkedObject = false, bool deserialize = false) where W : T, new() {
			return (W)Add(typeof(W), networkedObject, deserialize);
		}

		public T Add(Type type, bool networkedObject = false, bool deserialize = false) {
			if (!typeof(T).IsAssignableFrom(type)) {
				throw new Exception($"Unable to add unassignable type {type.FullName} to a list of {typeof(T).FullName}");
			}
			var newElement = (T)Activator.CreateInstance(type);
			newElement.Initialize(World, this, (_syncObjects.Count - 1).ToString(), networkedObject, deserialize);
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
			Changed?.Invoke(this);
		}

		private void BroadcastAdd(T data) {
			if (NoSync) {
				return;
			}
			var sendData = new DataNodeGroup();
			sendData.SetValue("type", new DataNode<int>(1));
			sendData.SetValue("fieldType", new DataNode<string>(data.GetType().FullName));
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
					var typeName = nodeGroup.GetValue<string>("fieldType");
					var type = Type.GetType(typeName);
					if (type is not null) {
						var objrc = Add(type, true);
						objrc.Deserialize(nodeGroup.GetValue("ElementData"), new SyncObjectDeserializerObject(false));
					}
					else {
						throw new Exception($"Failed to load received type {typeName}");
					}
					break;
				default:
					break;
			}
		}
		public override void InitializeMembers(bool networkedObject, bool deserialize,Func<NetPointer> func) {
		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return syncObjectSerializerObject.CommonAbstractListSerialize(this, this);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.AbstractListDeserialize((DataNodeGroup)data, this);
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
