using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public static class CurveUtils
	{

		public static Vector3d GetTangent(in List<Vector3d> vertices, in int i, in bool bLoop = false) {
			if (bLoop) {
				var NV = vertices.Count;
				return i == 0 ? (vertices[1] - vertices[NV - 1]).Normalized : (vertices[(i + 1) % NV] - vertices[i - 1]).Normalized;
			}
			else {
				return i == 0
					? (vertices[1] - vertices[0]).Normalized
					: i == vertices.Count - 1
					? (vertices[vertices.Count - 1] - vertices[vertices.Count - 2]).Normalized
					: (vertices[i + 1] - vertices[i - 1]).Normalized;
			}
		}


		public static double ArcLength(in List<Vector3d> vertices, in bool bLoop = false) {
			double sum = 0;
			var NV = vertices.Count;
			for (var i = 1; i < NV; ++i) {
				sum += vertices[i].Distance(vertices[i - 1]);
			}

			if (bLoop) {
				sum += vertices[NV - 1].Distance(vertices[0]);
			}

			return sum;
		}
		public static double ArcLength(in Vector3d[] vertices, in bool bLoop = false) {
			double sum = 0;
			for (var i = 1; i < vertices.Length; ++i) {
				sum += vertices[i].Distance(vertices[i - 1]);
			}

			if (bLoop) {
				sum += vertices[vertices.Length - 1].Distance(vertices[0]);
			}

			return sum;
		}
		public static double ArcLength(in IEnumerable<Vector3d> vertices) {
			double sum = 0;
			Vector3d prev = Vector3f.Zero;
			var i = 0;
			foreach (var v in vertices) {
				if (i++ > 0) {
					sum += (v - prev).Length;
				}

				prev = v;
			}
			return sum;
		}



		public static int FindNearestIndex(in ISampledCurve3d c, in Vector3d v) {
			var iNearest = -1;
			var dNear = double.MaxValue;
			var N = c.VertexCount;
			for (var i = 0; i < N; ++i) {
				var dSqr = (c.GetVertex(i) - v).LengthSquared;
				if (dSqr < dNear) {
					dNear = dSqr;
					iNearest = i;
				}
			}
			return iNearest;
		}



		public static bool FindClosestRayIntersection(in ISampledCurve3d c, in double segRadius, in Ray3d ray, out double minRayT) {
			minRayT = double.MaxValue;
			var nNearSegment = -1;

			var nSegs = c.SegmentCount;
			for (var i = 0; i < nSegs; ++i) {
				var seg = c.GetSegment(i);

				// raycast to line bounding-sphere first (is this going ot be faster??)
				var bHitBoundSphere = RayIntersection.SphereSigned(ray.origin, ray.direction,
					seg.center, seg.extent + segRadius, out var fSphereHitT);
				if (bHitBoundSphere == false) {
					continue;
				}

				var dSqr = DistRay3Segment3.SquaredDistance(ray,seg, out var rayt, out var segt);
				if (dSqr < segRadius * segRadius) {
					if (rayt < minRayT) {
						minRayT = rayt;
						nNearSegment = i;
					}
				}
			}
			return nNearSegment >= 0;
		}





		/// <summary>
		/// smooth set of vertices in-place (will not produce a symmetric result, but does not require extra buffer)
		/// </summary>
		public static void InPlaceSmooth(in IList<Vector3d> vertices, in double alpha, in int nIterations, in bool bClosed) {
			InPlaceSmooth(vertices, 0, vertices.Count, alpha, nIterations, bClosed);
		}
		/// <summary>
		/// smooth set of vertices in-place (will not produce a symmetric result, but does not require extra buffer)
		/// </summary>
		public static void InPlaceSmooth(in IList<Vector3d> vertices, in int iStart, in int iEnd, in double alpha, in int nIterations, in bool bClosed) {
			var N = vertices.Count;
			if (bClosed) {
				for (var iter = 0; iter < nIterations; ++iter) {
					for (var ii = iStart; ii < iEnd; ++ii) {
						var i = ii % N;
						var iPrev = (ii == 0) ? N - 1 : ii - 1;
						var iNext = (ii + 1) % N;
						Vector3d prev = vertices[iPrev], next = vertices[iNext];
						var c = (prev + next) * 0.5f;
						vertices[i] = ((1 - alpha) * vertices[i]) + (alpha * c);
					}
				}
			}
			else {
				for (var iter = 0; iter < nIterations; ++iter) {
					for (var i = iStart; i <= iEnd; ++i) {
						if (i == 0 || i >= N - 1) {
							continue;
						}

						Vector3d prev = vertices[i - 1], next = vertices[i + 1];
						var c = (prev + next) * 0.5f;
						vertices[i] = ((1 - alpha) * vertices[i]) + (alpha * c);
					}
				}
			}
		}



		/// <summary>
		/// smooth set of vertices using extra buffer
		/// </summary>
		public static void IterativeSmooth(in IList<Vector3d> vertices, in double alpha, in int nIterations, in bool bClosed) {
			IterativeSmooth(vertices, 0, vertices.Count, alpha, nIterations, bClosed);
		}
		/// <summary>
		/// smooth set of vertices using extra buffer
		/// </summary>
		public static void IterativeSmooth(in IList<Vector3d> vertices, in int iStart, in int iEnd, in double alpha, in int nIterations, in bool bClosed, Vector3d[] buffer = null) {
			var N = vertices.Count;
			if (buffer == null || buffer.Length < N) {
				buffer = new Vector3d[N];
			}

			if (bClosed) {
				for (var iter = 0; iter < nIterations; ++iter) {
					for (var ii = iStart; ii < iEnd; ++ii) {
						var i = ii % N;
						var iPrev = (ii == 0) ? N - 1 : ii - 1;
						var iNext = (ii + 1) % N;
						Vector3d prev = vertices[iPrev], next = vertices[iNext];
						var c = (prev + next) * 0.5f;
						buffer[i] = ((1 - alpha) * vertices[i]) + (alpha * c);
					}
					for (var ii = iStart; ii < iEnd; ++ii) {
						var i = ii % N;
						vertices[i] = buffer[i];
					}
				}
			}
			else {
				for (var iter = 0; iter < nIterations; ++iter) {
					for (var i = iStart; i <= iEnd; ++i) {
						if (i == 0 || i >= N - 1) {
							continue;
						}

						Vector3d prev = vertices[i - 1], next = vertices[i + 1];
						var c = (prev + next) * 0.5f;
						buffer[i] = ((1 - alpha) * vertices[i]) + (alpha * c);
					}
					for (var ii = iStart; ii < iEnd; ++ii) {
						var i = ii % N;
						vertices[i] = buffer[i];
					}
				}
			}
		}




	}





	/// <summary>
	/// Simple sampled-curve wrapper type
	/// </summary>
	public sealed class IWrappedCurve3d : ISampledCurve3d
	{
		public IList<Vector3d> VertexList;
		public bool Closed { get; set; }

		public int VertexCount => (VertexList == null) ? 0 : VertexList.Count;
		public int SegmentCount => Closed ? VertexCount : VertexCount - 1;

		public Vector3d GetVertex(in int i) { return VertexList[i]; }
		public Segment3d GetSegment(in int iSegment) {
			return Closed ? new Segment3d(VertexList[iSegment], VertexList[(iSegment + 1) % VertexList.Count])
				: new Segment3d(VertexList[iSegment], VertexList[iSegment + 1]);
		}


		public IEnumerable<Vector3d> Vertices => VertexList;
	}

}
