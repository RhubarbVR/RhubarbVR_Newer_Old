using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using BepuPhysics.Collidables;
using BepuPhysics;
using RhuEngine.Linker.MeshAddons;
using BepuUtilities.Memory;
using System.Numerics;
using System;
using BepuUtilities;
using System.Runtime.InteropServices;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public sealed class ConvexMeshShape : PhysicsMeshShape<ConvexHull, PhysicsConvexHullAddon>
	{
		protected override void CleanUpShapeData() {
			GetShape?.Dispose(World.PhysicsSimulation.BufferPool);
		}

		protected override ConvexHull CreateEmpty(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			Span<Vector3> blankpoints = stackalloc Vector3[3];
			blankpoints[0] = Vector3.Zero;
			blankpoints[1] = Vector3.Zero;
			blankpoints[2] = Vector3.Zero;
			var hullShape = new ConvexHull(blankpoints, World.PhysicsSimulation.BufferPool, out _);
			inertia = !mass.HasValue ? default : hullShape.ComputeInertia(mass.Value);
			return hullShape;
		}

		protected override ConvexHull CreateShape(PhysicsConvexHullAddon addon, ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			speculativeMargin = Math.Min(speculativeMargin, _last?.BoundingBox.Extents.MinComponent?? 0);
			var size = Vector3f.One;
			ApplyGlobalScaleValues(ref size);
			Matrix3x3.CreateScale(size, out var linearTransform);
			ConvexHullHelper.CreateTransformedShallowCopy(in addon.convexHull, in linearTransform, World.PhysicsSimulation.BufferPool, out var hullShape);
			inertia = !mass.HasValue ? default : hullShape.ComputeInertia(mass.Value);
			return hullShape;
		}
	}
}
