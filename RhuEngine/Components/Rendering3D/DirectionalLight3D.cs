using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;

namespace RhuEngine.Components
{

	[Category(new string[] { "Rendering3D" })]
	public sealed partial class DirectionalLight3D : Light3D
	{
		public enum DirectionalLightMode : byte
		{
			Light,
			Sky,
			LightAndSky
		}

		[Default(DirectionalLightMode.LightAndSky)]
		public readonly Sync<DirectionalLightMode> SkyMode;

		public enum DirectionalLightShadowMode : byte
		{
			Orthongonal,
			PSSM2Splits,
			PSSM4Splits
		}

		[Default(DirectionalLightShadowMode.Orthongonal)]
		public readonly Sync<DirectionalLightShadowMode> ShadowMode;

		[Default(0.1f)]
		public readonly Sync<float> SplitOne;

		[Default(0.2f)]
		public readonly Sync<float> SplitTwo;

		[Default(0.5f)]
		public readonly Sync<float> SplitThree;

		public readonly Sync<bool> BlendSplits;

		[Default(0.8f)]
		public readonly Sync<float> FadeStart;

		[Default(100f)]
		public readonly Sync<float> MaxDistance;

		[Default(20f)]
		public readonly Sync<float> PancakeSize;
	}
}
