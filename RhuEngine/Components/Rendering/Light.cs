using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;

namespace RhuEngine.Components
{
	public enum RLightType
	{
		Spot,
		Directional,
		Point,
	}
	public enum ShadowMode
	{
		Off,
		Hard,
		Soft,
	}

	[Supported(SupportedFancyFeatures.Lighting)]
	[Category(new string[] { "Rendering" })]
	public class Light : RenderingComponent
	{
		[Default(RLightType.Point)]
		public readonly Sync<RLightType> LightType;

		//SpotLight Point
		[Default(10f)]
		public readonly Sync<float> Range;
		[Default(30f)]
		public readonly Sync<float> SpotAngle;

		//Directional
		[Default(10f)]
		public readonly Sync<float> Size;

		[Default(1f)]
		public readonly Sync<float> Intensity;
		[Default(1f)]
		public readonly Sync<float> IndirectMultipiler;

		public readonly Sync<Colorf> Color;

		public readonly Sync<ShadowMode> ShadowType;

		[Supported(SupportedFancyFeatures.LightCookie)]
		public readonly AssetRef<RTexture2D> LightCookie;

		public readonly Sync<RenderLayer> Culling;

		public override void FirstCreation() {
			base.FirstCreation();
			Color.Value = Colorf.White;
		}
	}
}
