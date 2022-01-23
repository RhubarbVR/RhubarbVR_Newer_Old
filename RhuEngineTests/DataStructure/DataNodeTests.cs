using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhuEngine.DataStructure;
using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;
using MessagePack;
using MessagePack.Resolvers;

namespace RhuEngine.DataStructure.Tests
{
	[TestClass()]
	public class DataNodeTests
	{
		[TestMethod()]
		public void TestAllDataNodes() {
			var test1 = new DataNode<int>(1);
			var data1 = test1.GetByteArray();
			Console.WriteLine(MessagePackSerializer.ConvertToJson(data1));
		
			var dataNodeGroup = new DataNodeGroup();
			dataNodeGroup.SetValue("RenderLayer", new DataNode<int>(1));
			dataNodeGroup.SetValue("Name", new DataNode<string>("Trains"));
			dataNodeGroup.SetValue("Pos", new DataNode<Vec3>(Vec3.Forward));
			dataNodeGroup.SetValue("Rot", new DataNode<Quat>(Quat.Identity));
			dataNodeGroup.SetValue("scale", new DataNode<Vec3>(Vec3.Forward));
			dataNodeGroup.SetValue("pose", new DataNode<Pose>(new Pose(Vec3.Forward, Quat.Identity)));

			var dataNodeGroupw = new DataNodeGroup();
			dataNodeGroupw.SetValue("RenderLayer", new DataNode<int>(1));
			dataNodeGroupw.SetValue("Name", new DataNode<string>("Trains"));
			dataNodeGroupw.SetValue("Pos", new DataNode<Vec3>(Vec3.Forward));
			dataNodeGroupw.SetValue("Rot", new DataNode<Quat>(Quat.Identity));
			dataNodeGroupw.SetValue("scale", new DataNode<Vec3>(Vec3.Forward));
			dataNodeGroupw.SetValue("pose", new DataNode<Pose>(new Pose(Vec3.Forward, Quat.Identity)));
			dataNodeGroup.SetValue("ew", dataNodeGroupw);
			var list = new DataNodeList();
			list.Add(dataNodeGroupw);
			list.Add(dataNodeGroupw);
			dataNodeGroup.SetValue("testlist", list);

			var data = dataNodeGroup.GetByteArray();
			var jsonstring = MessagePackSerializer.ConvertToJson(data);
			Console.WriteLine(jsonstring);
			var datanode = new DataNodeGroup(MessagePackSerializer.ConvertFromJson(jsonstring));
			Console.WriteLine(((DataNode<string>)datanode.GetValue("Name")).Value);
			Console.WriteLine(((DataNode<string>)((DataNodeGroup)datanode.GetValue("ew")).GetValue("Name")).Value);
		}
	}
}