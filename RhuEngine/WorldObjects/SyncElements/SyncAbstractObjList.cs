using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface IAbstractObjList : ISyncObject {

	}

	public interface IAbstractObjList<T> : IAbstractObjList
	{
		T Add(Type type, bool networkedObject = false, bool deserialize = false);
	}
	public sealed partial class SyncAbstractObjList<T> : SyncListBase<T>, IAbstractObjList<T>, INetworkedObject, IEnumerable<ISyncObject>, ISyncMember where T : class, ISyncObject
	{
		public W Add<W>(bool networkedObject = false, bool deserialize = false) where W : T, new() {
			return (W)Add(typeof(W), networkedObject, deserialize);
		}

		public T Add(Type type, bool networkedObject = false, bool deserialize = false) {
			if (!typeof(T).IsAssignableFrom(type)) {
				throw new Exception($"Unable to add unassignable type {type.FullName} to a list of {typeof(T).FullName}");
			}
			var newElement = (T)Activator.CreateInstance(type);
			newElement.Initialize(World, this, "List element", networkedObject, deserialize);
			AddInternal(newElement);
			if (!networkedObject) {
				BroadcastAdd(newElement);
				if (!deserialize) {
					newElement.CallFirstCreation();
				}
			}
			return newElement;
		}
		protected override void InitializeMembers(bool networkedObject, bool deserialize, NetPointerUpdateDelegate func) {
		}
		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			_hasBeenNetSynced |= syncObjectSerializerObject.NetSync;
			return syncObjectSerializerObject.CommonAbstractListSerialize(this, this);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.AbstractListDeserialize((DataNodeGroup)data, this);
		}

		public override T LoadElement(IDataNode data) {
			var nodeGroup = (DataNodeGroup)data;
			var typeName = nodeGroup.GetValue<string>("fieldType");
			var type = Type.GetType(typeName);
			if (type is not null) {
				if (!typeof(T).IsAssignableFrom(type)) {
					throw new Exception($"Unable to add unassignable type {type.FullName} to a list of {typeof(T).FullName}");
				}
				var objrc = (T)Activator.CreateInstance(type);
				objrc.Initialize(World, this, "List element", true, true);
				var deserlizer = new SyncObjectDeserializerObject(false);
				objrc.Deserialize(nodeGroup.GetValue("ElementData"), deserlizer);
				foreach (var item in deserlizer.onLoaded) {
					item?.Invoke();
				}
				return objrc;
			}
			else {
				throw new Exception($"Failed to load received type {typeName}");
			}
		}

		public override IDataNode SaveElement(T val) {
			var sendData = new DataNodeGroup();
			sendData.SetValue("fieldType", new DataNode<string>(val.GetType().FullName));
			sendData.SetValue("ElementData", val.Serialize(new SyncObjectSerializerObject(true))); //Todo exsperment with not sending element data
			return sendData;
		}
	}
}
