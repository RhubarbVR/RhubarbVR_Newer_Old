using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using BepuPhysics.Collidables;
using BepuPhysics;
using System.Runtime.InteropServices;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class BoxShape : BasicPhysicsShape<Box>
	{
		[OnChanged(nameof(UpdateShape))]
		public readonly Sync<Vector3f> Size;

		public override Box CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			var size = Size.Value;
			ApplyGlobalScaleValues(ref size);
			speculativeMargin = MathF.Min(speculativeMargin, MathUtil.Max(size.x, size.y, size.z));
			var result = new Box(size.x, size.y, size.z);
			inertia = !mass.HasValue ? default : result.ComputeInertia(mass.Value);
			return result;
		}

		protected override void OnAttach() {
			base.OnAttach();
			Size.Value = Vector3f.One;
		}

	}
}
