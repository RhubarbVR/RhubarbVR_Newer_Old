using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;

namespace Rhubarb_VR_HeadLess.Commads
{
	public class Help : Command
	{
		public override string HelpMsg => "Lists all commands and what they do";
		public override void RunCommand() {
			Console.WriteLine($"  ======  Help {Program._app.version}  ======");
			Console.WriteLine("");
			foreach (var comand in Program._commands) {
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("  " + comand.Name);
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine(" : "+ ((Command)Activator.CreateInstance(comand)).HelpMsg);
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("");
			Console.WriteLine("  --------===========--------");
		}
	}
}
