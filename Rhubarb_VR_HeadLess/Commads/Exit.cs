using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
namespace Rhubarb_VR_HeadLess.Commads
{
	public class Exit : Command
	{
		public override string HelpMsg => "Close RhubarbVR";

		public override void RunCommand() {
			Program.isRunning = false;
		}
	}
}
