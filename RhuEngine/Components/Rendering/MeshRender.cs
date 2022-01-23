using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Rendering" })]
	public class MeshRender : RenderingComponent
	{
		public AssetRef<Mesh> mesh;
		public SyncObjList<AssetRef<Material>> materials;

		public Sync<Color> colorLinear;

		[Default(RenderLayer.All)]
		public Sync<RenderLayer> renderLayer;


		public override void FirstCreation() {
			base.FirstCreation();
			colorLinear.Value = Color.White;
		}

		public override void Render() {
			if (mesh.Asset is not null) {
				foreach (AssetRef<Material> item in materials) {
					if (item.Asset is not null) {
						mesh.Asset.Draw(item.Asset, Entity.GlobalTrans, colorLinear.Value, renderLayer.Value);
					}
				}
			}
		}
	}
}
