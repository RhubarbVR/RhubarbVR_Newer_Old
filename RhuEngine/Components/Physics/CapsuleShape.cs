using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using BepuPhysics.Collidables;
using BepuPhysics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class CapsuleShape : BasicPhysicsShape<Capsule>
	{
		[Default(0.5f)]
		[OnChanged(nameof(UpdateShape))]
		public readonly Sync<float> Radius;

		[Default(1.0f)]
		[OnChanged(nameof(UpdateShape))]
		public readonly Sync<float> Height;

		public override Capsule CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			var size = new Vector3f(Radius.Value, Height.Value, Radius.Value);
			ApplyGlobalScaleValues(ref size);
			speculativeMargin = MathF.Min(speculativeMargin, MathUtil.Max(size.x, size.y, size.z));
			var result = new Capsule((size.x + size.z) / 2, size.y);
			inertia = !mass.HasValue ? default : result.ComputeInertia(mass.Value);
			return result;
		}
	}
}
