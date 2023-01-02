using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using BepuPhysics.Trees;
using System.Numerics;
using BepuUtilities;

namespace RhuEngine.Linker.MeshAddons
{
	public sealed class PhysicsMeshAddon : PhysicsAddon
	{
		public override string Name => "PhysicsMesh";

		public Buffer<Triangle> TrianglesBuffer;

		public Tree Tree;

		public event Action OnDataUpdate;

		public override void Load(IMesh mesh) {
			if (!TrianglesBuffer.Allocated) {
				BufferPool.Take(mesh.TriangleCount, out TrianglesBuffer);
			}
			if (TrianglesBuffer.Length != mesh.TriangleCount) {
				BufferPool.Resize(ref TrianglesBuffer, mesh.TriangleCount, 0);
			}
			for (var i = 0; i < mesh.TriangleCount; ++i) {
				var tri = mesh.GetTriangle(i);
				TrianglesBuffer[i] = new Triangle(mesh.GetVertex(tri.b), mesh.GetVertex(tri.a), mesh.GetVertex(tri.c));
			}
			Tree = new Tree(BufferPool, TrianglesBuffer.Length);
			BufferPool.Take(TrianglesBuffer.Length, out Buffer<BoundingBox> buffer);
			for (var i = 0; i < TrianglesBuffer.Length; i++) {
				ref var reference = ref TrianglesBuffer[i];
				ref var reference2 = ref buffer[i];
				reference2.Min = Vector3.Min(reference.A, Vector3.Min(reference.B, reference.C));
				reference2.Max = Vector3.Max(reference.A, Vector3.Max(reference.B, reference.C));
			}
			Tree.SweepBuild(BufferPool, buffer);
			BufferPool.Return(ref buffer);

			OnDataUpdate?.Invoke();
		}

		public override void Unload() {
			BufferPool.Resize(ref TrianglesBuffer, 1, 1);
			Tree.Dispose(World.PhysicsSimulation.BufferPool);
			OnDataUpdate?.Invoke();
		}

		public override void Dispose() {
			if (TrianglesBuffer.Allocated) {
				BufferPool.Return(ref TrianglesBuffer);
			}
			Tree.Dispose(World.PhysicsSimulation.BufferPool);
			GC.SuppressFinalize(this);
		}

	}
}
