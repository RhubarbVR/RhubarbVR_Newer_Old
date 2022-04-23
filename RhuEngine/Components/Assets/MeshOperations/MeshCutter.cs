using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Mesh Operations" })]
	public class MeshCutter : ProceduralMesh
	{
		[OnAssetLoaded(nameof(ComputeMesh))]
		public AssetRef<RMesh> TargetMesh;
		[OnChanged(nameof(LoadMesh))]
		public Sync<Plane3d> CutPlane;
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> SwitchSide;
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> RemoveSide;

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if(TargetMesh.Asset is null) {
				return;
			}
			GenMesh(new SimpleMesh(TargetMesh.Asset.LoadedMesh).CutOnPlane(CutPlane.Value,SwitchSide,RemoveSide));
		}
	}
}