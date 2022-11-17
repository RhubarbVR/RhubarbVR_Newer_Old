using Microsoft.VisualStudio.TestTools.UnitTesting;

using RNumerics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RNumerics.Tests
{
	[TestClass()]
	public class FamcyTypeParserTests
	{
		[TestMethod()]
		public void SingleTypeParseTest() {
			var type = FamcyTypeParser.PraseType("RhuEngine.Components.Spinner");
			Assert.IsNotNull(type);
		}

		[TestMethod()]
		public void SmallHandParseTest() {
			var type = FamcyTypeParser.PraseType("int");
			Assert.IsNotNull(type);
		}

		[TestMethod()]
		public void NameSpaceSmallHandParseTestSpinner() {
			var type = FamcyTypeParser.PraseType("Spinner");
			Assert.IsNotNull(type);
		}
		[TestMethod()]
		public void NameSpaceSmallHandParseTestWorld() {
			var type = FamcyTypeParser.PraseType("World");
			Assert.IsNotNull(type);
		}
		[TestMethod()]
		public void NameSpaceSmallHandParseTestEnitity() {
			var type = FamcyTypeParser.PraseType("Entity");
			Assert.IsNotNull(type);
		}
		[TestMethod()]
		public void NameSpaceSmallHandParseTestVector3f() {
			var type = FamcyTypeParser.PraseType("Vector3f");
			Assert.IsNotNull(type);
		}
		[TestMethod()]
		public void GenaricTypeTestOne() {
			var type = FamcyTypeParser.PraseType("ValueField<int>");
			Assert.IsNotNull(type);
		}
		[TestMethod()]
		public void GenaricTypeTestTwo() {
			var type = FamcyTypeParser.PraseType("RefField<ValueField<int>>");
			Assert.IsNotNull(type);
		}
		[TestMethod()]
		public void GenaricTypeTestThree() {
			var type = FamcyTypeParser.PraseType("RefField<Entity>");
			Assert.IsNotNull(type);
		}
		[TestMethod()]
		public void GenaricTypeTestFour() {
			var type = FamcyTypeParser.PraseType("ValueField<Vector3f>");
			Assert.IsNotNull(type);
		}
		[TestMethod()]
		public void GenaricTypeTestFive() {
			var type = FamcyTypeParser.PraseType("SingleOperators<Vector3f,Vector3f>");
			Assert.IsNotNull(type);
		}
	}
}