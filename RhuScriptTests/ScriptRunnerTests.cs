using Microsoft.VisualStudio.TestTools.UnitTesting;

using RhuScript;
using RhuScript.ScriptParts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuScript.Tests
{
	[TestClass()]
	public class ScriptRunnerTests
	{
		[TestMethod()]
		public void TestParseVariable_0() {
			var root = new ScriptRoot(@"
bool clickCount = false;
");
			Assert.IsTrue(root.Parts[0].GetType().Name.Contains("Variable"));
		}

		[TestMethod()]
		public void TestParseVariable_1() {
			var root = new ScriptRoot(@"
static bool clickCount = false;
");
			Assert.IsTrue(root.Parts[0].GetType().Name.Contains("Variable"));
		}

		[TestMethod()]
		public void TestParseVariable_2() {
			var root = new ScriptRoot(@"
public static bool clickCount = false;
");
			Assert.IsTrue(root.Parts[0].GetType().Name.Contains("Variable"));
		}

		[TestMethod()]
		public void TestParseVariable_3() {
			var root = new ScriptRoot(@"
static readonly bool clickCount = false;
");
			Assert.IsTrue(root.Parts[0].GetType().Name.Contains("Variable"));
		}

		[TestMethod()]
		public void TestParseVariable_4() {
			var root = new ScriptRoot(@"
public static readonly bool clickCount = false;
");
			Assert.IsTrue(root.Parts[0].GetType().Name.Contains("Variable"));
		}
	}
}