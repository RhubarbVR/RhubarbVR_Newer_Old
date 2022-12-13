using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using System.Numerics;
using Assimp;
using RhuEngine.Components;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Dataflow;

namespace RhuEngine.Linker.MeshAddons
{
	public sealed class PhysicsConvexHullAddon : PhysicsAddon
	{
		public override string Name => "ConvexHull";

		private object _bufferLock;

		public ConvexHull convexHull;
		public Vector3f Center { get; private set; }
		

		private static bool IsValidPoints(Span<Vector3> points) {
			for (var i = 0; i < points.Length; i++) {
				if (((Vector3f)points[i]).IsAnyNanOrInfinity) {
					return false;
				}
			}
			return points.Length >= 3;
		}

		public override void Load(IMesh mesh) {
			_bufferLock ??= new object();
			var vertexCount = mesh.VertexCount;
			Span<Vector3> points = stackalloc Vector3[vertexCount];
			for (var i = 0; i < vertexCount; i++) {
				points[i] = (Vector3f)mesh.GetVertex(i);
			}
			var checkedPoints = IsValidPoints(points);
			lock (_bufferLock) {
				if (checkedPoints) {
					ConvexHullHelper.CreateShape(points, BufferPool, out var center2, out convexHull);
					Center = center2;
				}
				else {
					Span<Vector3> blankpoints = stackalloc Vector3[3];
					blankpoints[0] = Vector3.Zero;
					blankpoints[1] = Vector3.Zero;
					blankpoints[2] = Vector3.Zero;
					convexHull = new ConvexHull(blankpoints, BufferPool, out _);
					Center = Vector3f.Zero;
				}
			}
		}

		public override void Unload() {
			convexHull.Dispose(BufferPool);
			convexHull = default;
		}
	}
}
