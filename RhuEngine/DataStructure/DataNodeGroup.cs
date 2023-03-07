using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RhuSettings;

using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure
{
	public sealed class DataNodeGroup : IDataNode
	{
		public void SaveAction(DataSaver DataSaver) {
			foreach (var item in _nodeGroup) {
				DataSaver.RunChildAction(item.Value);
			}
		}
		public List<IDataNode> _TempGroup = new();

		public void InitData() {
			_nodeGroup.Clear();
			for (var i = 0; i < Keys.Length; i++) {
				_nodeGroup.Add(Keys[i], _TempGroup[i]);
			}
			_TempGroup.Clear();
		}

		public string[] Keys;

		public Dictionary<string, IDataNode> _nodeGroup = new();


		public void Serlize(BinaryWriter binaryWriter) {
			Keys = _nodeGroup.Keys.ToArray();
			binaryWriter.Write(Keys.Length);
			binaryWriter.Write(_TempGroup.Count);
			foreach (var key in Keys) {
				binaryWriter.Write(key);
			}
			foreach (var dataNode in _TempGroup) {
				DataNode.Serlize(binaryWriter, dataNode);
			}
		}

		public void DeSerlize(BinaryReader binaryReader) {
			var lenth = binaryReader.ReadInt32();
			var count = binaryReader.ReadInt32();
			Keys = new string[lenth];
			for (var i = 0; i < lenth; i++) {
				Keys[i] = binaryReader.ReadString();
			}
			_TempGroup.Clear();
			for (var i = 0; i < count; i++) {
				_TempGroup.Add(DataNode.DeSerlize(binaryReader));
			}
		}

		public IDataNode this[string key]
		{
			get => GetValue(key);
			set => SetValue(key, value);
		}


		public IDataNode GetValue(string key) {
			return _nodeGroup.TryGetValue(key, out var value) ? value : null;
		}

		public T GetValue<T>(string key) {
			return ((DataNode<T>)(_nodeGroup.TryGetValue(key, out var value) ? value : default)).Value;
		}

		public void SetValue(string key, IDataNode obj) {
			_nodeGroup.Add(key, obj);
		}



		public void ReadChildEnd(IDataNode child) {
			_TempGroup.Insert(0, child);
		}


		public DataNodeGroup() {
		}
	}
}
