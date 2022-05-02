
using RhuEngine.DataStructure;

namespace RhuEngine.WorldObjects
{
	public abstract class SyncStream : SyncObject, INetworkedObject
	{
		public readonly Sync<string> name;

		public bool NoSync { get; set; }

		public abstract void Received(Peer sender, IDataNode data);

		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return SyncObjectSerializerObject.CommonSerialize(this);
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.BindPointer((DataNodeGroup)data, this);
		}

	}
}
