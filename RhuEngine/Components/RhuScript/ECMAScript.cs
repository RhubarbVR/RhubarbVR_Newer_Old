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
using System.Runtime.CompilerServices;

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
				options.TimeoutInterval(TimeSpan.FromSeconds(5));
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
						RLog.Info($"Loaded Static ecma {name}{field.Name}");
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

	internal static class MathEx
	{
		private interface IObjectAdder
		{
			public object UnaryPlus(object obj);

			public object UnaryNegation(object obj);

			public object Increment(object obj);

			public object Decrement(object obj);

			public object LogicalNot(object obj);
			public object OnesComplement(object obj);
			public object Addition(object obj, object b);
			public object Subtraction(object obj, object b);
			public object Multiply(object obj, object b);
			public object Division(object obj, object b);
			public object BitwiseAnd(object obj, object b);
			public object BitwiseOr(object obj, object b);
			public object ExclusiveOr(object obj, object b);
			public bool Equality(object obj, object b);
			public bool Inequality(object obj, object b);
			public bool LessThan(object obj, object b);
			public bool GreaterThan(object obj, object b);
			public bool LessThanOrEqual(object obj, object b);
			public bool GreaterThanOrEqual(object obj, object b);

			public object LeftShift(object obj, object b);
			public object RightShift(object obj, object b);
			public object Modulus(object obj, object b);
		}
		private sealed class ObjectAdder<T> : IObjectAdder
		{
			public RDynamic<T> valueOne;
			public RDynamic<T> valueTwo;

			public object Addition(object obj, object b) {
				T value = (RDynamic<T>)(T)obj + (RDynamic<T>)(T)b;
				return value;
			}

			public object BitwiseAnd(object obj, object b) {
				T value = (RDynamic<T>)(T)obj & (RDynamic<T>)(T)b;
				return value;
			}

			public object BitwiseOr(object obj, object b) {
				T value = (RDynamic<T>)(T)obj | (RDynamic<T>)(T)b;
				return value;
			}

			public object Decrement(object obj) {
				var value = (RDynamic<T>)(T)obj;
				value--;
				var re = (T)value;
				return re;
			}

			public object Division(object obj, object b) {
				T value = (RDynamic<T>)(T)obj / (RDynamic<T>)(T)b;
				return value;
			}

			public bool Equality(object obj, object b) {
				var value = (RDynamic<T>)(T)obj == (RDynamic<T>)(T)b;
				return value;
			}

			public object ExclusiveOr(object obj, object b) {
				T value = (RDynamic<T>)(T)obj ^ (RDynamic<T>)(T)b;
				return value;
			}

			public bool GreaterThan(object obj, object b) {
				var value = (RDynamic<T>)(T)obj > (RDynamic<T>)(T)b;
				return value;
			}

			public bool GreaterThanOrEqual(object obj, object b) {
				var value = (RDynamic<T>)(T)obj >= (RDynamic<T>)(T)b;
				return value;
			}

			public object Increment(object obj) {
				var value = (RDynamic<T>)(T)obj;
				value++;
				var re = (T)value;
				return re;
			}

			public bool Inequality(object obj, object b) {
				var value = (RDynamic<T>)(T)obj != (RDynamic<T>)(T)b;
				return value;
			}

			public object LeftShift(object obj, object b) {
				T value = (RDynamic<T>)(T)obj << (RDynamic<T>)(T)b;
				return value;
			}

			public bool LessThan(object obj, object b) {
				var value = (RDynamic<T>)(T)obj < (RDynamic<T>)(T)b;
				return value;
			}

			public bool LessThanOrEqual(object obj, object b) {
				var value = (RDynamic<T>)(T)obj <= (RDynamic<T>)(T)b;
				return value;
			}

			public object LogicalNot(object obj) {
				T value = !(RDynamic<T>)(T)obj;
				return value;
			}

			public object Modulus(object obj, object b) {
				T value = (RDynamic<T>)(T)obj % (RDynamic<T>)(T)b;
				return value;
			}

			public object Multiply(object obj, object b) {
				T value = (RDynamic<T>)(T)obj * (RDynamic<T>)(T)b;
				return value;
			}

			public object OnesComplement(object obj) {
				T value = ~(RDynamic<T>)(T)obj;
				return value;
			}

			public object RightShift(object obj, object b) {
				T value = (RDynamic<T>)(T)obj >> (RDynamic<T>)(T)b;
				return value;
			}

			public object Subtraction(object obj, object b) {
				T value = (RDynamic<T>)(T)obj - (RDynamic<T>)(T)b;
				return value;
			}

			public object UnaryNegation(object obj) {
				T value = -(RDynamic<T>)(T)obj;
				return value;
			}

			public object UnaryPlus(object obj) {
				T value = +(RDynamic<T>)(T)obj;
				return value;
			}
		}

		public static object OP_UnaryPlus(this object obj) {
			var value = GetManager(obj.GetType()).UnaryPlus(obj);
			return value;
		}
		public static object OP_UnaryNegation(this object obj) {
			var value = GetManager(obj.GetType()).UnaryNegation(obj);
			return value;
		}
		public static object OP_Increment(this object obj) {
			var value = GetManager(obj.GetType()).Increment(obj);
			return value;
		}
		public static object OP_Decrement(this object obj) {
			var value = GetManager(obj.GetType()).Decrement(obj);
			return value;
		}
		public static object OP_LogicalNot(this object obj) {
			var value = GetManager(obj.GetType()).LogicalNot(obj);
			return value;
		}
		public static object OP_OnesComplement(this object obj) {
			var value = GetManager(obj.GetType()).OnesComplement(obj);
			return value;
		}
		public static object OP_Addition(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).Addition(obj, b);
					return value;
				}
				return (dynamic)obj + (dynamic)b;
			}
			catch {
				return null;
			}
		}
		public static object OP_Subtraction(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).Subtraction(obj, b);
					return value;
				}
				return (dynamic)obj - (dynamic)b;
			}
			catch {
				return null;
			}
		}
		public static object OP_Multiply(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).Multiply(obj, b);
					return value;
				}
				return (dynamic)obj * (dynamic)b;
			}
			catch {
				return null;
			}
		}
		public static object OP_Division(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).Division(obj, b);
					return value;
				}
				return (dynamic)obj / (dynamic)b;
			}
			catch {
				return null;
			}
		}
		public static object OP_BitwiseAnd(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).BitwiseAnd(obj, b);
					return value;
				}
				return (dynamic)obj & (dynamic)b;
			}
			catch {
				return null;
			}
		}
		public static object OP_BitwiseOr(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).BitwiseOr(obj, b);
					return value;
				}
				return (dynamic)obj | (dynamic)b;
			}
			catch {
				return null;
			}
		}
		public static object OP_ExclusiveOr(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).ExclusiveOr(obj, b);
					return value;
				}
				return (dynamic)obj ^ (dynamic)b;
			}
			catch {
				return null;
			}
		}
		public static bool OP_Equality(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).Equality(obj, b);
					return value;
				}
				return (dynamic)obj == (dynamic)b;
			}
			catch {
				return false;
			}
		}
		public static bool OP_Inequality(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).Inequality(obj, b);
					return value;
				}
				return (dynamic)obj != (dynamic)b;
			}
			catch {
				return false;
			}
		}
		public static bool OP_LessThan(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).LessThan(obj, b);
					return value;
				}
				return (dynamic)obj < (dynamic)b;
			}
			catch {
				return false;
			}
		}
		public static bool OP_GreaterThan(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).GreaterThan(obj, b);
					return value;
				}
				return (dynamic)obj > (dynamic)b;
			}
			catch {
				return false;
			}
		}
		public static bool OP_LessThanOrEqual(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).LessThanOrEqual(obj, b);
					return value;
				}
				return (dynamic)obj <= (dynamic)b;
			}
			catch {
				return false;
			}
		}
		public static bool OP_GreaterThanOrEqual(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).GreaterThanOrEqual(obj, b);
					return value;
				}
				return (dynamic)obj >= (dynamic)b;
			}
			catch {
				return false;
			}
		}
		public static object OP_LeftShift(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).LeftShift(obj, b);
					return value;
				}
				return (dynamic)obj << (dynamic)b;
			}
			catch {
				return false;
			}
		}
		public static object OP_RightShift(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).RightShift(obj, b);
					return value;
				}
				return (dynamic)obj >> (dynamic)b;
			}
			catch {
				return false;
			}
		}
		public static object OP_Modulus(this object obj, object b) {
			try {
				if (obj.GetType() == b.GetType()) {
					var value = GetManager(obj.GetType()).Modulus(obj, b);
					return value;
				}
				return (dynamic)obj % (dynamic)b;
			}
			catch {
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static IObjectAdder GetManager(Type type) {
			return (IObjectAdder)Activator.CreateInstance(typeof(ObjectAdder<>).MakeGenericType(type));
		}
	}

}
