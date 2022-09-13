using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class CapsuleShape : PhysicsObject
	{
		[OnChanged(nameof(RebuildPysics))]
		[Default(0.5)]
		public readonly Sync<double> Radius;
		[OnChanged(nameof(RebuildPysics))]
		[Default(1.0)]
		public readonly Sync<double> Height;
		[OnChanged(nameof(RebuildPysics))]
		public readonly Sync<Dir> Direction;

		public override ColliderShape PysicsBuild() {
			return Direction.Value switch {
				Dir.Y => new RCapsuleShape(Radius, Height),
				Dir.X => new RCapsuleShapeX(Radius, Height),
				Dir.Z => new RCapsuleShapeZ(Radius, Height),
				_ => null,
			};
		}
	}
}
