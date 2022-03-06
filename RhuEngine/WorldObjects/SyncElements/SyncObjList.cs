using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface ISyncObjectList<T> : ISyncObject
	{
		public T Add(bool networkedObject = false, bool deserialize = false);
	}

	public class SyncValueList<T> : SyncObjList<Sync<T>>
	{
		public new T this[int index]
		{
			get => base[index].Value;
			set => base[index].Value = value;
		}

		public override void OnAddedElement(Sync<T> element) {
			element.Changed += ChildElementOnChanged;
		}


		public override void OnElementRemmoved(Sync<T> element) {
			element.Changed -= ChildElementOnChanged;
		}

		public void Add(T value) {
			Add().Value = value;
		}

		public void Append(IEnumerable<T> values) {
			foreach (var item in values) {
				Add(item);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T[](SyncValueList<T> data) => data.ToArray().Select((objec) => ((Sync<T>)objec).Value).ToArray();
	}

	public class SyncObjList<T> : SyncListBase<T>, ISyncObjectList<T>, INetworkedObject, IEnumerable<ISyncObject> where T : ISyncObject, new()
	{
		public T AddWithCustomRefIds(bool networkedObject = false, bool deserialize = false,Func<NetPointer> func = null) {
			var newElement = new T();
			newElement.Initialize(World, this, "List Elemenet", networkedObject, deserialize, func);
			AddInternal(newElement);
			return newElement;
		}

		public T Add(bool networkedObject = false, bool deserialize = false) {
			var newElement = new T();
			newElement.Initialize(World, this, "List Elemenet", networkedObject, deserialize);
			if (!networkedObject) {
				BroadcastAdd(newElement);
				if (!deserialize) {
					newElement.FirstCreation();
				}
			}
			AddInternal(newElement);
			return newElement;
		}

		public override void InitializeMembers(bool networkedObject, bool deserialize, Func<NetPointer> func) {
		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return syncObjectSerializerObject.CommonListSerialize(this, this);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.ListDeserialize((DataNodeGroup)data, this);
		}

		public override T LoadElement(IDataNode data) {
			var newElement = new T();
			newElement.Initialize(World, this, "List Elemenet", true, false);
			newElement.Deserialize(data, new SyncObjectDeserializerObject(false));
			return newElement;
		}

		public override IDataNode SaveElement(T val) {
			return val.Serialize(new SyncObjectSerializerObject(true));
		}
	}
}
