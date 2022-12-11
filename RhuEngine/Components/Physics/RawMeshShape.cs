using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using BepuPhysics.Collidables;
using BepuPhysics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class MeshShape : PhysicsShape<Mesh>
	{
		[OnAssetLoaded(nameof(UpdateShape))]
		public readonly AssetRef<RMesh> TargetMesh;

		public override Mesh CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			inertia = default;
			return default;
		}

		public override void RemoveShape() {
			Simulation.Simulation.Shapes.RemoveAndDispose(ShapeIndex, Simulation.BufferPool);
			ShapeIndex = default;
		}
	}
}
