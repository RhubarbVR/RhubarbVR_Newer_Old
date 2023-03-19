using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Audio" })]
	public sealed partial class AudioSource3D : AudioSourceBase
	{
		public enum AttenutionModelEnum : byte
		{
			Inverse,
			InverseSquare,
			Logarithmic,
			Disabled
		}
		[Flags]
		public enum AreaMask : int
		{
			Layer1 = 1,
			Layer2 = 2,
			Layer3 = 4,
			Layer4 = 8,
			Layer5 = 16,
			Layer6 = 32,
			Layer7 = 64,
			Layer8 = 128,
			Layer9 = 256,
			Layer10 = 512,
			Layer11 = 1024,
			Layer12 = 2048,
			Layer13 = 4096,
			Layer14 = 8192,
			Layer15 = 16384,
			Layer16 = 32768,
			Layer17 = 65536,
			Layer18 = 131072,
			Layer19 = 262144,
			Layer20 = 524288,
			Layer21 = 1048576,
			Layer22 = 2097152,
			Layer23 = 4194304,
			Layer24 = 8388608,
			Layer25 = 16777216,
			Layer26 = 33554432,
			Layer27 = 67108864,
			Layer28 = 134217728,
			Layer29 = 268435456,
			Layer30 = 536870912,
			Layer31 = 1073741824,
			Layer32 = -1073741824,
		}

		public readonly Sync<AttenutionModelEnum> AttenutionModel;
		[Default(10f)]
		public readonly Sync<float> UnitSize;
		[Default(3f)]
		public readonly Sync<float> MaxdB;
		public readonly Sync<float> MaxDistance;
		[Default(1f)]
		public readonly Sync<float> PanningStrength;
		[Default(AreaMask.Layer1)]
		public readonly Sync<AreaMask> Mask;

		public readonly Sync<bool> EmissionAngleEnabled;
		[Default(45f)]
		public readonly Sync<float> EmissionAngleDegrees;
		[Default(-12f)]
		public readonly Sync<float> EmissionAngleFilterAttenuation;
		[Default(5000f)]
		public readonly Sync<float> AttenuationCutOffHz;
		[Default(-24f)]
		public readonly Sync<float> AttenuationdB;
		[Default(true)]
		public readonly Sync<bool> Doppler;

	}
}
