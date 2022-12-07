using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;

namespace RhuEngine.Commads
{
	public class Help : Command
	{
		public override string HelpMsg => "Lists all commands and what they do";
		public override Task RunCommand() {
			Console.WriteLine($"  ======  Help {Engine.MainEngine.version}  ======");
			Console.WriteLine("");
			foreach (var comand in Manager._commands) {
				RhuConsole.ForegroundColor = ConsoleColor.White;
				Console.Write("  " + comand.Name);
				RhuConsole.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine(" : "+ ((Command)Activator.CreateInstance(comand)).HelpMsg);
			}
			RhuConsole.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("");
			Console.WriteLine("  --------===========--------");
			return Task.CompletedTask;
		}
	}
}
