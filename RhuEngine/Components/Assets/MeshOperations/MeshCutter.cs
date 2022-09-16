using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Mesh Operations" })]
	public sealed class MeshCutter : ProceduralMesh
	{
		[OnAssetLoaded(nameof(ComputeMesh))]
		public readonly AssetRef<RMesh> TargetMesh;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Plane3d> CutPlane;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> SwitchSide;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> RemoveSide;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> Cap;
		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if(TargetMesh.Asset is null) {
				return;
			}
			GenMesh(new SimpleMesh(TargetMesh.Asset.LoadedMesh).CutOnPlane(new SimpleMesh.PlaneSetting(CutPlane.Value,SwitchSide,RemoveSide, Cap)));
		}
	}
}