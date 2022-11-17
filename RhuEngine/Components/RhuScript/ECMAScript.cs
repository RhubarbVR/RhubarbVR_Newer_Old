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


		protected void InitECMA() {
			_ecma = new Jint.Engine(options => {
				options.LimitMemory(1_000_000); // alocate 1 MB
				options.TimeoutInterval(TimeSpan.FromSeconds(1));
				options.MaxStatements(3050);
				options.SetTypeResolver(new TypeResolver {
					MemberFilter = member => (Attribute.IsDefined(member, typeof(ExposedAttribute)) || Attribute.IsDefined(member, typeof(KeyAttribute)) || typeof(ISyncObject).IsAssignableFrom(member.MemberInnerType())) && !Attribute.IsDefined(member, typeof(UnExsposedAttribute)),
				});
				options.Strict = true;
			});
			_ecma.ResetCallStack();
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
