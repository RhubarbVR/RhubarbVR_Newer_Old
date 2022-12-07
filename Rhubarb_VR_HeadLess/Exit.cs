using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
namespace Rhubarb_VR_HeadLess.Commads
{
	public sealed class Exit : Command
	{
		public override string HelpMsg => "Close RhubarbVR";

		public override Task RunCommand() {
			Program._isRunning = false;
			return Task.CompletedTask;
		}
	}
}
