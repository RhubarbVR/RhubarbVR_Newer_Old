using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using BepuPhysics.Collidables;
using BepuPhysics;
using RhuEngine.Linker.MeshAddons;
using BepuUtilities.Memory;
using System.Runtime.CompilerServices;
using System;
using BepuPhysics.Trees;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public class MeshShape : PhysicsMeshShape<Mesh, PhysicsMeshAddon>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override Mesh CreateEmpty(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			return CreateShape(World.PhysicsSimulation.EmptyTriangles, World.PhysicsSimulation.EmptyTree, ref speculativeMargin, mass, out inertia);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Mesh CreateShape(Buffer<Triangle> buffer, Tree tree, ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			speculativeMargin = Math.Min(speculativeMargin, _last?.BoundingBox.Extents.MinComponent ?? 0);
			var size = Vector3f.One;
			ApplyGlobalScaleValues(ref size);
			if (buffer.Length < 1) {
				buffer = World.PhysicsSimulation.EmptyTriangles;
			}
			var result = new Mesh {
				Tree = tree,
				Triangles = buffer,
				Scale = size
			};
			inertia = !mass.HasValue ? default : result.ComputeOpenInertia(mass.Value);
			return result;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override Mesh CreateShape(PhysicsMeshAddon addon, ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			return CreateShape(addon.TrianglesBuffer, addon.Tree, ref speculativeMargin, mass, out inertia);
		}

		protected override void CleanUpShapeData() {
		}

		protected override void RemoveData() {
			GetAddon.OnDataUpdate -= UpdateShape;
		}

		protected override void AddedData() {
			GetAddon.OnDataUpdate += UpdateShape;
		}
	}
}
