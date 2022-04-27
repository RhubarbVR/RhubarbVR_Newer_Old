using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhuEngine.DataStructure;
using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using MessagePack.Resolvers;
using RNumerics;
using SharedModels.GameSpecific;

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
			dataNodeGroup.SetValue("Pos", new DataNode<Vector3f>(Vector3f.Forward));
			dataNodeGroup.SetValue("Rot", new DataNode<Quaternionf>(Quaternionf.Identity));
			dataNodeGroup.SetValue("scale", new DataNode<Vector3f>(Vector3f.Forward));

			var dataNodeGroupw = new DataNodeGroup();
			dataNodeGroupw.SetValue("RenderLayer", new DataNode<int>(1));
			dataNodeGroupw.SetValue("Name", new DataNode<string>("Trains"));
			dataNodeGroupw.SetValue("Pos", new DataNode<Vector3f>(Vector3f.Forward));
			dataNodeGroupw.SetValue("Rot", new DataNode<Quaternionf>(Quaternionf.Identity));
			dataNodeGroupw.SetValue("scale", new DataNode<Vector3f>(Vector3f.Forward));
			dataNodeGroup.SetValue("ew", dataNodeGroupw);
			var list = new DataNodeList();
			list.Add(dataNodeGroupw);
			list.Add(dataNodeGroupw);
			dataNodeGroup.SetValue("testlist", list);

			var data = dataNodeGroup.GetByteArray();
			var jsonstring = MessagePackSerializer.ConvertToJson(data, Serializer.Options);
			Console.WriteLine(jsonstring);
			var datanode = new DataNodeGroup(MessagePackSerializer.ConvertFromJson(jsonstring, Serializer.Options));
			Console.WriteLine(((DataNode<string>)datanode.GetValue("Name")).Value);
			Console.WriteLine(((DataNode<string>)((DataNodeGroup)datanode.GetValue("ew")).GetValue("Name")).Value);
		}
	}
}