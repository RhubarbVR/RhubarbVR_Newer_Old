using System;
using System.Collections.Generic;
using System.Text;

using NAudio.Wave;

using RhuEngine.Linker;

using RNumerics;

namespace RhuEngine.Input
{
	public interface IMicDevice : IDefualtDevice, INamedDevice
	{
		public IWaveProvider WaveProvider { get; }
	}
}
