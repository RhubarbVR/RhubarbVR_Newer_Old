using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class ConvexMeshShape : PhysicsObject
	{
		[OnAssetLoaded(nameof(RebuildPysics))]
		public readonly AssetRef<RMesh> TargetMesh;

		public override ColliderShape PysicsBuild() {
			return TargetMesh.Target is null
				? null
				: TargetMesh.Asset is null
				? null
				: TargetMesh.Asset.LoadedMesh is null ? null : (ColliderShape)new RConvexMeshShape(TargetMesh.Asset.LoadedMesh);
		}
	}
}
