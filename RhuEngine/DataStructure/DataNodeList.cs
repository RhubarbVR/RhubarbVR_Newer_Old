using System.Collections.Generic;

using MessagePack;

using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure
{
	[MessagePackObject]
	public class DataNodeList : IDataNode
	{
		[Key(0)]
		public List<IDataNode> _nodeGroup = new();
		public byte[] GetByteArray() {
			return Serializer.Save(_nodeGroup);
		}

		public IEnumerator<IDataNode> GetEnumerator() {
			for (var i = 0; i < _nodeGroup.Count; i++) {
				yield return this[i];
			}
		}
		[IgnoreMember]
		IDataNode this[int i]
		{
			get => _nodeGroup[i];
			set => _nodeGroup[i] = value;
		}

		public void Add(IDataNode val) {
			_nodeGroup.Add(val);
		}
		public void SetByteArray(byte[] arrBytes) {
			_nodeGroup = Serializer.Read<List<IDataNode>>(arrBytes);
		}

		public DataNodeList() {
		}


		public DataNodeList(byte[] data) {
			SetByteArray(data);
		}
	}
}
