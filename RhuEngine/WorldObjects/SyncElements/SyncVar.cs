using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public class SyncVar : SyncObject, INetworkedObject {
		private Type _type;
		public Type Type
		{
			get => _type;
			set {
				if (!typeof(INetworkedObject).IsAssignableFrom(value)) {
					throw new InvalidOperationException("Has to be a INetworkedObject");
				}
				_type = value;
				var newElement = (INetworkedObject)Activator.CreateInstance(value);
				newElement.Initialize(World, this, "Sync Var Element", false, false);
				Target = newElement;
				if (!NoSync) {
					var sendData = new DataNodeGroup();
					sendData.SetValue("fieldType", new DataNode<string>(value.FullName));
					sendData.SetValue("ElementData", Target.Serialize(new SyncObjectSerializerObject(true)));
					World.BroadcastDataToAll(this, sendData, LiteNetLib.DeliveryMethod.ReliableOrdered);
				}
			}
		}
		[NoSync]
		[NoShow]
		[NoSave]
		[NoLoad]
		public INetworkedObject Target { get; private set; }
		public bool NoSync { get; set; }

		public T GetTarget<T>(out bool Failed) where T : class, INetworkedObject {
			try {
				Failed = false;
				return (T)Target;
			}
			catch {
				Failed = true;
				return null;
			}
		}

		public T SetTarget<T>(out bool Failed) where T : class, INetworkedObject {
			Type = typeof(T); 
			return GetTarget<T>(out Failed);
		}

		public void Received(Peer sender, IDataNode data) {
			var nodeGroup = (DataNodeGroup)data;
			var typeName = nodeGroup.GetValue<string>("fieldType");
			var type = Type.GetType(typeName);
			if (type is not null) {
				if (!typeof(INetworkedObject).IsAssignableFrom(type)) {
					throw new Exception($"Unable to add unassignable type {type.FullName} to Sync Var");
				}
				var objrc = (INetworkedObject)Activator.CreateInstance(type);
				objrc.Initialize(World, this, "Sync Var Element", true, false);
				objrc.Deserialize(nodeGroup.GetValue("ElementData"), new SyncObjectDeserializerObject(false));
				Target =  objrc;
			}
			else {
				throw new Exception($"Failed to load received type {typeName}");
			}
		}

		public override IDataNode Serialize(SyncObjectSerializerObject syncObjectSerializerObject) {
			var data = SyncObjectSerializerObject.CommonSerialize(this);
			data.SetValue("fieldType", new DataNode<string>(_type?.FullName??"Null"));
			if (Target != null) {
				data.SetValue("ElementData", Target.Serialize(syncObjectSerializerObject)); 
			}
			return data;
		}

		public override void Deserialize(IDataNode data, SyncObjectDeserializerObject syncObjectSerializerObject) {
			var nodeGroup = (DataNodeGroup)data;
			syncObjectSerializerObject.Deserialize(nodeGroup, this);
			var typeName = nodeGroup.GetValue<string>("fieldType");
			var type = Type.GetType(typeName);
			if (type is not null) {
				try {
					if (!typeof(INetworkedObject).IsAssignableFrom(type)) {
						throw new Exception($"Unable to add unassignable type {type.FullName} to Sync Var");
					}
					var value = nodeGroup.GetValue("ElementData");
					if (value is null) {
						return;
					}
					var objrc = (INetworkedObject)Activator.CreateInstance(type);
					objrc.Initialize(World, this, "Sync Var Element", true, false);
					objrc.Deserialize(value, syncObjectSerializerObject);
					Target = objrc;
				}
				catch(Exception e) {
					throw new Exception($"Failed to load type {typeName} ",e);
				}
			}
		}
	}
}
