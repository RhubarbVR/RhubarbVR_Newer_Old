using Microsoft.VisualStudio.TestTools.UnitTesting;

using RNumerics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNumerics.Tests
{
	public struct TestStruct
	{
		public int x;
		public float y;
	}
	public struct TestArray
	{
		public int[] Value;
	}
	public struct TestNested
	{
		public TestArray testArray;
		public TestStruct testStruct;
	}
	public struct TestNestedNested
	{
		public TestNested testStruct;
		public int x;
		public float y;
	}
	public struct TestOfTests
	{
		public TestStruct[] Value;
	}
	[TestClass()]
	public class StructEditorTests
	{
		[TestMethod()]
		public void TestNormalArrayStructs() {
			var normal = new int[1] { 0 };
			Assert.AreEqual(typeof(int), StructEditor.GetFielType(typeof(int[]), "[0]"));
			StructEditor.SetFieldValue(ref normal, "[0]", 10);
			Assert.AreEqual(10, normal[0]);
			StructEditor.SetFieldValue(ref normal, "[0]", 130);
			Assert.AreEqual(130, StructEditor.GetFieldValue(normal, "[0]"));
		}

		[TestMethod()]
		public void TestOfTests() {
			var normal = new TestOfTests { Value = new TestStruct[1] { new TestStruct { } } };
			Assert.AreEqual(typeof(TestStruct), StructEditor.GetFielType(typeof(TestOfTests), "Value[0]"));
			Assert.AreEqual(typeof(int), StructEditor.GetFielType(typeof(TestOfTests), "Value[0].x"));
			StructEditor.SetFieldValue(ref normal, "Value[0].x", 10);
			Assert.AreEqual(10, normal.Value[0].x);
			StructEditor.SetFieldValue(ref normal, "Value[0].x", 130);
			Assert.AreEqual(130, StructEditor.GetFieldValue(normal, "Value[0].x"));
		}


		[TestMethod()]
		public void TestNestedArrayStructs() {
			var normal = new TestArray { Value = new int[1] { 0 } };
			Assert.AreEqual(typeof(int), StructEditor.GetFielType(typeof(TestArray), "Value[0]"));
			StructEditor.SetFieldValue(ref normal, "Value[0]", 10);
			Assert.AreEqual(10, normal.Value[0]);
			StructEditor.SetFieldValue(ref normal, "Value[0]", 130);
			Assert.AreEqual(130, StructEditor.GetFieldValue(normal, "Value[0]"));
		}

		[TestMethod()]
		public void TestNormalStructs() {
			var normal = new TestStruct { };
			Assert.AreEqual(typeof(int), StructEditor.GetFielType(typeof(TestStruct), "x"));
			Assert.AreEqual(typeof(float), StructEditor.GetFielType(typeof(TestStruct), "y"));
			StructEditor.SetFieldValue(ref normal, "x", 10);
			StructEditor.SetFieldValue(ref normal, "y", 10.1f);
			Assert.AreEqual(10, normal.x);
			Assert.AreEqual(10.1f, normal.y);
			StructEditor.SetFieldValue(ref normal, "x", 130);
			StructEditor.SetFieldValue(ref normal, "y", 130.1f);
			Assert.AreEqual(130, StructEditor.GetFieldValue(normal, "x"));
			Assert.AreEqual(130.1f, StructEditor.GetFieldValue(normal, "y"));
		}

		[TestMethod()]
		public void TestNestedStructs() {
			var normal = new TestNested { };
			Assert.AreEqual(typeof(int), StructEditor.GetFielType(typeof(TestNested), "testStruct.x"));
			Assert.AreEqual(typeof(float), StructEditor.GetFielType(typeof(TestNested), "testStruct.y"));
			StructEditor.SetFieldValue(ref normal, "testStruct.x", 10);
			StructEditor.SetFieldValue(ref normal, "testStruct.y", 10.1f);
			Assert.AreEqual(10, normal.testStruct.x);
			Assert.AreEqual(10.1f, normal.testStruct.y);
			StructEditor.SetFieldValue(ref normal, "testStruct.x", 130);
			StructEditor.SetFieldValue(ref normal, "testStruct.y", 130.1f);
			Assert.AreEqual(130, StructEditor.GetFieldValue(normal, "testStruct.x"));
			Assert.AreEqual(130.1f, StructEditor.GetFieldValue(normal, "testStruct.y"));

		}
		[TestMethod()]
		public void TestNestedNestedStructs() {


			var normal = new TestNestedNested { };
			Assert.AreEqual(typeof(int), StructEditor.GetFielType(typeof(TestNestedNested), "testStruct.testStruct.x"));
			Assert.AreEqual(typeof(float), StructEditor.GetFielType(typeof(TestNestedNested), "testStruct.testStruct.y"));
			StructEditor.SetFieldValue(ref normal, "testStruct.testStruct.x", 10);
			StructEditor.SetFieldValue(ref normal, "testStruct.testStruct.y", 10.1f);
			Assert.AreEqual(10, normal.testStruct.testStruct.x);
			Assert.AreEqual(10.1f, normal.testStruct.testStruct.y);
			StructEditor.SetFieldValue(ref normal, "testStruct.testStruct.x", 130);
			StructEditor.SetFieldValue(ref normal, "testStruct.testStruct.y", 130.1f);
			Assert.AreEqual(130, StructEditor.GetFieldValue(normal, "testStruct.testStruct.x"));
			Assert.AreEqual(130.1f, StructEditor.GetFieldValue(normal, "testStruct.testStruct.y"));

		}
	}
}