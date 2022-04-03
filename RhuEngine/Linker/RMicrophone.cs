using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRMicrophone
	{
		public RSound RSound { get; }

		public bool Start(string deviceName);
	}
	public class RMicrophone
	{
		public static IRMicrophone Instance { get; set; }

		public static RSound Sound => Instance.RSound;

		public static bool Start(string deviceName) {
			return Instance.Start(deviceName);
		}
	}
}
