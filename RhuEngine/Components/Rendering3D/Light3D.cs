using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;

namespace RhuEngine.Components
{
	public enum BakeMode: byte
	{
		Disable,
		Static,
		Dynamic
	}

	[Category(new string[] { "Rendering3D" })]
	public abstract partial class Light3D : VisualInstance3D
	{
		public readonly Sync<Colorf> Color;

		[Default(1f)]
		public readonly Sync<float> Energy;

		[Default(1f)]
		public readonly Sync<float> IndirectEnergy;

		[Default(1f)]
		public readonly Sync<float> VolumetricFogEnergy;

		public readonly AssetRef<RTexture2D> Projector;

		public readonly Sync<float> Size;

		public readonly Sync<bool> Negative;

		[Default(0.5f)]
		public readonly Sync<float> Specular;

		public readonly Sync<BakeMode> BakeMode;

		[Default(RenderLayer.MainCam)]
		public readonly Sync<RenderLayer> CullMask;

		public readonly Sync<bool> ShadowEnabled;

		[Default(0.03f)]
		public readonly Sync<float> ShadowBias;

		[Default(1f)]
		public readonly Sync<float> ShadowNormalBias;

		public readonly Sync<bool> ShadowReverseCullFace;

		[Default(0.05f)]
		public readonly Sync<float> ShadowTransmittanceBias;

		[Default(1f)]
		public readonly Sync<float> ShadowOpacity;

		[Default(1f)]
		public readonly Sync<float> ShadowBlur;

		public readonly Sync<bool> DistanceFadeEnabled;

		[Default(40f)]
		public readonly Sync<float> DistanceFadeBegin;


		[Default(50f)]
		public readonly Sync<float> DistanceFadeShadow;


		[Default(10f)]
		public readonly Sync<float> DistanceFadeLength;

		protected override void FirstCreation() {
			base.FirstCreation();
			Color.Value = Colorf.White;
		}
	}
}
