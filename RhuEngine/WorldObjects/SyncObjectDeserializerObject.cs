using System;
using System.Collections.Generic;
using System.Reflection;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.WorldObjects
{
	public class SyncObjectDeserializerObject
	{
		public List<Action> onLoaded = new();
		public bool hasNewRefIDs = false;
		public Dictionary<ulong, ulong> newRefIDs = new();
		public Dictionary<ulong, List<Action<NetPointer>>> toReassignLater = new();

		public SyncObjectDeserializerObject(bool hasNewRefIDs) {
			this.hasNewRefIDs = hasNewRefIDs;
		}

		public void BindPointer(DataNodeGroup data, IWorldObject @object) {
			if (hasNewRefIDs) {
				if (newRefIDs == null) {
					Log.Warn($"Problem with {@object.GetType().FullName}");
				}
				newRefIDs.Add(((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID(), @object.Pointer.GetID());
				if (toReassignLater.ContainsKey(((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID())) {
					foreach (var func in toReassignLater[((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID()]) {
						func.Invoke(@object.Pointer);
					}
				}
			}
			else {
				@object.Pointer = ((DataNode<NetPointer>)data.GetValue("Pointer")).Value;
				if (@object.Pointer.id == new NetPointer(0).id) {
					Log.Warn($"RefID of {@object.GetType().FullName} is null");
				}
				else {
					@object.World.RegisterWorldObject(@object);
				}
			}
		}

		public void RefDeserialize(DataNodeGroup data, ISyncRef @object) {
			if (data == null) {
				throw new Exception("Node did not exist when loading SyncRef");
			}
			@object.NetValue = ((DataNode<NetPointer>)data.GetValue("targetPointer")).Value;
			if (hasNewRefIDs) {
				newRefIDs.Add(((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID(), @object.Pointer.GetID());
				if (toReassignLater.ContainsKey(((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID())) {
					foreach (var func in toReassignLater[((DataNode<NetPointer>)data.GetValue("Pointer")).Value.GetID()]) {
						func(@object.Pointer);
					}
				}
				if (newRefIDs.ContainsKey(@object.NetValue.GetID())) {
					@object.NetValue = new NetPointer(newRefIDs[@object.NetValue.GetID()]);
				}
				else {
					if (!toReassignLater.ContainsKey(@object.NetValue.GetID())) {
						toReassignLater.Add(@object.NetValue.GetID(), new List<Action<NetPointer>>());
					}
					toReassignLater[@object.NetValue.GetID()].Add((value) => @object.NetValue = value);
				}

			}
			else {
				@object.Pointer = ((DataNode<NetPointer>)data.GetValue("Pointer")).Value;
				@object.World.RegisterWorldObject(@object);
			}
			onLoaded.Add(@object.OnLoaded);
		}

		public T ValueDeserialize<T>(DataNodeGroup data, IWorldObject @object) {
			if (data == null) {
				throw new Exception($"Node did not exist when loading Sync value {@object.GetType().FullName}");
			}
			BindPointer(data, @object);
			if (typeof(ISyncObject).IsAssignableFrom(@object.GetType())) {
				onLoaded.Add(((ISyncObject)@object).OnLoaded);
			}
			return typeof(T) == typeof(Type)
				? (T)(object)Type.GetType(((DataNode<string>)data.GetValue("Value")).Value, false, false)
				: typeof(T).IsEnum ? (T)(object)((DataNode<int>)data.GetValue("Value")).Value : ((DataNode<T>)data.GetValue("Value")).Value;
		}

		public void ListDeserialize<T>(DataNodeGroup data, ISyncObjectList<T> @object) where T : ISyncObject, new() {
			if (data == null) {
				throw new Exception("Node did not exist when loading SyncObjList");
			}
			BindPointer(data, @object);
			foreach (DataNodeGroup val in (DataNodeList)data.GetValue("list")) {
				@object.Add(!hasNewRefIDs, true).Deserialize(val, this);
			}
			if (typeof(ISyncObject).IsAssignableFrom(@object.GetType())) {
				onLoaded.Add(@object.OnLoaded);
			}
		}

		public void AbstractListDeserialize<T>(DataNodeGroup data, IAbstractObjList<T> @object) where T : ISyncObject {
			if (data == null) {
				throw new Exception("Node did not exist when loading SyncAbstractObjList");
			}
			BindPointer(data, @object);
			foreach (DataNodeGroup val in (DataNodeList)data.GetValue("list")) {
				var ty = Type.GetType(((DataNode<string>)val.GetValue("Type")).Value);
				if (ty == typeof(MissingComponent)) {
					ty = Type.GetType(((DataNode<string>)((DataNodeGroup)val.GetValue("Value")).GetValue("type")).Value, false);
					if (ty == null) {
						Log.Warn("Component still not found " + ((DataNode<string>)val.GetValue("Type")).Value);
						@object.Add(typeof(MissingComponent), !hasNewRefIDs, true).Deserialize((DataNodeGroup)val.GetValue("Value"), this);
					}
					else if (ty != typeof(MissingComponent)) {
						if (ty.IsAssignableFrom(typeof(T))) {
							@object.Add(ty, !hasNewRefIDs, true).Deserialize((DataNodeGroup)((DataNodeGroup)val.GetValue("Value")).GetValue("Data"), this);
						}
						else {
							Log.Err("Something is broken or someone is messing with things", true);
						}
					}
					else {
						@object.Add(ty, !hasNewRefIDs, true).Deserialize((DataNodeGroup)val.GetValue("Value"), this);
					}
				}
				else {
					if (ty == null) {
						Log.Warn($"Type {((DataNode<string>)val.GetValue("Type")).Value} not found", true);
						if (typeof(T) == typeof(Component)) {
							@object.Add(typeof(MissingComponent), !hasNewRefIDs, true).Deserialize((DataNodeGroup)val.GetValue("Value"), this);
						}
					}
					else {
						@object.Add(ty, !hasNewRefIDs, true).Deserialize((DataNodeGroup)val.GetValue("Value"), this);
					}
				}
			}
			if (typeof(ISyncObject).IsAssignableFrom(@object.GetType())) {
				onLoaded.Add(((ISyncObject)@object).OnLoaded);
			}
		}

		public void Deserialize(DataNodeGroup data, IWorldObject @object) {
			if (data == null) {
				throw new Exception("Node did not exist when loading Node: " + @object.GetType().FullName);
			}
			BindPointer(data,@object);
			var fields = @object.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields) {
				if (typeof(IWorldObject).IsAssignableFrom(field.FieldType) && ((field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0) || (!hasNewRefIDs && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0)))) {
					if (((IWorldObject)field.GetValue(@object)) == null) {
						throw new Exception($"Sync not initialized on field {field.Name} of {@object.GetType().FullName}");
					}
					try {
						var filedData = (DataNodeGroup)data.GetValue(field.Name);
						if (filedData is null) {
							if (field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0) {
								((ISyncObject)field.GetValue(@object)).Deserialize(filedData, this);
							}
						}
						else {
							((ISyncObject)field.GetValue(@object)).Deserialize(filedData, this);
						}
					}
					catch (Exception e) {
						throw new Exception($"Failed to deserialize field {field.Name}", e);
					}
				}
			}
			if (typeof(ISyncObject).IsAssignableFrom(@object.GetType())) {
				onLoaded.Add(((ISyncObject)@object).OnLoaded);
			}
		}
	}
}