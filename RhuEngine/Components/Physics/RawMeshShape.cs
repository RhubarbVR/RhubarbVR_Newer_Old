using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class RawMeshShape : PhysicsObject
	{
		[OnAssetLoaded(nameof(RebuildPysics))]
		public readonly AssetRef<RMesh> TargetMesh;

		public override ColliderShape PysicsBuild() {
			return TargetMesh.Target is null
				? null
				: TargetMesh.Asset is null
				? null
				: TargetMesh.Asset.LoadedMesh is null ? null : (ColliderShape)new RRawMeshShape(TargetMesh.Asset.LoadedMesh);
		}
	}
}
