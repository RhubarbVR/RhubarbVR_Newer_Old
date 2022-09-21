using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhubarb_VR_NetFramework;

using RhuEngine;
using RhuEngine.Linker;
namespace Rhubarb_VR_HeadLess.Commads
{
	public sealed class Exit : Command
	{
		public override string HelpMsg => "Close RhubarbVR";

		public override void RunCommand() {
			Program._isRunning = false;
		}
	}
}
