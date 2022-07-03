using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
namespace RhuEngine.Components
{
	public enum ShadowCast
	{
		Off,
		On,
		TwoSided,
		ShadowsOnly
	}

	[Category(new string[] { "Rendering" })]
	public class MeshRender : RenderingComponent
	{
		public readonly AssetRef<RMesh> mesh;
		public readonly SyncObjList<AssetRef<RMaterial>> materials;

		public readonly Sync<Colorf> colorLinear;

		[Default(RenderLayer.MainLayer)]
		public readonly Sync<RenderLayer> renderLayer;

		[Supported(SupportedFancyFeatures.Lighting)]
		[Default(ShadowCast.Off)]
		public readonly Sync<ShadowCast> CastShadows;

		[Supported(SupportedFancyFeatures.Lighting)]
		[Default(false)]
		public readonly Sync<bool> RecevieShadows;


		[Default(false)]
		[Supported(SupportedFancyFeatures.GlobalIllumination)]
		public readonly Sync<bool> ContributeGlobalIllumination;

		[Default(false)]
		[Supported(SupportedFancyFeatures.ReflectionProbes)]
		public readonly Sync<bool> ReflectionProbs;

		[Default(false)]
		[Supported(SupportedFancyFeatures.LightProbeGroup)]
		public readonly Sync<bool> LightProbs;

		public override void FirstCreation() {
			base.FirstCreation();
			colorLinear.Value = Colorf.White;
		}
	}
}
