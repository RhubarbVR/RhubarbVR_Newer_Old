using System.Runtime.CompilerServices;

using MessagePack;

using SharedModels;
using SharedModels.GameSpecific;

namespace RhuEngine.DataStructure
{
	[MessagePackObject]
	public sealed class DataNode<T> : IDataNode
	{
		public DataNode(T def = default) {
			Value = def;
		}

		public DataNode() {
			Value = default;
		}
		[Key(0)]
		public T Value { get; set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(DataNode<T> data) => data.Value;

		public void SaveAction(DataSaver DataSaver) {
			//Not needed no children
		}

		public void InitData() {
			//Not needed no children
		}

		public void ReadChildEnd(IDataNode child) {
			//Not needed no children
		}

		public void ReadAction(DataReader dataReader) {
			//Not needed no children
		}
	}
}
