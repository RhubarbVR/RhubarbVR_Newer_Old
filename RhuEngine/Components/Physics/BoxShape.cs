using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public class BoxShape : PhysicsObject
	{
		[OnChanged(nameof(RebuildPysics))]
		public readonly Sync<Vector3d> boxHalfExtent;

		public override void OnAttach() {
			base.OnAttach();
			boxHalfExtent.Value = new Vector3d(0.5f);
		}

		public override ColliderShape PysicsBuild() {
			return new RBoxShape(boxHalfExtent.Value.x, boxHalfExtent.Value.y, boxHalfExtent.Value.z);
		}
	}
}
