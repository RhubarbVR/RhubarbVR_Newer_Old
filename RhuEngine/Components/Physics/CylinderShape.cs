using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public class CylinderShape : PhysicsObject
	{
		[OnChanged(nameof(RebuildPysics))]
		public readonly Sync<Vector3d> boxHalfExtent;
		[OnChanged(nameof(RebuildPysics))]
		public readonly Sync<Dir> Direction;
		protected override void OnAttach() {
			base.OnAttach();
			boxHalfExtent.Value = new Vector3d(0.5f);
		}
		public override ColliderShape PysicsBuild() {
			return Direction.Value switch {
				Dir.Y => new RCylinderShape(boxHalfExtent.Value.x, boxHalfExtent.Value.y, boxHalfExtent.Value.z),
				Dir.X => new RCylinderXShape(boxHalfExtent.Value.x, boxHalfExtent.Value.y, boxHalfExtent.Value.z),
				Dir.Z => new RCylinderZShape(boxHalfExtent.Value.x, boxHalfExtent.Value.y, boxHalfExtent.Value.z),
				_ => null,
			};
		}
	}
}
