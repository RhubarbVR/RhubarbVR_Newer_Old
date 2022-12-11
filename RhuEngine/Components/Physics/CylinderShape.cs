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
	public sealed class CylinderShape : BasicPhysicsShape<Cylinder>
	{
		[Default(0.5f)]
		[OnChanged(nameof(UpdateShape))]
		public readonly Sync<float> Radius;
		[Default(1.0f)]
		[OnChanged(nameof(UpdateShape))]
		public readonly Sync<float> Height;

		public override Cylinder CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			var size = new Vector3f(Radius.Value, Height.Value, Radius.Value);
			ApplyGlobalScaleValues(ref size);
			speculativeMargin = MathF.Min(speculativeMargin, MathUtil.Max(size.x, size.y, size.z));
			var result = new BepuPhysics.Collidables.Cylinder((size.x + size.y) / 2, size.y);
			inertia = !mass.HasValue ? default : result.ComputeInertia(mass.Value);
			return result;
		}
	}
}
