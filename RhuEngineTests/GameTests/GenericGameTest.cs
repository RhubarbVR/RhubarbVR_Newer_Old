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

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using SharedModels.GameSpecific;

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
			tester.Dispose();
		}

		private void SetUpForNormalTest() {
			tester = new GenericGameTester();
			tester.Start(Array.Empty<string>());
			tester.RunForSteps(5);
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

		public IEnumerable<Type> GetAllTypes(Func<Type,bool> func) {
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

		public class TestSyncObject<T>:Component, ITestSyncObject where T : SyncObject {
			public T SyncObject;

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
						var e = (ITestSyncObject)testEntity.AttachComponent<Component>(typeof(TestSyncObject<>).MakeGenericType(testType));
						RunSyncObjectTest(e.GetObject());
					}
				}
				else {
					Console.WriteLine("Testing SyncObjects " + item.GetFormattedName());
					var e = (ITestSyncObject)testEntity.AttachComponent<Component>(typeof(TestSyncObject<>).MakeGenericType(item));
					RunSyncObjectTest(e.GetObject());
				}
			}
			tester.RunForSteps();
			tester.Dispose();
		}

		[TestMethod()]
		public void TestAllComponents() {
			var testEntity = AttachEntity();
			var components = GetAllTypes((type) => !type.IsAbstract && !type.IsInterface && typeof(Component).IsAssignableFrom(type));
			foreach (var item in components) {
				if (typeof(ITestSyncObject).IsAssignableFrom(item)) {

				}
				else if (item.IsGenericType) {
					foreach (var testes in MakeTestGenerics(item)) {
						Console.WriteLine("Testing Component " + testes.GetFormattedName());
						var comp = testEntity.AttachComponent<Component>(testes);
						RunComponentTest(comp);
					}
				}
				else {
					Console.WriteLine("Testing Component " + item.GetFormattedName());
					var comp = testEntity.AttachComponent<Component>(item);
					RunComponentTest(comp);
				}
			}
			tester.RunForSteps();
			tester.Dispose();
		}

		[TestMethod()]
		public async Task TestAcountLoginAndCreation() {
			SetUpForNormalTest();
			var userName = "AutoRemovedTestAcount"+Guid.NewGuid().ToString();
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
