using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RhuEngine.Physics;
using MessagePack;
using MessagePack.Resolvers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NullContext;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using SharedModels.GameSpecific;
using System.Reflection;
using RhuEngine.Datatypes;

namespace RhuEngine.GameTests.Tests
{
	[TestClass()]
	public class GenericGameTest
	{
		public GenericGameTester tester;

		[TestMethod()]
		public void StartUpAndRunTest() {
			tester = new GenericGameTester();
			tester.Start(Array.Empty<string>());
			tester.RunForSteps(20);
			tester.app.startingthread.Join();
			tester.Dispose();
		}

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
			var newEntity = newworld.RootEntity.AddChild("TestEntity");
			Assert.IsNotNull(newEntity);
			return newEntity;
		}
		[TestMethod()]
		public void StartNewTestWorldTest() {
			StartNewTestWorld();
			tester.Dispose();
		}

		[TestMethod()]
		public void AttachEntityTest() {
			AttachEntity();
			tester.Dispose();
		}

		[TestMethod()]
		public void StandardEntityTest() {
			var newworld = StartNewTestWorld();
			var TestEntity = newworld.RootEntity.AddChild("TestEntity");
			var TestEntity2 = newworld.RootEntity.GetChildByName("TestEntity");
			Assert.AreEqual(TestEntity, TestEntity2);
			tester.Dispose();
		}

		public IEnumerable<Type> GetAllTypes(Func<Type, bool> func) {
			return from asm in AppDomain.CurrentDomain.GetAssemblies()
				   from type in asm.GetTypes()
				   where func.Invoke(type)
				   select type;
		}


		public List<Type> MakeTestGenerics(Type type) {
			var TestTypes = new List<Type>();
			var newType = type;
			var arguments = type.GetGenericArguments();
			var inputTypes = new List<Type>[arguments.Length];
			List<Type> GetDeffrentTypeConstrantes(Type type) {
				var types = new List<Type>();

				if (type.GetGenericParameterConstraints().Length == 0) {
					types.Add(typeof(string));
					types.Add(typeof(int));
					types.Add(typeof(Vector2d));
					types.Add(typeof(World));
					types.Add(typeof(Vector3d));
					types.Add(typeof(Vector4d));
					types.Add(typeof(RTexture2D));
					types.Add(typeof(Type));
					types.Add(typeof(EditLevel));
				}
				else {
					var rnd = new Random();
					var groupTypes = GetAllTypes((testtype) => {
						if (testtype.IsGenericType) {
							return false;
						}
						foreach (var item in type.GetGenericParameterConstraints()) {
							if (!item.IsAssignableFrom(testtype)) {
								return false;
							}
						}
						return true;
					}).OrderBy(x => rnd.Next()).Take(25);
					types.AddRange(groupTypes);
				}
				return types;
			}
			for (var i = 0; i < inputTypes.Length; i++) {
				inputTypes[i] = GetDeffrentTypeConstrantes(arguments[i]);
			}
			var tempTypeInputs = new Type[inputTypes.Length];
			void AddTypes(int index) {
				if (index == inputTypes.Length - 1) {
					foreach (var curentitem in inputTypes[index]) {
						tempTypeInputs[index] = curentitem;
						try {
							TestTypes.Add(type.MakeGenericType(tempTypeInputs));
						}
						catch { }
					}
					return;
				}
				foreach (var curentitem in inputTypes[index]) {
					tempTypeInputs[index] = curentitem;
					AddTypes(index + 1);
				}
			}
			AddTypes(0);
			return TestTypes;
		}

		public void RunComponentTest(Component component) {
			RunSyncObjectTest(component);
			Assert.IsNotNull(component);
		}
		public void RunSyncObjectTest(SyncObject syncObject) {
			syncObject.OnSave();
			var serlize = syncObject.Serialize(new SyncObjectSerializerObject(true));
			syncObject.Deserialize(serlize, new SyncObjectDeserializerObject(true));
			Assert.IsNotNull(syncObject);
		}
		public interface ITestSyncObject
		{
			public SyncObject GetObject();
		}

		public class TestSyncObject<T> : Component, ITestSyncObject where T : SyncObject
		{
			public readonly T SyncObject;

			public SyncObject GetObject() {
				return SyncObject;
			}
		}

		[TestMethod()]
		public void TestAllSyncObjects() {
			var testEntity = AttachEntity();
			var SyncObjecs = GetAllTypes((type) => !type.IsAbstract && !type.IsInterface && typeof(ISyncObject).IsAssignableFrom(type) && !typeof(Component).IsAssignableFrom(type));
			foreach (var item in SyncObjecs) {
				if (item.IsGenericType) {
					foreach (var testType in MakeTestGenerics(item)) {
						Console.WriteLine("Testing SyncObjects " + testType.GetFormattedName());
						ITestSyncObject e = null;
						try {
							try {
								e = (ITestSyncObject)testEntity.AttachComponent<Component>(typeof(TestSyncObject<>).MakeGenericType(testType));
							}
							catch {
								if (testType.GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) == null) {
									throw;
								}
							}
						}
						catch (Exception normalex) {
							if(normalex.InnerException.GetType() != typeof(SyncObject.NotVailedGenaric)) {
								throw;
							}
							RLog.Warn("used Invailed Genaric Type");
							continue;
						}
						if (testType.GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) != null && e != null) {
							throw new Exception("Loaded PrivateSpaceOnly object");
						}
						RunSyncObjectTest(e.GetObject());
					}
				}
				else {
					Console.WriteLine("Testing SyncObjects " + item.GetFormattedName());
					ITestSyncObject e = null;
					try {
						e = (ITestSyncObject)testEntity.AttachComponent<Component>(typeof(TestSyncObject<>).MakeGenericType(item));
					}
					catch {
						if (item.GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) == null) {
							throw;
						}
					}
					if (item.GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) != null && e != null) {
						throw new Exception("Loaded PrivateSpaceOnly object");
					}
					RunSyncObjectTest(e.GetObject());
				}
			}
			tester.RunForSteps();
			tester.Dispose();
		}

		[TestMethod()]
		public void TestSyncList() {
			var testEntity = AttachEntity();
			var synclist = testEntity.AttachComponent<TestSyncObject<SyncObjList<Sync<float>>>>();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.Add();
			synclist.SyncObject.DisposeAtIndex(0);
			synclist.SyncObject.DisposeAtIndex(0);
			synclist.SyncObject.DisposeAtIndex(0);
			Assert.AreEqual(7, synclist.SyncObject.Count);
			tester.RunForSteps();
			tester.Dispose();
		}

		//Todo: fix problem with RigidBody
		//[TestMethod]
		//public void RiggedBodyTest() {
		//	var testWorld = StartNewTestWorld();
		//	var box = new RBoxShape(0.5f);
		//	var startPos = Matrix.TS(Vector3f.One, 1);
		//	var collider = box.GetCollider(testWorld.PhysicsSim, startPos);
		//	collider.NoneStaticBody = true;
		//	collider.Mass = 100f;
		//	collider.Active = true;
		//	tester.RunForSteps(2);
		//	Assert.AreNotEqual(startPos, collider.Matrix);
		//	tester.Dispose();
		//}


		[TestMethod]
		public void ConvexRayTest() {
			var testWorld = StartNewTestWorld();
			var box = new RBoxShape(0.5f);
			var collider = box.GetCollider(testWorld.PhysicsSim, Matrix.TS(Vector3f.Zero, 1), "Trains");
			var pointworld = Vector3f.AxisX * -5;
			var OuterPoint = Vector3f.AxisX * 5;
			tester.RunForSteps(2);
			var outer = Matrix.T(OuterPoint);
			var startpoint = Matrix.T(pointworld);
			var hashit = testWorld.PhysicsSim.ConvexRayTest(box, ref startpoint, ref outer, out var hitcollider, out var hitnorm, out var worldhit);
			Assert.IsTrue(hashit);
			Assert.AreEqual("Trains", hitcollider.CustomObject);
			tester.RunForSteps(2);
			tester.Dispose();
		}

		[TestMethod]
		public void RayTestWithConvexMesh() {
			var testWorld = StartNewTestWorld();
			var gen = new TrivialBox3Generator {
				Box = new Box3d(Vector3f.Zero, Vector3f.One)
			};
			gen.Generate();
			var colider = new RConvexMeshShape(gen.MakeSimpleMesh());
			var collider = colider.GetCollider(testWorld.PhysicsSim, Matrix.TS(Vector3f.Zero, 1), "Trains");
			var pointworld = Vector3f.AxisX * -5;
			var OuterPoint = Vector3f.AxisX * 5;
			tester.RunForSteps();
			var hashit = testWorld.PhysicsSim.RayTest(ref OuterPoint, ref pointworld, out var hitcollider, out var hitnorm, out var worldhit);
			Assert.IsTrue(hashit);
			Assert.AreEqual("Trains", hitcollider.CustomObject);
			tester.RunForSteps();
			tester.Dispose();
		}
		[TestMethod]
		public void RayTestWithRawMesh() {
			var testWorld = StartNewTestWorld();
			var gen = new TrivialBox3Generator {
				Box = new Box3d(Vector3f.Zero, Vector3f.One)
			};
			gen.Generate();
			var colider = new RRawMeshShape(gen.MakeSimpleMesh());
			var collider = colider.GetCollider(testWorld.PhysicsSim, Matrix.TS(Vector3f.Zero, 1), "Trains");
			var pointworld = Vector3f.AxisX * -5;
			var OuterPoint = Vector3f.AxisX * 5;
			tester.RunForSteps();
			var hashit = testWorld.PhysicsSim.RayTest(ref OuterPoint, ref pointworld, out var hitcollider, out var hitnorm, out var worldhit);
			Assert.IsTrue(hashit);
			Assert.AreEqual("Trains", hitcollider.CustomObject);
			tester.RunForSteps();
			tester.Dispose();
		}


		[TestMethod]
		public void RayTest() {
			var testWorld = StartNewTestWorld();
			var box = new RBoxShape(0.5f);
			var collider = box.GetCollider(testWorld.PhysicsSim, Matrix.TS(Vector3f.Zero, 1), "Trains");
			var pointworld = Vector3f.AxisX * -5;
			var OuterPoint = Vector3f.AxisX * 5;
			tester.RunForSteps();
			var hashit = testWorld.PhysicsSim.RayTest(ref pointworld, ref OuterPoint, out var hitcollider, out var hitnorm, out var worldhit);
			Assert.IsTrue(hashit);
			Assert.AreEqual("Trains", hitcollider.CustomObject);
			tester.RunForSteps();
			tester.Dispose();
		}

		[TestMethod()]
		public void TestMultiOperatorsComponentNetPointer() {
			var testEntity = AttachEntity();
			var inttest = testEntity.AttachComponent<Components.MultiOperators<NetPointer, NetPointer>>();
			var output = testEntity.AttachComponent<Components.ValueField<NetPointer>>();
			inttest.Output.Target = output.Value;
			var random = new Random();
			var inputs = testEntity.AttachComponent<Components.ValueList<NetPointer>>();
			var one = inputs.Value.Add();
			var valueone = one.Value = new NetPointer((ulong)random.Next(0, 1000000));
			inttest.Inputs.Add().Target = one;

			var two = inputs.Value.Add();
			var valuetwo = two.Value = new NetPointer((ulong)random.Next(0, 1000000));
			inttest.Inputs.Add().Target = two;

			var three = inputs.Value.Add();
			var valueThree = three.Value = new NetPointer((ulong)random.Next(0, 1000000));
			inttest.Inputs.Add().Target = three;

			inttest.Operators.Value = Components.MultiOperators.Addition;
			Assert.AreNotEqual(output.Value, new NetPointer(valueone.id + valuetwo.id + valueThree.id));

			tester.RunForSteps();
			tester.Dispose();
		}

		[TestMethod()]
		public void TestMultiOperatorsComponentVector() {
			var testEntity = AttachEntity();
			var inttest = testEntity.AttachComponent<Components.MultiOperators<Vector3f, Vector3f>>();
			var output = testEntity.AttachComponent<Components.ValueField<Vector3f>>();
			inttest.Output.Target = output.Value;
			var random = new Random();
			var inputs = testEntity.AttachComponent<Components.ValueList<Vector3f>>();
			var one = inputs.Value.Add();
			var valueone = one.Value = new Vector3f(random.NextDouble(), random.NextDouble(), random.NextDouble());
			inttest.Inputs.Add().Target = one;

			var two = inputs.Value.Add();
			var valuetwo = two.Value = new Vector3f(random.NextDouble(), random.NextDouble(), random.NextDouble());
			inttest.Inputs.Add().Target = two;

			var three = inputs.Value.Add();
			var valueThree = three.Value = new Vector3f(random.NextDouble(), random.NextDouble(), random.NextDouble());
			inttest.Inputs.Add().Target = three;

			inttest.Operators.Value = Components.MultiOperators.Addition;
			Assert.AreEqual(output.Value, valueone + valuetwo + valueThree);

			inttest.Operators.Value = Components.MultiOperators.Subtraction;
			Assert.AreEqual(output.Value, valueone - valuetwo - valueThree);

			inttest.Operators.Value = Components.MultiOperators.Multiplication;
			Assert.AreEqual(output.Value, valueone * valuetwo * valueThree);

			inttest.Operators.Value = Components.MultiOperators.Division;
			Assert.AreEqual(output.Value, valueone / valuetwo / valueThree);

			tester.RunForSteps();
			tester.Dispose();
		}

		[TestMethod()]
		public void TestMultiOperatorsComponentInt() {
			var testEntity = AttachEntity();
			var inttest = testEntity.AttachComponent<Components.MultiOperators<int, int>>();
			var output = testEntity.AttachComponent<Components.ValueField<int>>();
			inttest.Output.Target = output.Value;
			var random = new Random();
			var inputs = testEntity.AttachComponent<Components.ValueList<int>>();
			var one = inputs.Value.Add();
			var valueone = one.Value = random.Next(-100, 100);
			inttest.Inputs.Add().Target = one;

			var two = inputs.Value.Add();
			var valuetwo = two.Value = random.Next(-100, 100);
			inttest.Inputs.Add().Target = two;

			var three = inputs.Value.Add();
			var valueThree = three.Value = random.Next(-100, 100);
			inttest.Inputs.Add().Target = three;

			inttest.Operators.Value = Components.MultiOperators.Addition;
			Assert.AreEqual(output.Value, valueone + valuetwo + valueThree);

			inttest.Operators.Value = Components.MultiOperators.Subtraction;
			Assert.AreEqual(output.Value, valueone - valuetwo - valueThree);

			inttest.Operators.Value = Components.MultiOperators.Multiplication;
			Assert.AreEqual(output.Value, valueone * valuetwo * valueThree);

			inttest.Operators.Value = Components.MultiOperators.Division;
			Assert.AreEqual(output.Value, valueone / valuetwo / valueThree);

			inttest.Operators.Value = Components.MultiOperators.Modulus;
			Assert.AreEqual(output.Value, valueone % valuetwo % valueThree);

			inttest.Operators.Value = Components.MultiOperators.LogicalOR;
			Assert.AreEqual(output.Value, valueone | valuetwo | valueThree);

			inttest.Operators.Value = Components.MultiOperators.LogicalExclusiveOR;
			Assert.AreEqual(output.Value, valueone ^ valuetwo ^ valueThree);

			inttest.Operators.Value = Components.MultiOperators.LogicalAND;
			Assert.AreEqual(output.Value, valueone & valuetwo & valueThree);

			tester.RunForSteps();
			tester.Dispose();
		}

		[TestMethod()]
		public void TestAllComponents() {
			var testEntity = AttachEntity();
			var components = GetAllTypes((type) => !type.IsAbstract && !type.IsInterface && typeof(Component).IsAssignableFrom(type));
			foreach (var item in components) {
				if (typeof(ITestSyncObject).IsAssignableFrom(item)) {
					continue;
				}
				if (item.IsGenericType) {
					foreach (var testes in MakeTestGenerics(item)) {
						Console.WriteLine("Testing Component " + testes.GetFormattedName());
						Component comp = null;
						try {
							try {
								comp = testEntity.AttachComponent<Component>(testes);
								RunComponentTest(comp);
							}
							catch {
								if (testes.GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) == null) {
									throw;
								}
							}
						}
						catch (SyncObject.NotVailedGenaric) {
							RLog.Warn("used Invailed Genaric Type");
							continue;
						}
						if (testes.GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) != null && comp != null) {
							throw new Exception("Loaded PrivateSpaceOnly object");
						}
					}
				}
				else {
					Console.WriteLine("Testing Component " + item.GetFormattedName());
					Component comp = null;
					try {
						comp = testEntity.AttachComponent<Component>(item);
						RunComponentTest(comp);
					}
					catch {
						if (item.GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) == null) {
							throw;
						}
					}
					if (item.GetCustomAttribute<PrivateSpaceOnlyAttribute>(true) != null && comp != null) {
						throw new Exception("Loaded PrivateSpaceOnly object");
					}
				}
			}
			tester.RunForSteps();
			tester.Dispose();
		}

		public static bool RunAcountLoginAndCreation = false;

		[TestMethod()]
		public async Task TestAcountLoginAndCreation() {
			SetUpForNormalTest();
			if (!RunAcountLoginAndCreation) {
				tester.RunForSteps();
				tester.Dispose();
				return;
			}
			var userName = "AutoRemovedTestAcount" + Guid.NewGuid().ToString();
			var email = $"{userName}@AutoRemovedTestAcount.rhubarbvr.net";
			var password = "Password" + Guid.NewGuid().ToString();
			var dateOfBirth = DateTime.Now.AddYears(-100);
			var signup = await tester.app.netApiManager.SignUp(userName, email, password, dateOfBirth);
			if (signup.Error) {
				throw new Exception("Failed to createAcount" + signup.Message + $"\n{signup.ErrorDetails}");
			}
			var login = await tester.app.netApiManager.Login(email, password);
			if (!login.Login) {
				throw new Exception("Failed to login" + login.Message);
			}
			RLog.Info("Login as " + userName);
			Assert.AreEqual(userName, tester.app.netApiManager.User.UserName);
			tester.RunForSteps();
			tester.Dispose();
		}
	}
}
