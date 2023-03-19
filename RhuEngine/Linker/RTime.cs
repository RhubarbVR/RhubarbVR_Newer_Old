using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRTime
	{
		public double Elapsed { get; }
	}
	public static class RTime
	{
		public static IRTime Instance { get; set; }
		public static double Elapsed => Instance.Elapsed;
		public static float ElapsedF => (float)Instance.Elapsed;

	}
}
