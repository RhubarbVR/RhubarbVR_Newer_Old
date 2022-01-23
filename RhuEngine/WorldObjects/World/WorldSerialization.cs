using RhuEngine.DataStructure;

namespace RhuEngine.WorldObjects
{
	public partial class World
	{
		public bool IsDeserializing { get; private set; } = true;

		public DataNodeGroup Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return syncObjectSerializerObject.CommonWorkerSerialize(this);
		}


		public void Deserialize(DataNodeGroup data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.Deserialize(data, this);
			IsDeserializing = false;
		}
	}
}
