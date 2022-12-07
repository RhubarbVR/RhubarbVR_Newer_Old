using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
namespace RhuEngine.Commads
{
	public class Trains : Command
	{
		public override string HelpMsg => "Faolan Says Hi";

		public override Task RunCommand() {
			RhuConsole.ForegroundColor = ConsoleColor.DarkMagenta;
			Console.Write("Faolan Says ");
			RhuConsole.ForegroundColor = ConsoleColor.White;
			Console.Write("Hello ");
			RhuConsole.ForegroundColor = ConsoleColor.Magenta;
			Console.Write("World");
			RhuConsole.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("!!");
			RhuConsole.ForegroundColor = ConsoleColor.White;
			return Task.CompletedTask;
		}
	}
}
