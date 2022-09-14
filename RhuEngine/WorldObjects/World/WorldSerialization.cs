﻿using RhuEngine.DataStructure;

namespace RhuEngine.WorldObjects
{
	public sealed partial class World
	{
		public bool IsDeserializing { get; internal set; } = true;

		public DataNodeGroup Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			return syncObjectSerializerObject.CommonWorkerSerialize(this);
		}


		public void Deserialize(DataNodeGroup data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			syncObjectSerializerObject.Deserialize(data, this);
			IsDeserializing = false;
		}
	}
}
