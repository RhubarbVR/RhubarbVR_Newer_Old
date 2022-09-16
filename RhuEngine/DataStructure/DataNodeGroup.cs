using System;
using System.Collections.Generic;
using System.Linq;

using MessagePack;

using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure
{

	[MessagePackObject]
	public sealed class DataNodeGroup : IDataNode,IMessagePackSerializationCallbackReceiver
	{
		public void SaveAction(DataSaver DataSaver) {
			foreach (var item in _nodeGroup) {
				DataSaver.RunChildAction(item.Value);
			}
		}
		[IgnoreMember]
		public List<IDataNode> _TempGroup = new();

		public void InitData() {
			_nodeGroup.Clear();
			for (var i = 0; i < Keys.Length; i++) {
				_nodeGroup.Add(Keys[i], _TempGroup[i]);
			}
			_TempGroup.Clear();
		}

		[Key(0)]
		public string[] Keys;

		[IgnoreMember]
		public Dictionary<string, IDataNode> _nodeGroup = new();

		public void OnBeforeSerialize() {
			Keys = _nodeGroup.Keys.ToArray();
		}

		public void OnAfterDeserialize() {
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
