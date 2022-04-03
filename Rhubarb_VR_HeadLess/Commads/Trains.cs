using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
namespace Rhubarb_VR_HeadLess.Commads
{
	public class Trains : Command
	{
		public override string HelpMsg => "Faolan Says Hi";

		public override void RunCommand() {
			Console.ForegroundColor = ConsoleColor.DarkMagenta;
			Console.Write("Faolan Says ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write("Hello ");
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.Write("World");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("!!");
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}
