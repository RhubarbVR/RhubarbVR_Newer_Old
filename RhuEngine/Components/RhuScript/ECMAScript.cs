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

namespace RhuEngine.Components
{
	public class ECMAScript : Component
	{
		public class ECMAScriptFunction:SyncObject
		{
			[Default("RunCode")]
			public readonly Sync<string> FunctionName;

			[Exsposed]
			public void Invoke() {
				((ECMAScript)Parent.Parent).RunCode(FunctionName);
			}

			[Exsposed]
			public void Invoke(params object[] prams) {
				((ECMAScript)Parent.Parent).RunCode(FunctionName, prams);
			}
		}

		public readonly SyncObjList<ECMAScriptFunction> Functions;

		public override void OnAttach() {
			base.OnAttach();
			Functions.Add();
		}

		[Exsposed]
		public bool ScriptLoaded => _ecma is not null;

		public readonly SyncObjList<SyncRef<IWorldObject>> Targets;

		[Default(@"
		function RunCode()	{
			
		}
		")]
		[OnChanged(nameof(InitECMA))]
		public readonly Sync<string> Script; 

		private Jint.Engine _ecma;
		
		[Exsposed]
		public void Invoke(string function, params object[] values) {
			RunCode(function, values);
		}


		private void RunCode(string function,params object[] values) {
			try {
				WorldThreadSafty.MethodCalls++;
				if (WorldThreadSafty.MethodCalls > WorldThreadSafty.MaxCalls) {
					throw new StackOverflowException();
				}
				if (values.Length == 0) {
					_ecma.GetValue(function).Call();
				}
				else {
					_ecma.Invoke(function, values);
				}
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
		}

		[Exsposed]
		public IWorldObject GetTarget(int index) {
			return Targets.GetValue(index).Target;
		}


		private void InitECMA() {
			_ecma = new Jint.Engine(options => {
				options.LimitMemory(1_000_000); // alocate 1 MB
				options.TimeoutInterval(TimeSpan.FromSeconds(1));
				options.MaxStatements(1000);
				options.SetTypeResolver(new TypeResolver {
					MemberFilter = member => Attribute.IsDefined(member, typeof(ExsposedAttribute)) || typeof(ISyncObject).IsAssignableFrom(member.MemberInnerType()),
				});
			});
			_ecma.SetValue("script", this);
			_ecma.SetValue("entity", Entity);
			_ecma.SetValue("world", World);
			_ecma.SetValue("localUser", LocalUser);
			_ecma.SetValue("log", new Action<string>(RLog.Info));
			_ecma.SetValue("getType", (string a) => Type.GetType(a,false,true));
			_ecma.SetValue("typeOf", (object a) => a?.GetType());
			_ecma.SetValue("toString", new Func<object,string>((object a) => (a.GetType() == typeof(Type))? ((Type)a).GetFormattedName():a?.ToString()));
			try {
				 _ecma.Execute(Script.Value);
				Console.WriteLine($"LoadedScript {Script.Value}");
			}
			catch (Exception ex) {
				_ecma = null;
				WorldThreadSafty.MethodCalls = 0;
				RLog.Err("Script Err " + ex.ToString());
			}
		}

		public override void OnLoaded() {
			InitECMA();
		}
	}
}
