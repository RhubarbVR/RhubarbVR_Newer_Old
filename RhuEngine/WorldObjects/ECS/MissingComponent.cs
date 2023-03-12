using System;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;

namespace RhuEngine.WorldObjects.ECS
{
	[HideCategory]
	public sealed partial class MissingComponent : Component
	{
		public readonly Sync<string> type;

		public string tempType;

		public DataNodeGroup tempData;

		protected override void OnLoaded() {
			type.Value = tempType;
		}

		public override IDataNode Serialize(SyncObjectSerializerObject workerSerializerObject) {
			var obj = new DataNodeGroup();
			if (Persistence) {
				var Refid = new DataNode<NetPointer>(Pointer);
				obj.SetValue("Pointer", Refid);
				obj.SetValue("Data", tempData);
				var typeValue = new DataNode<string>(type.Value);
				obj.SetValue("type", typeValue);
			}
			return obj;
		}

		public override void Deserialize(IDataNode notCastedData, SyncObjectDeserializerObject syncObjectDeserializer) {
			// TODO: refactor this. go to your corner. think REALLY hard about what you've done.
			var data = (DataNodeGroup)notCastedData;
			if (data == null) {
				RLog.Warn("Node did not exist when loading Node");
				return;
			}
			if (syncObjectDeserializer.hasNewRefIDs) {
				if (syncObjectDeserializer.newRefIDs == null) {
					Console.WriteLine("Problem with " + GetType().FullName);
				}
				syncObjectDeserializer.newRefIDs.Add(((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID(), Pointer.GetID());
				if (syncObjectDeserializer.toReassignLater.ContainsKey(((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID())) {
					foreach (var func in syncObjectDeserializer.toReassignLater[((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID()]) {
						func(Pointer);
					}
				}
			}
			else {
				Pointer = ((DataNode<NetPointer>)data.GetValue("Pointer")).Value;
				World.RegisterWorldObject(this);
			}
			if (((DataNode<string>)data.GetValue("type")) != null) {
				tempType = ((DataNode<string>)data.GetValue("type")).Value;
				tempData = (DataNodeGroup)data.GetValue("Data");
			}
			else {
				tempData = data;
			}
			OnLoaded();
		}

	}
}
