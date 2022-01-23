using System;

using MessagePack;

using RhuEngine.Datatypes;

using StereoKit;

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
	[Union(19, typeof(DataNode<Vec2>))]
	[Union(20, typeof(DataNode<Vec3>))]
	[Union(21, typeof(DataNode<Vec4>))]
	[Union(22, typeof(DataNode<Bounds>))]
	[Union(23, typeof(DataNode<Color>))]
	[Union(24, typeof(DataNode<Color32>))]
	[Union(25, typeof(DataNode<GradientKey>))]
	[Union(26, typeof(DataNode<HandJoint>))]
	[Union(27, typeof(DataNode<LinePoint>))]
	[Union(28, typeof(DataNode<Matrix>))]
	[Union(29, typeof(DataNode<Mouse>))]
	[Union(30, typeof(DataNode<Plane>))]
	[Union(31, typeof(DataNode<Pointer>))]
	[Union(32, typeof(DataNode<Pose>))]
	[Union(33, typeof(DataNode<Quat>))]
	[Union(34, typeof(DataNode<Ray>))]
	[Union(35, typeof(DataNode<Rect>))]
	[Union(36, typeof(DataNode<SHLight>))]
	[Union(37, typeof(DataNode<Sphere>))]
	[Union(38, typeof(DataNode<SphericalHarmonics>))]
	[Union(39, typeof(DataNode<SystemInfo>))]
	[Union(40, typeof(DataNode<UISettings>))]
	[Union(41, typeof(DataNode<Vertex>))]
	[Union(42, typeof(DataNode<float[]>))]
	[Union(43, typeof(DataNode<int[]>))]

	public interface IDataNode
	{
		public abstract byte[] GetByteArray();
		public abstract void SetByteArray(byte[] array);

	}
}
