using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	public enum Dir {
		Y, X, Z
	}
	[Category(new string[] { "Physics" })]
	public class ConeShape : PhysicsObject
	{
		[OnChanged(nameof(RebuildPysics))]
		[Default(0.5)]
		public readonly Sync<double> Radius;
		[OnChanged(nameof(RebuildPysics))]
		[Default(1)]
		public readonly Sync<double> Height;
		[OnChanged(nameof(RebuildPysics))]
		public readonly Sync<Dir> Direction;
		public override ColliderShape PysicsBuild() {
			return Direction.Value switch {
				Dir.Y => new RConeShape(Radius, Height),
				Dir.X => new RConeXShape(Radius, Height),
				Dir.Z => new RConeZShape(Radius, Height),
				_ => null,
			};
		}
	}
}
