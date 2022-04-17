using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRTime
	{
		public float Elapsedf { get; }
	}
	public class RTime
	{
		public static IRTime Instance { get; set; }
		public static float Elapsedf => Instance.Elapsedf;
	}
}
