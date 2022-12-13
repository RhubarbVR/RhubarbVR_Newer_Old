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

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	public class MeshShape : PhysicsMeshShape<Mesh, PhysicsMeshAddon>
	{
		protected override Mesh CreateEmpty(ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			return CreateShape(World.PhysicsSimulation.EmptyTriangles, ref speculativeMargin, mass, out inertia);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Mesh CreateShape(Buffer<Triangle> buffer, ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			speculativeMargin = Math.Min(speculativeMargin, _last?.BoundingBox.Extents.MinComponent ?? 0);
			var size = Vector3f.One;
			ApplyGlobalScaleValues(ref size);
			if(buffer.Length < 1) {
				buffer = World.PhysicsSimulation.EmptyTriangles;
			}
			var result = new Mesh(buffer, size, World.PhysicsSimulation.BufferPool);
			inertia = !mass.HasValue ? default : result.ComputeOpenInertia(mass.Value);
			return result;
		}

		protected override Mesh CreateShape(PhysicsMeshAddon addon, ref float speculativeMargin, float? mass, out BodyInertia inertia) {
			return CreateShape(addon.TrianglesBuffer, ref speculativeMargin, mass, out inertia);
		}

		protected override void CleanUpShapeData() {
			GetShape?.Tree.Dispose(World.PhysicsSimulation.BufferPool);
		}

		protected override void RemoveData() {
			GetAddon.OnDataUpdate -= UpdateShape;
		}

		protected override void AddedData() {
			GetAddon.OnDataUpdate += UpdateShape;
		}
	}
}
