using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public class SphereShape : PhysicsObject
	{
		[OnChanged(nameof(RebuildPysics))]
		[Default(0.5f)]
		public readonly Sync<double> Radus;
		public override ColliderShape PysicsBuild() {
			return new RSphereShape(Radus);
		}
	}
}
