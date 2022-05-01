using System;
using System.Collections.Generic;
using System.Text;
using RNumerics;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using Jint;

namespace RhuEngine.Components
{
	public class ECMAScript : Component
	{
		[Default("")]
		[OnChanged(nameof(initECMA))]
		public Sync<string> script;
		private Jint.Engine _ECMA;
		private void initECMA() {
			_ECMA = new Jint.Engine(options => {
				options.LimitMemory(1_000_000); // alocate 1 MB
				options.TimeoutInterval(TimeSpan.FromSeconds(4));
				options.MaxStatements(1000);
			});
			_ECMA.SetValue("entity", Entity);
			_ECMA.SetValue("log", new Action<string>(RLog.Info));
			_ECMA.Execute(script.Value);
		}

		public override void OnLoaded() {
			initECMA();
		}
	}
}
