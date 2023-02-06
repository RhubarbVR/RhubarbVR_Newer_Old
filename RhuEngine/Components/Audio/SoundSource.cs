using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using NAudio.Wave;

namespace RhuEngine.Components
{
	[Category("Audio")]
	public sealed class SoundSource : LinkedWorldComponent
	{
		public readonly AssetRef<IWaveProvider> sound;

		[Default(1f)]
		public readonly Sync<float> volume;

	}
}
