using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MessagePack;
using MessagePack.Resolvers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NullContext;

using RhuEngine.Components;
using RhuEngine.Datatypes;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using SharedModels.GameSpecific;

namespace RhuEngine.GameTests.Tests
{
	[TestClass()]
	public class EcmaTests
	{
		public GenericGameTester tester;

		private void SetUpForNormalTest() {
			tester = new GenericGameTester();
			tester.Start(Array.Empty<string>());
			tester.RunForSteps(5);
			tester.app.startingthread.Join();
		}
		public World StartNewTestWorld() {
			SetUpForNormalTest();
			var world = tester.app.worldManager.CreateNewWorld(World.FocusLevel.Focused, true);
			Assert.IsNotNull(world);
			return world;
		}

		public Entity AttachEntity() {
			var newworld = StartNewTestWorld();
			var newEntity = newworld.RootEntity.AddChild("TestScriptEntity");
			Assert.IsNotNull(newEntity);
			return newEntity;
		}

		public RawECMAScript AttachTestScript() {
			return AttachEntity().AttachComponent<RawECMAScript>();
		}

		[TestMethod()]
		public void TestHiding() {
			var script = AttachTestScript();
			var value = script.Entity.AttachComponent<ValueField<string>>();
			value.Value.Value = "FirstValue";
			script.Targets.Add().Target = value;
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).Value.Value = script.OnLoad;
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(null, value.Value.Value);
			((IDisposable)tester).Dispose();
		}

		public class TestComp : Component
		{
			public bool IShowFail = false;

			public void IfIrunICrash() {
				IShowFail = true;
				RLog.Err("This should NeverHappen");
				throw new Exception();
			}
			public World IfIrunIAlsoRunICrash() {
				IShowFail = true;
				RLog.Err("This should NeverHappen");
				throw new Exception();
			}

			public bool IGoTrue = false;
			[Exposed]
			public void IfIrunISetValueToTrue() {
				RLog.Err("I Ran Value Change");
				IGoTrue = true;
			}
		}
		[TestMethod()]
		public void TestNormalFunctionHiding() {
			var script = AttachTestScript();
			var value = script.Entity.AttachComponent<TestComp>();
			script.Targets.Add().Target = value;
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).IfIrunICrash();
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(false, value.IShowFail);
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestSyncHideFunctionHiding() {
			var script = AttachTestScript();
			var value = script.Entity.AttachComponent<TestComp>();
			script.Targets.Add().Target = value;
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).IfIrunIAlsoRunICrash();
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(false, value.IShowFail);
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestFunctionShowing() {
			var script = AttachTestScript();
			var value = script.Entity.AttachComponent<TestComp>();
			script.Targets.Add().Target = value;
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).IfIrunISetValueToTrue();
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(true, value.IGoTrue);
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestRemoveOfValue() {
			var script = AttachTestScript();
			var value = script.Entity.AttachComponent<ValueField<string>>();
			value.Value.Value = "FirstValue";
			script.Targets.Add().Target = value;
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).Value = null;
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreNotEqual(null, value.Value);
			((IDisposable)tester).Dispose();
		}


		[TestMethod()]
		public void TestNormalTypeValue() {
			var script = AttachTestScript();
			var value = script.Entity.AttachComponent<ValueField<Type>>();
			script.Targets.Add().Target = value;
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).Value.Value = getType(""RhuEngine.Components.CapsuleMesh"")
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(typeof(CapsuleMesh), value.Value.Value);
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestFloatAdd() {
			var script = AttachTestScript();
			var random = new Random();
			var test1 = random.NextSingle();
			var test2 = random.NextSingle();
			var value = script.Entity.AttachComponent<ValueField<float>>();
			value.Value.Value = test1;
			var value2 = script.Entity.AttachComponent<ValueField<float>>();
			value2.Value.Value = test2;
			script.Targets.Add().Target = value;
			script.Targets.Add().Target = value2;
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).Value.Value = script.GetTarget(0).Value.Value + script.GetTarget(1).Value.Value;
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(test1 + test2, value.Value.Value);
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestVectorsReadX() {
			var script = AttachTestScript();
			var random = new Random();
			var test2 = new Vector2f(random.NextSingle(), random.NextSingle());
			var value = script.Entity.AttachComponent<ValueField<float>>();
			var value2 = script.Entity.AttachComponent<ValueField<Vector2f>>();
			value2.Value.Value = test2;
			script.Targets.Add().Target = value;
			script.Targets.Add().Target = value2;
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).Value.Value = script.GetTarget(1).Value.Value.X;
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(value.Value.Value, value2.Value.Value.x);
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestVectorsSetX() {
			var script = AttachTestScript();
			var random = new Random();
			var test2 = new Vector2f(random.NextSingle(), random.NextSingle());
			var value = script.Entity.AttachComponent<ValueField<float>>();
			var value2 = script.Entity.AttachComponent<ValueField<Vector2f>>();
			value2.Value.Value = test2;
			script.Targets.Add().Target = value;
			script.Targets.Add().Target = value2;
			script.ScriptCode.Value = @"
				function RunCode()	{
					var value = script.GetTarget(1).Value.Value;
					value.X = script.GetTarget(0).Value.Value;
					script.GetTarget(1).Value.Value = value;
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(value.Value.Value, value2.Value.Value.x);
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestRefIDWriteTwo() {
			unchecked {
				var script = AttachTestScript();
				var random = new Random();
				var test2 = (ulong)random.NextInt64();

				var value = script.Entity.AttachComponent<ValueField<NetPointer>>();
				var value2 = script.Entity.AttachComponent<ValueField<ulong>>();
				value2.Value.Value = test2;
				script.Targets.Add().Target = value;
				script.Targets.Add().Target = value2;
				script.ScriptCode.Value = @"
				function RunCode()	{
					var value = script.GetTarget(0).Value.Value;
					value.id = script.GetTarget(1).Value.Value;
					script.GetTarget(0).Value.Value = value;
				}
			";
				if (!script.ScriptLoaded) {
					throw new Exception("Script not loaded");
				}
				script.Invoke("RunCode");
				Assert.AreNotEqual(value.Value.Value.id, value2.Value.Value);
				((IDisposable)tester).Dispose();
			}
		}

		[TestMethod()]
		public void TestRefIDWriteOne() {
			unchecked {
				var script = AttachTestScript();
				var random = new Random();
				var test2 = (ulong)random.NextInt64();

				var value = script.Entity.AttachComponent<ValueField<NetPointer>>();
				var value2 = script.Entity.AttachComponent<ValueField<ulong>>();
				value2.Value.Value = test2;
				script.Targets.Add().Target = value;
				script.Targets.Add().Target = value2;
				script.ScriptCode.Value = @"
				function RunCode()	{
					script.GetTarget(0).Value.Value.id = script.GetTarget(1).Value.Value;
				}
			";
				if (!script.ScriptLoaded) {
					throw new Exception("Script not loaded");
				}
				script.Invoke("RunCode");
				Assert.AreNotEqual(value.Value.Value.id, value2.Value.Value);
				((IDisposable)tester).Dispose();
			}
		}

		[TestMethod()]
		public void TryToMakeSyncWorldTest() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.Entity.AttachComponent(getType(""ValueField<World>""));
					var e = new Vector3f(1,1,1);
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.IsNull(script.Entity.GetFirstComponent<ValueField<World>>());
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void AttachComponentTest() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.Entity.AttachComponent(getType(""RhuEngine.Components.Spinner""));
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.IsNotNull(script.Entity.GetFirstComponent<Spinner>());
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void AttachEntityTest() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.Entity.AddChild();
					script.Entity.AddChild();
					script.Entity.AddChild();
					script.Entity.AddChild();
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(4, script.Entity.children.Count);
			((IDisposable)tester).Dispose();
		}
		[TestMethod()]
		public void AttachEntityTest2() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				function RunCode()	{
					script.Entity.AddChild(""IHaveAName"");
					script.Entity.AddChild(""IalsoHaveName"");
					script.Entity.AddChild(""IHaveAnotherName"");
					script.Entity.AddChild(""i like trains"");
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			Assert.AreEqual(4, script.Entity.children.Count);
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestOverFlowStopTwo() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				function RunCode()	{
						script.RunCode();
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestOverFlowStop() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				function RunCode()	{
					while(true){
						script.RunCode();
					}
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestWhileStop() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				function RunCode()	{
					while(true){
					}
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestTargets() {
			var script = AttachTestScript();
			var value = script.Entity.AttachComponent<ValueField<int>>();
			value.Value.Value = 12;
			var testNumber = 10232;
			script.Targets.Add().Target = value;
			tester.Step();
			script.ScriptCode.Value = @"
				function RunCode()	{
					log(""StartingValue"" + script.GetTarget(0).Value.Value);
					script.GetTarget(0).Value.Value = 10232;
					log(""NewValue"" + script.GetTarget(0).Value.Value);
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			tester.Step();
			script.Invoke("RunCode");
			Assert.AreEqual(testNumber, value.Value.Value);
			((IDisposable)tester).Dispose();
		}
		[TestMethod()]
		public void TestArguments() {
			var script = AttachTestScript();
			var value = script.Entity.AttachComponent<ValueField<int>>();
			script.Targets.Add().Target = value;
			script.ScriptCode.Value = @"
				function RunCode(arg1)	{
					script.GetTarget(0).Value.Value = arg1;
				}
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			var testNumber = 10232;
			script.Invoke("RunCode", testNumber);
			Assert.AreEqual(testNumber, value.Value.Value);
			((IDisposable)tester).Dispose();
		}
		[TestMethod()]
		public void TestBlankCode() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				
			";
			if (!script.ScriptLoaded) {
				throw new Exception("Script not loaded");
			}
			script.Invoke("RunCode");
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestBadCode() {
			var script = AttachTestScript();
			script.ScriptCode.Value = @"
				adwdhaiudhnsk nfse fse fse fhsui s()Fesfse-fds s-= fsef se9f se fs}awd [[] awd\a 
Awdawdad aw d es ;;s;ef ;sef sf s67576576 5aw 7^&^% 67da 656 7a6d %76d7 adlwa l()dawd a ()d aD()d ad()
function dwad daw da
			";
			if (script.ScriptLoaded) {
				throw new Exception("Script loaded");
			}
			script.Invoke("RunCode");
			((IDisposable)tester).Dispose();
		}

		[TestMethod()]
		public void TestMultipleFunctions() {
			var script = AttachTestScript();
			var functionAmount = 25;
			script.Functions.DisposeAtIndex(0);
			var value = script.Entity.AttachComponent<ValueField<int>>();
			script.Targets.Add().Target = value;
			var scriptcode = "";
			RLog.Info("adding functions");
			for (var i = 0; i < functionAmount; i++) {
				var func = script.Functions.Add();
				func.FunctionName.Value = $"FunctionNum{i}";
				scriptcode += $"\nfunction FunctionNum{i}() {{ script.GetTarget(0).Value.Value = {i};  }}";
			}
			RLog.Info("loading script");
			script.ScriptCode.Value = scriptcode;
			if (!script.ScriptLoaded) {
				throw new Exception("Script loaded");
			}
			RLog.Info("Running function Invoke tests");
			for (var i = 0; i < functionAmount; i++) {
				script.Functions.GetValue(i).Invoke();
				Assert.AreEqual(i, value.Value.Value);
			}

			RLog.Info("Running script Invoke tests");
			for (var i = 0; i < functionAmount; i++) {
				script.Invoke($"FunctionNum{i}");
				Assert.AreEqual(i, value.Value.Value);
			}
			Assert.AreEqual(script.Functions.Count, functionAmount);
			((IDisposable)tester).Dispose();
		}
	}
}
