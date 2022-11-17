using System;
using System.Collections.Generic;
using System.Text;
using RNumerics;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using Jint;
using Jint.Runtime.Interop;
using Jint.Native;
using System.Reflection;
using Jint.Native.Object;
using System.Threading;
using MessagePack;
using System.Linq;

namespace RhuEngine.Components
{

	public abstract class ProceduralECMAScript : ECMAScript
	{
	}

	[Category(new string[] { "RhuScript" })]
	public sealed class RawECMAScript : ECMAScript
	{

		[Default(@"
		function RunCode()	{
			
		}
		")]
		[OnChanged(nameof(InitECMA))]
		public readonly Sync<string> ScriptCode;

		protected override string Script => ScriptCode;
	}

	internal static class MathEx
	{
		private interface IObjectAdder
		{
			public object Add(object obj, object b);
		}
		private sealed class ObjectAdder<T> : IObjectAdder
		{
			public RDynamic<T> valueOne;
			public RDynamic<T> valueTwo;

			public object Add(object obj, object b) {
				T value = (RDynamic<T>)(T)obj + (RDynamic<T>)(T)b;
				return value;
			}
		}

		public static object MathAdd(this object a, object b) {
			try {
				if (a.GetType() == b.GetType()) {
					var value = ((IObjectAdder)Activator.CreateInstance(typeof(ObjectAdder<>).MakeGenericType(a.GetType()))).Add(a, b);
					return value;
				}
				return (dynamic)a + (dynamic)b;
			}
			catch {
				return null;
			}
		}
	}


	public abstract class ECMAScript : Component
	{
		public class ECMAScriptFunction : SyncObject
		{
			[Default("RunCode")]
			public readonly Sync<string> FunctionName;

			[Exposed]
			public void Invoke() {
				((ECMAScript)Parent.Parent).RunCode(FunctionName.Value);
			}

			[Exposed]
			public void Invoke(params object[] prams) {
				((ECMAScript)Parent.Parent).RunCode(FunctionName.Value, prams);
			}

			[Exposed]
			public object InvokeWithReturn() {
				return ((ECMAScript)Parent.Parent).RunCode(FunctionName.Value);
			}

			[Exposed]
			public object InvokeWithReturn(params object[] prams) {
				return ((ECMAScript)Parent.Parent).RunCode(FunctionName.Value, prams);
			}
		}

		public readonly SyncObjList<ECMAScriptFunction> Functions;

		protected override void OnAttach() {
			base.OnAttach();
			Functions.Add();
		}

		[Exposed]
		public bool ScriptLoaded => _ecma is not null;

		public readonly SyncObjList<SyncRef<IWorldObject>> Targets;

		private Jint.Engine _ecma;

		[Exposed]
		public void Invoke(string function, params object[] values) {
			RunCode(function, values);
		}
		[Exposed]
		public object InvokeWithReturn(string function, params object[] values) {
			return RunCode(function, values);
		}

		private object RunCode(string function, params object[] values) {
			object reterndata = null;
			try {
				WorldThreadSafty.MethodCalls++;
				if (WorldThreadSafty.MethodCalls > WorldThreadSafty.MaxCalls) {
					throw new StackOverflowException();
				}
				if (_ecma.GetValue(function) == JsValue.Undefined) {
					throw new Exception("function " + function + " Not found");
				}
				reterndata = _ecma.Invoke(function, values);
				WorldThreadSafty.MethodCalls--;
			}
			catch (StackOverflowException) {
				_ecma = null;
				RLog.Err("Script Err StackOverflowException");
				WorldThreadSafty.MethodCalls--;
			}
			catch (Exception ex) {
#if DEBUG
				WorldThreadSafty.MethodCalls--;
				RLog.Err("Script Err " + ex.ToString());
#endif
			}
			return reterndata;
		}

		[Exposed]
		public IWorldObject GetTarget(int index) {
			return Targets.GetValue(index).Target;
		}

		protected abstract string Script { get; }

		private object GetBestConstructor(IEnumerable<ConstructorInfo> constructors, params object[] objects) {
			foreach (var item in constructors) {
				var prams = item.GetParameters();
				var sendPrams = new object[prams.Length];
				var isSupported = true;
				for (var i = 0; i < objects.Length; i++) {
					var supported = false;
					if (prams.Length <= i) {
						if (objects[i] is null) {
							supported = true;
						}
						isSupported &= supported;
						continue;
					}
					if (objects[i]?.GetType() == prams[i].ParameterType) {
						sendPrams[i] = objects[i];
						supported = true;
					}
					if (objects[i] is null && !prams[i].ParameterType.IsValueType) {
						sendPrams[i] = objects[i];
						supported = true;
					}
					if (objects[i]?.GetType().MakeByRefType() == prams[i].ParameterType) {
						sendPrams[i] = objects[i];
						supported = true;
					}
					isSupported &= supported;
				}
				if (isSupported) {
					return item.Invoke(sendPrams);
				}
			}
			return null;
		}


		protected void InitECMA() {
			_ecma = new Jint.Engine(options => {
				options.LimitMemory(1_000_000); // alocate 1 MB
				options.TimeoutInterval(TimeSpan.FromSeconds(1));
				options.MaxStatements(3050);
				options.SetTypeResolver(new TypeResolver {
					MemberFilter = member => (Attribute.IsDefined(member, typeof(ExposedAttribute)) || Attribute.IsDefined(member, typeof(KeyAttribute)) || typeof(ISyncObject).IsAssignableFrom(member.MemberInnerType())) && !Attribute.IsDefined(member, typeof(UnExsposedAttribute)),
				});
				options.AddExtensionMethods(typeof(MathEx));
				options.Strict = true;
			});
			_ecma.ResetCallStack();
			foreach (var item in Assembly.GetAssembly(typeof(Vector2f)).GetTypes().Where(x => x.GetCustomAttribute<MessagePackObjectAttribute>() is not null)) {
				var name = item.Name;
				var functionName = $"new_{name}";
#if DEBUG
				RLog.Info($"Loaded Type ecma {name}");
#endif
				var cons = item.GetConstructors();
				_ecma.SetValue(functionName, (object val, object val2, object val3, object val4, object val5) => GetBestConstructor(cons, val, val2, val3, val4, val5));
				foreach (var field in item.GetFields()) {
					if (field.IsStatic && field.GetCustomAttribute<ExposedAttribute>() is not null) {
						_ecma.SetValue($"{name}{field.Name}", field.GetValue(null));
#if DEBUG
						RLog.Info($"Loaded Static ecma {name}{item.Name}");
#endif
					}
				}
			}
			_ecma.SetValue("script", this);
			_ecma.SetValue("entity", Entity);
			_ecma.SetValue("world", World);
			_ecma.SetValue("localUser", LocalUser);
			_ecma.SetValue("log", new Action<string>(RLog.Info));
			_ecma.SetValue("getType", (string a) => FamcyTypeParser.PraseType(a));
			_ecma.SetValue("typeOf", (object a) => a?.GetType());
			_ecma.SetValue("toString", new Func<object, string>((object a) => (a.GetType() == typeof(Type)) ? ((Type)a).GetFormattedName() : a?.ToString()));
			try {
				_ecma.Execute(Script);

			}
			catch (Exception ex) {
				_ecma = null;
				WorldThreadSafty.MethodCalls = 0;
				RLog.Err("Script Err " + ex.ToString());
			}
		}

		protected override void OnLoaded() {
			InitECMA();
		}
	}
}
