using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Linq;
namespace RhuEngine.Components
{
	[Category(new string[] { "Rendering" })]
	public class MeshRender : RenderingComponent
	{
		public readonly AssetRef<RMesh> mesh;
		public readonly SyncObjList<AssetRef<RMaterial>> materials;

		public readonly Sync<Colorf> colorLinear;

		[Default(RenderLayer.All)]
		public readonly Sync<RenderLayer> renderLayer;


		public override void FirstCreation() {
			base.FirstCreation();
			colorLinear.Value = Colorf.White;
		}
	}
}
