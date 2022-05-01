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

namespace RhuEngine.Components
{
	public class ECMAScript : Component
	{
		public SyncObjList<SyncRef<IWorldObject>> Targets;

		[Default(@"
		function RunCode()	{
			
		}
		")]
		[OnChanged(nameof(InitECMA))]
		public Sync<string> Script; 

		private Jint.Engine _ecma;

		[Exsposed]
		public void RunCode() {
			try {
				WorldThreadSafty.MethodCalls++;
				if (WorldThreadSafty.MethodCalls > 3) {
					throw new StackOverflowException();
				}
				_ecma?.Invoke("RunCode");
				WorldThreadSafty.MethodCalls--;
			}
			catch (StackOverflowException) {
				_ecma = null;
			}
			catch (Exception ex) {
#if DEBUG
				RLog.Err("Script Err " + ex.ToString());
#endif
			}
		}
		[Exsposed]
		public void RunCode(params object[] values) {
			try {
				WorldThreadSafty.MethodCalls++;
				if (WorldThreadSafty.MethodCalls > 3) {
					throw new StackOverflowException();
				}
				_ecma?.Invoke("RunCode", values);
				WorldThreadSafty.MethodCalls--;
			}
			catch (StackOverflowException) {
				_ecma = null;
			}
			catch (Exception ex) {
#if DEBUG
				RLog.Err("Script Err " + ex.ToString());
#endif
			}
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
			_ecma.SetValue("self", this);
			_ecma.SetValue("entity", Entity);
			_ecma.SetValue("world", World);
			_ecma.SetValue("localUser", LocalUser);
			_ecma.SetValue("log", new Action<string>(RLog.Info));
			_ecma.SetValue("getType", (string a) => Type.GetType(a,false,true));
			_ecma.SetValue("typeOf", (object a) => a?.GetType());
			_ecma.SetValue("toString", new Func<object,string>((object a) => (a.GetType() == typeof(Type))? ((Type)a).GetFormattedName():a?.ToString()));
			try {
				_ecma.Execute(Script.Value);
			}
			catch (Exception ex) {
				_ecma = null;
#if DEBUG
				RLog.Err("Script Err " + ex.ToString());
#endif
			}
		}

		public override void OnLoaded() {
			InitECMA();
		}
	}
}
