using System.Collections.Generic;

using MessagePack;

using SharedModels;

namespace RhuEngine.DataStructure
{
	[MessagePackObject]
	public class DataNodeGroup : IDataNode
	{
		[Key(0)]
		public Dictionary<string, IDataNode> _nodeGroup = new();
		
		public IDataNode this[string key]
		{
			get => GetValue(key);
			set => SetValue(key, value);
		}

		public byte[] GetByteArray() {
			return Serializer.Save(_nodeGroup);
		}

		public IDataNode GetValue(string key) {
			return _nodeGroup.TryGetValue(key, out var value) ? value : default;
		}

		public T GetValue<T>(string key) {
			return ((DataNode<T>)(_nodeGroup.TryGetValue(key, out var value) ? value : default)).Value;
		}

		public void SetValue(string key, IDataNode obj) {
			_nodeGroup.Add(key, obj);
		}
		public void SetByteArray(byte[] arrBytes) {
			_nodeGroup = Serializer.Read<Dictionary<string, IDataNode>>(arrBytes);
		}


		public DataNodeGroup() {
		}
		public DataNodeGroup(string data) {
			SetByteArray(MessagePackSerializer.ConvertFromJson(data, Serializer.Options));
		}

		public DataNodeGroup(byte[] data) {
			if (data == null) {
				throw new System.Exception("Data is null");
			}
			SetByteArray(data);
		}

		public DataNodeGroup(Dictionary<string, IDataNode> data) {
			_nodeGroup = data;
		}
	}
}
