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

	[Category(new string[] { "Rendering3D" })]
	public class MeshRender : LinkedWorldComponent, IWorldBoundingBox
	{
		public readonly AssetRef<RMesh> mesh;
		public readonly SyncObjList<AssetRef<RMaterial>> materials;

		public readonly Sync<Colorf> colorLinear;
		public readonly Sync<int> zOrderOffset;

		[Default(RenderLayer.MainLayer)]
		public readonly Sync<RenderLayer> renderLayer;

		[Default(ShadowCast.Off)]
		public readonly Sync<ShadowCast> CastShadows;

		[Default(false)]
		public readonly Sync<bool> RecevieShadows;

		[Default(false)]
		public readonly Sync<bool> ReflectionProbs;

		[Default(false)]
		public readonly Sync<bool> LightProbs;

		public AxisAlignedBox3f Bounds => mesh.Asset?.BoundingBox??AxisAlignedBox3f.CenterZero;

		protected override void FirstCreation() {
			base.FirstCreation();
			colorLinear.Value = Colorf.White;
		}
	}
}
