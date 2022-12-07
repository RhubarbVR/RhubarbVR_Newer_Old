using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.Components;
using RNumerics;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Commads
{
	public class Eval : Command
	{
		
		public override string HelpMsg => "Runs ecma Script";

		public override Task RunCommand() {
			if (args.Length == 1) {
				Console.WriteLine("Needs code to run");
				return Task.CompletedTask;
			}
			var enty = Manager.Engine.worldManager.FocusedWorld.GetLocalUser().userRoot.Target?.Entity ?? Manager.Engine.worldManager.FocusedWorld.RootEntity;
			var comp = enty.AttachComponent<RawECMAScript>();
			var code = FullCommand.Substring(5);
			if (code.Contains('\n') || code.Contains(';')) {
				var firstLast = code.LastIndexOf('\n');
				var secondLast = code.LastIndexOf(';');
				if (secondLast > firstLast) {
					firstLast = secondLast;
				}
				comp.ScriptCode.Value = "function Eval(){"+ code.Substring(0, firstLast) + "; return " + code.Substring(firstLast + 1)+ "}";
			}
			else {
				comp.ScriptCode.Value = "function Eval(){ return " + code + "}";
			}
			var returnData = comp.InvokeWithReturn("Eval");
			Console.WriteLine(returnData.ToString());
			comp.Destroy();
			return Task.CompletedTask;
		}
	}
}
