using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhubarb_VR_HeadLess
{
	public abstract class Command
	{
		public abstract void RunCommand();

		public abstract string HelpMsg { get; }
	}
}
