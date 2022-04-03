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
		public AssetRef<RMesh> mesh;
		public SyncObjList<AssetRef<RMaterial>> materials;

		public Sync<Colorf> colorLinear;

		[Default(RenderLayer.All)]
		public Sync<RenderLayer> renderLayer;


		public override void FirstCreation() {
			base.FirstCreation();
			colorLinear.Value = Colorf.White;
		}
	}
}
