using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRMicReader
	{
		public RSound SoundClip { get; }
		public void Read(ref float[] ReadSamples);
		public int SamplesAmount { get; }
		public int GetPosition { get; }
		public bool IsRecording { get; }
	}

	public interface IRMicrophone
	{
		public string[] Devices { get; }

		public bool Start(string deviceName, out IRMicReader rMicReader);
	}
	public class RMicrophone
	{
		public static IRMicrophone Instance { get; set; }

		public string[] Devices => Instance.Devices;

		public static bool Start(string deviceName,out IRMicReader rMicReader) {
			return Instance.Start(deviceName,out rMicReader);
		}
	}
}
