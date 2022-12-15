using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace RhuEngine.Linker.MeshAddons
{
	public sealed class PhysicsMeshAddon : PhysicsAddon
	{
		public override string Name => "PhysicsMesh";

		public Buffer<Triangle> TrianglesBuffer;

		public event Action OnDataUpdate;

		public override void Load(IMesh mesh) {
			if (BufferPool == default) {
				BufferPool.Take(mesh.TriangleCount, out TrianglesBuffer);
			}
			if (TrianglesBuffer.Length != mesh.TriangleCount) {
				BufferPool.Resize(ref TrianglesBuffer, mesh.TriangleCount, 0);
			}
			for (var i = 0; i < mesh.TriangleCount; ++i) {
				var tri = mesh.GetTriangle(i);
				TrianglesBuffer[i] = new Triangle(mesh.GetVertex(tri.b), mesh.GetVertex(tri.a), mesh.GetVertex(tri.c));
			}
			OnDataUpdate?.Invoke();
		}

		public override void Unload() {
			BufferPool.Resize(ref TrianglesBuffer, 0, 0);
			OnDataUpdate?.Invoke();
		}

		public override void Dispose() {
			if (BufferPool != default) {
				BufferPool.Return(ref TrianglesBuffer);
				TrianglesBuffer = default;
			}
			base.Dispose();
		}

	}
}
