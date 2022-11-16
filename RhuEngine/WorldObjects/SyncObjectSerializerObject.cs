using System;
using System.Collections.Generic;
using System.Reflection;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;

namespace RhuEngine.WorldObjects
{
	public sealed class SyncObjectSerializerObject
	{
		public bool NetSync { get; private set; }

		public SyncObjectSerializerObject(bool netSync) {
			NetSync = netSync;
		}

		public static DataNodeGroup CommonSerialize(IWorldObject @object) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			return obj;
		}

		public DataNodeGroup CommonAbstractListSerialize(IWorldObject @object, IEnumerable<ISyncObject> worldObjects) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			var list = new DataNodeList();
			foreach (var val in worldObjects) {
				if (!val.IsRemoved) {
					var tip = val.Serialize(this);
					var listobj = new DataNodeGroup();
					if (tip != null) {
						listobj.SetValue("Value", tip);
					}
					//Need To add Constant Type Strings for better compression 
					listobj.SetValue("Type", new DataNode<string>(val.GetType().FullName));
					list.Add(listobj);
				}
			}
			obj.SetValue("list", list);
			return obj;
		}

		public DataNodeGroup CommonListSerialize(IWorldObject @object, IEnumerable<ISyncObject> worldObjects) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			var list = new DataNodeList();
			foreach (var val in worldObjects) {
				if (!val.IsRemoved) {
					var tip = val.Serialize(this);
					if (tip != null) {
						list.Add(tip);
					}
				}
			}
			obj.SetValue("list", list);
			return obj;
		}

		public static DataNodeGroup CommonRefSerialize(IWorldObject @object, NetPointer target) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			var Value = new DataNode<NetPointer>(target);
			obj.SetValue("targetPointer", Value);
			return obj;
		}
		public static DataNodeGroup CommonValueSerialize<T>(IWorldObject @object, T value) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			var inputType = typeof(T);
			IDataNode Value;
			if (inputType == typeof(Type)) {
				Value = new DataNode<string>(((Type)(object)value)?.FullName);
			}
			else if (inputType == typeof(Uri)) {
				Value = new DataNode<string>(((Uri)(object)value)?.ToString());
			}
			else {
				if (inputType.IsEnum) {
					var unType = inputType.GetEnumUnderlyingType();
					if (unType == typeof(int)) {
						Value = new DataNode<int>((int)(object)value);
					}
					else if (unType == typeof(uint)) {
						Value = new DataNode<uint>((uint)(object)value);
					}
					else if (unType == typeof(bool)) {
						Value = new DataNode<bool>((bool)(object)value);
					}
					else if (unType == typeof(byte)) {
						Value = new DataNode<byte>((byte)(object)value);
					}
					else if (unType == typeof(sbyte)) {
						Value = new DataNode<sbyte>((sbyte)(object)value);
					}
					else if (unType == typeof(short)) {
						Value = new DataNode<short>((short)(object)value);
					}
					else if (unType == typeof(ushort)) {
						Value = new DataNode<ushort>((ushort)(object)value);
					}
					else if (unType == typeof(long)) {
						Value = new DataNode<long>((long)(object)value);
					}
					else if (unType == typeof(ulong)) {
						Value = new DataNode<ulong>((ulong)(object)value);
					}
					else {
						throw new NotSupportedException();
					}
				}
				else {
					Value = new DataNode<T>(value);
				}
			}
			obj.SetValue("Value", Value);
			return obj;
		}

		struct SerializeFunction
		{
			internal ISyncObject _syncObject;

			internal DataNodeGroup _parrentData;

			internal string _name;
		}

		readonly List<SerializeFunction> _serializeFunctions = new();

		bool _firstCall = false;
		public DataNodeGroup CommonWorkerSerialize(IWorldObject @object) {
			var localFirstCall = false;
			if (!_firstCall) {
				localFirstCall = true;
				_firstCall = true;
			}
			var fields = @object.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			DataNodeGroup obj = null;
			if (@object.Persistence || NetSync) {
				if (!NetSync) {
					if (typeof(ISyncObject).IsAssignableFrom(@object.GetType())) {
						var castObject = @object as ISyncObject;
						try {
							castObject.RunOnSave();
						}
						catch (Exception e) {
							RLog.Warn($"Failed to save {@object.GetType().GetFormattedName()} Error: {e}");
						}
					}
				}
				obj = new DataNodeGroup();
				foreach (var field in fields) {
					if (typeof(ISyncObject).IsAssignableFrom(field.FieldType) && ((field.GetCustomAttribute<NoSaveAttribute>() is null) || (NetSync && (field.GetCustomAttribute<NoSyncAttribute>() is null)))) {
						try {
							if (!@object.IsRemoved) {
								_serializeFunctions.Add(new SerializeFunction {
									_parrentData = obj,
									_syncObject = (ISyncObject)field.GetValue(@object),
									_name = field.Name
								});
							}
						}
						catch (Exception e) {
							throw new Exception($"Failed to serialize {@object.GetType()}, field {field.Name}, field type {field.FieldType.GetFormattedName()}. Error: {e}");
						}
					}
				}
				var refID = new DataNode<NetPointer>(@object.Pointer);
				obj.SetValue("Pointer", refID);
			}
			if (localFirstCall) {
				for (var i = 0; i < _serializeFunctions.Count; i++) {
					_serializeFunctions[i]._parrentData.SetValue(_serializeFunctions[i]._name, _serializeFunctions[i]._syncObject.Serialize(this));
				}
			}
			return obj;
		}
	}
}
