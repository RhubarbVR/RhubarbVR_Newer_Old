using System;
using System.Collections.Generic;

using MessagePack;

using RhuEngine.Datatypes;

using RNumerics;

using SharedModels.GameSpecific;

using static RhuEngine.WorldObjects.World;

namespace RhuEngine.DataStructure
{
	[Union(0, typeof(DataNodeList))]
	[Union(1, typeof(DataNodeGroup))]
	[Union(2, typeof(DataNode<int>))]
	[Union(3, typeof(DataNode<uint>))]
	[Union(4, typeof(DataNode<bool>))]
	[Union(5, typeof(DataNode<char>))]
	[Union(6, typeof(DataNode<string>))]
	[Union(7, typeof(DataNode<float>))]
	[Union(8, typeof(DataNode<double>))]
	[Union(9, typeof(DataNode<string[]>))]
	[Union(10, typeof(DataNode<long>))]
	[Union(11, typeof(DataNode<ulong>))]
	[Union(12, typeof(DataNode<byte>))]
	[Union(13, typeof(DataNode<sbyte>))]
	[Union(14, typeof(DataNode<short>))]
	[Union(15, typeof(DataNode<decimal>))]
	[Union(16, typeof(DataNode<byte[]>))]
	[Union(17, typeof(DataNode<NetPointer>))]
	[Union(18, typeof(DataNode<DateTime>))]
	[Union(19, typeof(DataNode<Playback>))]
	[Union(20, typeof(DataNode<float[]>))]
	[Union(21, typeof(DataNode<int[]>))]
	[Union(30, typeof(DataNode<Colorb>))]
	[Union(31, typeof(DataNode<Colorf>))]
	[Union(32, typeof(DataNode<ColorHSV>))]
	[Union(33, typeof(DataNode<ColorMap>))]
	[Union(34, typeof(DataNode<ColorMap.ColorPoint>))]
	[Union(35, typeof(DataNode<AxisAlignedBox2d>))]
	[Union(36, typeof(DataNode<AxisAlignedBox2f>))]
	[Union(37, typeof(DataNode<AxisAlignedBox2i>))]
	[Union(38, typeof(DataNode<AxisAlignedBox3d>))]
	[Union(39, typeof(DataNode<AxisAlignedBox3f>))]
	[Union(40, typeof(DataNode<AxisAlignedBox3i>))]
	[Union(41, typeof(DataNode<Box2d>))]
	[Union(42, typeof(DataNode<Box2f>))]
	[Union(43, typeof(DataNode<Box3d>))]
	[Union(45, typeof(DataNode<Box3f>))]
	[Union(46, typeof(DataNode<Frame3f>))]
	[Union(47, typeof(DataNode<Index3i>))]
	[Union(48, typeof(DataNode<Index2i>))]
	[Union(49, typeof(DataNode<Index4i>))]
	[Union(50, typeof(DataNode<Interval1d>))]
	[Union(51, typeof(DataNode<Interval1i>))]
	[Union(52, typeof(DataNode<Line2d>))]
	[Union(53, typeof(DataNode<Line2f>))]
	[Union(54, typeof(DataNode<Line3d>))]
	[Union(55, typeof(DataNode<Line3f>))]
	[Union(56, typeof(DataNode<Matrix2d>))]
	[Union(57, typeof(DataNode<Matrix2f>))]
	[Union(58, typeof(DataNode<Matrix3d>))]
	[Union(59, typeof(DataNode<Matrix3f>))]
	[Union(60, typeof(DataNode<Plane3d>))]
	[Union(61, typeof(DataNode<Plane3f>))]
	[Union(62, typeof(DataNode<Quaterniond>))]
	[Union(63, typeof(DataNode<Quaternionf>))]
	[Union(64, typeof(DataNode<Ray3d>))]
	[Union(65, typeof(DataNode<Ray3f>))]
	[Union(66, typeof(DataNode<Segment3d>))]
	[Union(67, typeof(DataNode<Segment3f>))]
	[Union(68, typeof(DataNode<Triangle2d>))]
	[Union(69, typeof(DataNode<Triangle2f>))]
	[Union(70, typeof(DataNode<Triangle3d>))]
	[Union(71, typeof(DataNode<Triangle3f>))]
	[Union(72, typeof(DataNode<Vector2b>))]
	[Union(73, typeof(DataNode<Vector2d>))]
	[Union(74, typeof(DataNode<Vector2f>))]
	[Union(75, typeof(DataNode<Vector2i>))]
	[Union(76, typeof(DataNode<Vector2l>))]
	[Union(77, typeof(DataNode<Vector2u>))]
	[Union(78, typeof(DataNode<Vector3b>))]
	[Union(79, typeof(DataNode<Vector3d>))]
	[Union(80, typeof(DataNode<Vector3f>))]
	[Union(81, typeof(DataNode<Vector3i>))]
	[Union(82, typeof(DataNode<Vector3u>))]
	[Union(83, typeof(DataNode<Vector4b>))]
	[Union(84, typeof(DataNode<Vector4d>))]
	[Union(85, typeof(DataNode<Vector4f>))]
	[Union(86, typeof(DataNode<Vector4i>))]
	[Union(87, typeof(DataNode<Vector4u>))]
	[Union(88, typeof(DataNode<Vector3dTuple2>))]
	[Union(89, typeof(DataNode<Vector3dTuple3>))]
	[Union(90, typeof(DataNode<Vector3fTuple3>))]
	[Union(91, typeof(DataNode<Vector2dTuple2>))]
	[Union(92, typeof(DataNode<Vector2dTuple3>))]
	[Union(93, typeof(DataNode<Vector2dTuple4>))]
	[Union(94, typeof(DataNode<Circle3d>))]
	[Union(95, typeof(DataNode<Cylinder3d>))]
	[Union(96, typeof(DataNode<ushort>))]
	[Union(97, typeof(DataNode<Matrix>))]
	[Union(100, typeof(DataNode<object>))]

	public interface IDataNode
	{
		public abstract void InitData();
		public abstract void ReadChildEnd(IDataNode child);
		public abstract void SaveAction(DataSaver DataSaver);
	}

	[MessagePackObject]
	public sealed class DataNodeHolder
	{
		[Key(0)]
		public int index;
		[Key(1)]
		public IDataNode dataNode;
	}

	[MessagePackObject]
	public sealed class BlockStore : INetPacked
	{
		[Key(0)]
		public List<DataNodeHolder> dataNodes = new();
	}


	public sealed class DataReader
	{
		public BlockStore Store = new();
		public DataReader(byte[] data) {
			Store = Serializer.Read<BlockStore>(data);
			ReadData();
		}
		public DataReader(BlockStore storeData) {
			Store = storeData;
			ReadData();
		}

		public IDataNode Data { get; private set; }

		private void ReadData() {
			for (var i = Store.dataNodes.Count - 1; i >= 0; i--) {
				if(i == 0) {
					Data = Store.dataNodes[0].dataNode;
					Store.dataNodes[0].dataNode.InitData();
					Store = null;
					break;
				}
				Store.dataNodes[Store.dataNodes[i].index].dataNode.ReadChildEnd(Store.dataNodes[i].dataNode);
				Store.dataNodes[i].dataNode.InitData();
			}
		}
	}
	public class DataSaver
	{
		public DataSaver(IDataNode dataNode) {
			Store.dataNodes.Add(new DataNodeHolder { index = -1, dataNode = dataNode });
			LoadDataInToStore();
		}

		public void LoadDataInToStore() {
			for (var i = 0; i < Store.dataNodes.Count; i++) {
				CurrentIndex = i;
				var currentObject = Store.dataNodes[i];
				currentObject.dataNode.SaveAction(this);
			}
		}

		public BlockStore Store = new();
		public int CurrentIndex;

		public void RunChildAction(IDataNode item) {
			Store.dataNodes.Add(new DataNodeHolder { index = CurrentIndex, dataNode = item });
		}

		public byte[] SaveStore() {
			return Serializer.Save(Store);
		}
	}
}
