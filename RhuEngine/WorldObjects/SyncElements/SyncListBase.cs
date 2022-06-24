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
	public abstract class SyncListBase<T> : SyncObject, INetworkedObject, IEnumerable<ISyncObject> , IChangeable, ISyncMember where T : ISyncObject
	{
		private readonly SynchronizedCollection<T> _syncObjects = new(5);

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

		public T this[NetPointer pointer] => _syncObjects.Where((val)=> val.Pointer == pointer).First();

		public int Count => _syncObjects.Count;

		public bool NoSync { get; set; }

		private readonly object _locker = new();
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

		private void FixAllNames() {
			for (var i = 0; i < Count; i++) {
				_syncObjects[i].ChangeName(i.ToString());
			}
		}

		private void NewElement_OnDispose(object obj) {
			if(obj is T castedObject) {
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
			lock (_locker) {
				_syncObjects.Remove(value);
				FixAllNames();
			}
			Changed?.Invoke(this);
		}

		private void ReorderList() {
			if (IsRemoved) {
				return;
			}
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
					var objecte = this[(DataNode<NetPointer>)nodeGroup["ref"]];
					RLog.Info("Removed net");
					RemoveInternal(objecte);
					objecte.Destroy();
					break;
				case 3:
					foreach (var item in _syncObjects) {
						item.Dispose();
					}
					_syncObjects.Clear();
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
			lock (_syncObjects.SyncRoot) {
				for (var i = 0; i < _syncObjects.Count; i++) {
					yield return _syncObjects[i];
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable<IWorldObject>)this).GetEnumerator();
		}
		public override void Dispose() {
			base.Dispose();
			for (var i = 0; i < _syncObjects.Count; i++) {
				_syncObjects[i].IsDestroying = true;
				_syncObjects[i].Dispose();
			}
			_syncObjects.Clear();
		}

	}
}
