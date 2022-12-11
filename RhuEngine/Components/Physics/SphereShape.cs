using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using BepuPhysics.Collidables;
using Assimp;
using BepuPhysics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class SphereShape : BasicPhysicsShape<Sphere>
	{
		[Default(0.5f)]
		[OnChanged(nameof(UpdateShape))]
		public readonly Sync<float> Radius;

		public override Sphere CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			var size = new Vector3f(Radius.Value, Radius.Value, Radius.Value);
			ApplyGlobalScaleValues(ref size);
			speculativeMargin = MathF.Min(speculativeMargin, MathUtil.Max(size.x, size.y, size.z));
			var result = new BepuPhysics.Collidables.Sphere((size.x + size.y + size.y) / 3);
			inertia = !mass.HasValue ? default : result.ComputeInertia(mass.Value);
			return result;
		}
	}
}
