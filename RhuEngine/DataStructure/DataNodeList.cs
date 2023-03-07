using System.Collections.Generic;
using System.IO;

using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure
{
	public sealed class DataNodeList : IDataNode
	{
		public void SaveAction(DataSaver DataSaver) {
			foreach (var item in _nodeGroup) {
				DataSaver.RunChildAction(item);
			}
		}

		public List<IDataNode> _nodeGroup = new();

		public IEnumerator<IDataNode> GetEnumerator() {
			for (var i = 0; i < _nodeGroup.Count; i++) {
				yield return this[i];
			}
		}
		public IDataNode this[int i]
		{
			get => _nodeGroup[i];
			set => _nodeGroup[i] = value;
		}

		public void Add(IDataNode val) {
			_nodeGroup.Add(val);
		}

		public void InitData() {
		}

		public void ReadChildEnd(IDataNode child) {
			_nodeGroup.Insert(0, child);
		}

		public void Serlize(BinaryWriter binaryWriter) {
		}

		public void DeSerlize(BinaryReader binaryReader) {
		}

		public DataNodeList() {
		}
	}
}
