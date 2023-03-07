using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using RhuEngine.Datatypes;

using RNumerics;

using SharedModels.GameSpecific;

using static RhuEngine.WorldObjects.World;

namespace RhuEngine.DataStructure
{

	public interface IDataNode : ISerlize<IDataNode>
	{
		public abstract void InitData();
		public abstract void ReadChildEnd(IDataNode child);
		public abstract void SaveAction(DataSaver DataSaver);
	}

	public static class DataNode
	{
		public static IDataNode DeSerlize(BinaryReader binaryReader) {
			var targetByte = binaryReader.ReadByte();
			IDataNode dataNode = targetByte switch {
				0 => new DataNodeList(),
				1 => new DataNodeGroup(),
				2 => new DataNode<int>(),
				3 => new DataNode<uint>(),
				4 => new DataNode<bool>(),
				5 => new DataNode<char>(),
				6 => new DataNode<string>(),
				7 => new DataNode<float>(),
				8 => new DataNode<double>(),
				9 => new DataNode<string[]>(),
				10 => new DataNode<long>(),
				11 => new DataNode<ulong>(),
				12 => new DataNode<byte>(),
				13 => new DataNode<sbyte>(),
				14 => new DataNode<short>(),
				15 => new DataNode<decimal>(),
				16 => new DataNode<byte[]>(),
				17 => new DataNode<NetPointer>(),
				18 => new DataNode<DateTime>(),
				19 => new DataNode<Playback>(),
				20 => new DataNode<float[]>(),
				21 => new DataNode<int[]>(),
				22 => new DataNode<Colorb>(),
				23 => new DataNode<Colorf>(),
				24 => new DataNode<ColorHSV>(),
				25 => new DataNode<AxisAlignedBox2d>(),
				26 => new DataNode<AxisAlignedBox2f>(),
				27 => new DataNode<AxisAlignedBox2i>(),
				28 => new DataNode<AxisAlignedBox3d>(),
				29 => new DataNode<AxisAlignedBox3f>(),
				30 => new DataNode<AxisAlignedBox3i>(),
				31 => new DataNode<Box2d>(),
				32 => new DataNode<Box2f>(),
				33 => new DataNode<Box3d>(),
				34 => new DataNode<Box3f>(),
				35 => new DataNode<Frame3f>(),
				36 => new DataNode<Index3i>(),
				37 => new DataNode<Index2i>(),
				38 => new DataNode<Index4i>(),
				39 => new DataNode<Interval1d>(),
				40 => new DataNode<Interval1i>(),
				41 => new DataNode<Line2d>(),
				42 => new DataNode<Line2f>(),
				43 => new DataNode<Line3d>(),
				44 => new DataNode<Line3f>(),
				45 => new DataNode<Matrix2d>(),
				46 => new DataNode<Matrix2f>(),
				47 => new DataNode<Matrix3d>(),
				48 => new DataNode<Matrix3f>(),
				49 => new DataNode<Plane3d>(),
				50 => new DataNode<Plane3f>(),
				51 => new DataNode<Quaterniond>(),
				52 => new DataNode<Quaternionf>(),
				53 => new DataNode<Ray3d>(),
				54 => new DataNode<Ray3f>(),
				55 => new DataNode<Segment3d>(),
				56 => new DataNode<Segment3f>(),
				57 => new DataNode<Triangle2d>(),
				58 => new DataNode<Triangle2f>(),
				59 => new DataNode<Triangle3d>(),
				60 => new DataNode<Triangle3f>(),
				61 => new DataNode<Vector2b>(),
				62 => new DataNode<Vector2d>(),
				63 => new DataNode<Vector2f>(),
				64 => new DataNode<Vector2i>(),
				65 => new DataNode<Vector2l>(),
				66 => new DataNode<Vector2u>(),
				67 => new DataNode<Vector3b>(),
				68 => new DataNode<Vector3d>(),
				69 => new DataNode<Vector3f>(),
				70 => new DataNode<Vector3i>(),
				71 => new DataNode<Vector3u>(),
				72 => new DataNode<Vector4b>(),
				73 => new DataNode<Vector4d>(),
				74 => new DataNode<Vector4f>(),
				75 => new DataNode<Vector4i>(),
				76 => new DataNode<Vector4u>(),
				77 => new DataNode<Vector3dTuple2>(),
				78 => new DataNode<Vector3dTuple3>(),
				79 => new DataNode<Vector3fTuple3>(),
				80 => new DataNode<Vector2dTuple2>(),
				81 => new DataNode<Vector2dTuple3>(),
				82 => new DataNode<Vector2dTuple4>(),
				83 => new DataNode<Circle3d>(),
				84 => new DataNode<Cylinder3d>(),
				85 => new DataNode<ushort>(),
				86 => new DataNode<Matrix>(),
				_ => null,
			};
			dataNode.DeSerlize(binaryReader);
			return dataNode;
		}

		private static readonly Type[] _typeArray = new Type[87] {
			typeof(DataNodeList),
			typeof(DataNodeGroup),
			typeof(DataNode<int>),
			typeof(DataNode<uint>),
			typeof(DataNode<bool>),
			typeof(DataNode<char>),
			typeof(DataNode<string>),
			typeof(DataNode<float>),
			typeof(DataNode<double>),
			typeof(DataNode<string[]>),
			typeof(DataNode<long>),
			typeof(DataNode<ulong>),
			typeof(DataNode<byte>),
			typeof(DataNode<sbyte>),
			typeof(DataNode<short>),
			typeof(DataNode<decimal>),
			typeof(DataNode<byte[]>),
			typeof(DataNode<NetPointer>),
			typeof(DataNode<DateTime>),
			typeof(DataNode<Playback>),
			typeof(DataNode<float[]>),
			typeof(DataNode<int[]>),
			typeof(DataNode<Colorb>),
			typeof(DataNode<Colorf>),
			typeof(DataNode<ColorHSV>),
			typeof(DataNode<AxisAlignedBox2d>),
			typeof(DataNode<AxisAlignedBox2f>),
			typeof(DataNode<AxisAlignedBox2i>),
			typeof(DataNode<AxisAlignedBox3d>),
			typeof(DataNode<AxisAlignedBox3f>),
			typeof(DataNode<AxisAlignedBox3i>),
			typeof(DataNode<Box2d>),
			typeof(DataNode<Box2f>),
			typeof(DataNode<Box3d>),
			typeof(DataNode<Box3f>),
			typeof(DataNode<Frame3f>),
			typeof(DataNode<Index3i>),
			typeof(DataNode<Index2i>),
			typeof(DataNode<Index4i>),
			typeof(DataNode<Interval1d>),
			typeof(DataNode<Interval1i>),
			typeof(DataNode<Line2d>),
			typeof(DataNode<Line2f>),
			typeof(DataNode<Line3d>),
			typeof(DataNode<Line3f>),
			typeof(DataNode<Matrix2d>),
			typeof(DataNode<Matrix2f>),
			typeof(DataNode<Matrix3d>),
			typeof(DataNode<Matrix3f>),
			typeof(DataNode<Plane3d>),
			typeof(DataNode<Plane3f>),
			typeof(DataNode<Quaterniond>),
			typeof(DataNode<Quaternionf>),
			typeof(DataNode<Ray3d>),
			typeof(DataNode<Ray3f>),
			typeof(DataNode<Segment3d>),
			typeof(DataNode<Segment3f>),
			typeof(DataNode<Triangle2d>),
			typeof(DataNode<Triangle2f>),
			typeof(DataNode<Triangle3d>),
			typeof(DataNode<Triangle3f>),
			typeof(DataNode<Vector2b>),
			typeof(DataNode<Vector2d>),
			typeof(DataNode<Vector2f>),
			typeof(DataNode<Vector2i>),
			typeof(DataNode<Vector2l>),
			typeof(DataNode<Vector2u>),
			typeof(DataNode<Vector3b>),
			typeof(DataNode<Vector3d>),
			typeof(DataNode<Vector3f>),
			typeof(DataNode<Vector3i>),
			typeof(DataNode<Vector3u>),
			typeof(DataNode<Vector4b>),
			typeof(DataNode<Vector4d>),
			typeof(DataNode<Vector4f>),
			typeof(DataNode<Vector4i>),
			typeof(DataNode<Vector4u>),
			typeof(DataNode<Vector3dTuple2>),
			typeof(DataNode<Vector3dTuple3>),
			typeof(DataNode<Vector3fTuple3>),
			typeof(DataNode<Vector2dTuple2>),
			typeof(DataNode<Vector2dTuple3>),
			typeof(DataNode<Vector2dTuple4>),
			typeof(DataNode<Circle3d>),
			typeof(DataNode<Cylinder3d>),
			typeof(DataNode<ushort>),
			typeof(DataNode<Matrix>),
		};

		public static void Serlize(BinaryWriter binaryWriter, IDataNode dataNode) {
			var checkType = dataNode.GetType();
			var targetType = (byte)Array.IndexOf(_typeArray, checkType);
			binaryWriter.Write(targetType);
			dataNode.Serlize(binaryWriter);
		}
	}
	public sealed class DataNodeHolder : ISerlize<DataNodeHolder>
	{
		public int index;
		public IDataNode dataNode;
		public void DeSerlize(BinaryReader binaryReader) {
			index = binaryReader.ReadInt32();
			dataNode = DataNode.DeSerlize(binaryReader);
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(index);
			DataNode.Serlize(binaryWriter, dataNode);
		}
	}

	public sealed class BlockStore : INetPacked, ISerlize<BlockStore>
	{
		public List<DataNodeHolder> dataNodes = new();

		public void DeSerlize(BinaryReader binaryReader) {
			var size = binaryReader.ReadInt32();
			for (var i = 0; i < size; i++) {
				var dataNode = new DataNodeHolder();
				dataNode.DeSerlize(binaryReader);
				dataNodes.Add(dataNode);
			}
		}

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(dataNodes.Count);
			for (var i = 0; i < dataNodes.Count; i++) {
				dataNodes[i].Serlize(binaryWriter);
			}
		}
	}


	public sealed class DataReader
	{
		public BlockStore Store = new();
		public DataReader(byte[] data) {
			Store = new();
			var reader = new BinaryReader(new MemoryStream(data));
			Store.DeSerlize(reader);
			ReadData();
		}
		public DataReader(BlockStore storeData) {
			Store = storeData;
			ReadData();
		}

		public IDataNode Data { get; private set; }

		private void ReadData() {
			for (var i = Store.dataNodes.Count - 1; i >= 0; i--) {
				if (i == 0) {
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
			using var stream = new MemoryStream();
			var dataNodes = new BinaryWriter(stream);
			Store.Serlize(dataNodes);
			return stream.ToArray();
		}
	}
}
