using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using NAudio.Wave;

namespace RhuEngine.Components
{
	public abstract partial class AudioSourceBase : LinkedWorldComponent
	{
		public enum TargetBus:byte
		{
			World,
			Media,
			Voice
		}

		public readonly AssetRef<IWaveProvider> AudioStream;
		public readonly Sync<float> Volume;
		[Default(1.0f)]
		public readonly Sync<float> PitchScale;
		[Default(1)]
		public readonly Sync<int> MaxPolyphony;
		[Default(TargetBus.World)]
		public readonly Sync<TargetBus> AudioBus;
	}
}
