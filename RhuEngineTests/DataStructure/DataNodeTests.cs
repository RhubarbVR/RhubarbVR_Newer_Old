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
			var dataNodeGroup = new DataNodeGroup();
			dataNodeGroup.SetValue("RenderLayer", new DataNode<int>(1));
			dataNodeGroup.SetValue("Name", new DataNode<string>("Trains"));
			dataNodeGroup.SetValue("Pos", new DataNode<Vector3f>(Vector3f.Forward));
			dataNodeGroup.SetValue("Rot", new DataNode<Quaternionf>(Quaternionf.Identity));
			dataNodeGroup.SetValue("scale", new DataNode<Vector3f>(Vector3f.Forward));

			var dataNodeGroupw = new DataNodeGroup();
			dataNodeGroupw.SetValue("RenderLayer", new DataNode<int>(1));
			dataNodeGroupw.SetValue("Name", new DataNode<string>("Trains2"));
			dataNodeGroupw.SetValue("Pos", new DataNode<Vector3f>(Vector3f.Forward));
			dataNodeGroupw.SetValue("Rot", new DataNode<Quaternionf>(Quaternionf.Identity));
			dataNodeGroupw.SetValue("scale", new DataNode<Vector3f>(Vector3f.Forward));
			var list = new DataNodeList();
			list.Add(new DataNode<int>(1));
			list.Add(new DataNode<string>("Wdadwa"));
			dataNodeGroupw.SetValue("testlist", list);
			dataNodeGroup.SetValue("ew", dataNodeGroupw);

			var saver = new DataSaver(dataNodeGroup);
			var data = saver.SaveStore();
			var loaded = new DataReader(data);
			if (loaded.Data is null) {
				throw new Exception("Failed to load data at all");
			}
			Assert.AreEqual("Trains", ((DataNode<string>)((DataNodeGroup)loaded.Data).GetValue("Name")).Value);
			Assert.AreEqual("Trains2", ((DataNode<string>)((DataNodeGroup)((DataNodeGroup)loaded.Data).GetValue("ew")).GetValue("Name")).Value);
			Assert.AreEqual(1, ((DataNode<int>)((DataNodeList)((DataNodeGroup)((DataNodeGroup)loaded.Data).GetValue("ew")).GetValue("testlist"))[0]).Value);
		}
	}
}