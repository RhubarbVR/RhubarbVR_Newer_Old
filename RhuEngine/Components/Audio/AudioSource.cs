using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using NAudio.Wave;

namespace RhuEngine.Components
{
	[Category(new string[] { "Audio" })]
	public sealed partial class AudioSource : AudioSourceBase
	{
		public enum TargetSource : byte
		{
			Stereo,
			Surround,
			Center
		}
		public readonly Sync<TargetSource> MixTarget;
	}
}
