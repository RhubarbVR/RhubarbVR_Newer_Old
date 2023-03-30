using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RNumerics
{
	/// <summary>
	/// Generate a minimal box
	/// </summary>
	public sealed class TrivialBox3Generator : MeshGenerator
	{
		public Box3d Box = Box3d.UnitZeroCentered;
		public bool NoSharedVertices = false;

		public override MeshGenerator Generate() {
			vertices = new VectorArray3d(NoSharedVertices ? (4 * 6) : 8);
			uv = new VectorArray2f(vertices.Count);
			normals = new VectorArray3f(vertices.Count);
			triangles = new IndexArray3i(2 * 6);

			if (NoSharedVertices == false) {
				var square_uv = new Vector2f[4] { new Vector2f(0, 1), new Vector2f(1, 1),
					new Vector2f(1, 0), Vector2f.Zero };
				for (var i = 0; i < 8; ++i) {
					vertices[i] = Box.Corner(i);
					normals[i] = (Vector3f)(vertices[i] - Box.center[i]).Normalized;
					uv[i] = square_uv[i % 4];      // what to do for UVs in this case ?!?
				}
				var ti = 0;
				for (var fi = 0; fi < 6; ++fi) {
					triangles.Set(ti++,
						GIndices.BoxFaces[fi, 0], GIndices.BoxFaces[fi, 1], GIndices.BoxFaces[fi, 2], Clockwise);
					triangles.Set(ti++,
						GIndices.BoxFaces[fi, 0], GIndices.BoxFaces[fi, 2], GIndices.BoxFaces[fi, 3], Clockwise);
				}
			}
			else {
				var ti = 0;
				var vi = 0;
				var square_uv = new Vector2f[4] { new Vector2f(0, 1), new Vector2f(1, 1),
					new Vector2f(1, 0), Vector2f.Zero };
				for (var fi = 0; fi < 6; ++fi) {
					var v0 = vi++;
					vi += 3;
					var ni = GIndices.BoxFaceNormals[fi];
					var n = (Vector3f)(Math.Sign(ni) * Box.Axis(Math.Abs(ni) - 1));
					for (var j = 0; j < 4; ++j) {
						vertices[v0 + j] = Box.Corner(GIndices.BoxFaces[fi, j]);
						normals[v0 + j] = n;
						uv[v0 + j] = square_uv[j];
					}

					triangles.Set(ti++, v0, v0 + 1, v0 + 2, Clockwise);
					triangles.Set(ti++, v0, v0 + 2, v0 + 3, Clockwise);
				}
			}

			return this;
		}
	}









	/// <summary>
	/// Generate a mesh of a box that has "gridded" faces, ie grid of triangulated quads, 
	/// with EdgeVertices verts along each edge.
	/// [TODO] allow varying EdgeVertices in each dimension (tricky...)
	/// </summary>
	public class GridBox3Generator : MeshGenerator
	{
		public Box3d Box = Box3d.UnitZeroCentered;
		public int EdgeVertices = 8;
		public bool NoSharedVertices = false;

		public override MeshGenerator Generate() {
			var N = (EdgeVertices > 1) ? EdgeVertices : 2;
			var Nm2 = N - 2;
			var NT = N - 1;
			var N2 = N * N;
			vertices = new VectorArray3d(NoSharedVertices ? (N2 * 6) : (8 + (Nm2 * 12) + (Nm2 * Nm2 * 6)));
			uv = new VectorArray2f(vertices.Count);
			normals = new VectorArray3f(vertices.Count);
			triangles = new IndexArray3i(2 * NT * NT * 6);
			groups = new int[triangles.Count];

			var boxvertices = Box.ComputeVertices();

			var vi = 0;
			var ti = 0;
			if (NoSharedVertices) {
				for (var fi = 0; fi < 6; ++fi) {
					// get corner vertices
					var v00 = boxvertices[GIndices.BoxFaces[fi, 0]];
					var v01 = boxvertices[GIndices.BoxFaces[fi, 1]];
					var v11 = boxvertices[GIndices.BoxFaces[fi, 2]];
					var v10 = boxvertices[GIndices.BoxFaces[fi, 3]];
					var faceN = Math.Sign(GIndices.BoxFaceNormals[fi]) * (Vector3f)Box.Axis(Math.Abs(GIndices.BoxFaceNormals[fi]) - 1);

					// add vertex rows
					var start_vi = vi;
					for (var yi = 0; yi < N; ++yi) {
						var ty = (double)yi / (double)(N - 1);
						for (var xi = 0; xi < N; ++xi) {
							var tx = (double)xi / (double)(N - 1);
							normals[vi] = faceN;
							uv[vi] = new Vector2f(tx, ty);
							vertices[vi++] = Bilerp(v00, v01, v11, v10, tx, ty);
						}
					}

					// add faces
					for (var y0 = 0; y0 < NT; ++y0) {
						for (var x0 = 0; x0 < NT; ++x0) {
							var i00 = start_vi + (y0 * N) + x0;
							var i10 = start_vi + ((y0 + 1) * N) + x0;
							int i01 = i00 + 1, i11 = i10 + 1;

							groups[ti] = fi;
							triangles.Set(ti++, i00, i01, i11, Clockwise);
							groups[ti] = fi;
							triangles.Set(ti++, i00, i11, i10, Clockwise);
						}
					}
				}

			}
			else {
				// construct integer coordinates
				var intvertices = new Vector3i[boxvertices.Length];
				for (var k = 0; k < boxvertices.Length; ++k) {
					var v = boxvertices[k] - Box.center;
					intvertices[k] = new Vector3i(
						v.x < 0 ? 0 : N - 1,
						v.y < 0 ? 0 : N - 1,
						v.z < 0 ? 0 : N - 1);
				}
				var faceIndicesV = new int[N2];

				// add edge vertices and store in this map
				// todo: don't use a map (?)  how do we do that, though...
				//   - each index is in range [0,N). If we have (i,j,k), then for a given
				//     i, we have a finite number of j and k (< 2N?).
				//     make N array of 2N length, key on i, linear search for matching j/k?
				var edgeVerts = new Dictionary<Vector3i, int>();
				for (var fi = 0; fi < 6; ++fi) {
					// get corner vertices
					int c00 = GIndices.BoxFaces[fi, 0], c01 = GIndices.BoxFaces[fi, 1],
						c11 = GIndices.BoxFaces[fi, 2], c10 = GIndices.BoxFaces[fi, 3];
					var v00 = boxvertices[c00];
					var vi00 = intvertices[c00];
					var v01 = boxvertices[c01];
					var vi01 = intvertices[c01];
					var v11 = boxvertices[c11];
					var vi11 = intvertices[c11];
					var v10 = boxvertices[c10];
					var vi10 = intvertices[c10];

					void do_edge(Vector3d a, Vector3d b, Vector3i ai, Vector3i bi) {
						for (var i = 0; i < N; ++i) {
							var t = (double)i / (double)(N - 1);
							var vidx = Lerp(ai, bi, t);
							if (edgeVerts.ContainsKey(vidx) == false) {
								var v = Vector3d.Lerp(a, b, t);
								normals[vi] = (Vector3f)v.Normalized;
								uv[vi] = Vector2f.Zero;
								edgeVerts[vidx] = vi;
								vertices[vi++] = v;
							}
						}
					}
					do_edge(v00, v01, vi00, vi01);
					do_edge(v01, v11, vi01, vi11);
					do_edge(v11, v10, vi11, vi10);
					do_edge(v10, v00, vi10, vi00);
				}


				// now generate faces
				for (var fi = 0; fi < 6; ++fi) {
					// get corner vertices
					int c00 = GIndices.BoxFaces[fi, 0], c01 = GIndices.BoxFaces[fi, 1],
						c11 = GIndices.BoxFaces[fi, 2], c10 = GIndices.BoxFaces[fi, 3];
					var v00 = boxvertices[c00];
					var vi00 = intvertices[c00];
					var v01 = boxvertices[c01];
					var vi01 = intvertices[c01];
					var v11 = boxvertices[c11];
					var vi11 = intvertices[c11];
					var v10 = boxvertices[c10];
					var vi10 = intvertices[c10];
					var faceN = Math.Sign(GIndices.BoxFaceNormals[fi]) * (Vector3f)Box.Axis(Math.Abs(GIndices.BoxFaceNormals[fi]) - 1);

					// add vertex rows, using existing vertices if we have them in map
					for (var yi = 0; yi < N; ++yi) {
						var ty = (double)yi / (double)(N - 1);
						for (var xi = 0; xi < N; ++xi) {
							var tx = (double)xi / (double)(N - 1);
							var vidx = Bilerp(vi00, vi01, vi11, vi10, tx, ty);
							if (edgeVerts.TryGetValue(vidx, out var use_vi) == false) {
								var v = Bilerp(v00, v01, v11, v10, tx, ty);
								use_vi = vi++;
								normals[use_vi] = faceN;
								uv[use_vi] = new Vector2f(tx, ty);
								vertices[use_vi] = v;
							}
							faceIndicesV[(yi * N) + xi] = use_vi;
						}
					}

					// add faces
					for (var y0 = 0; y0 < NT; ++y0) {
						var y1 = y0 + 1;
						for (var x0 = 0; x0 < NT; ++x0) {
							var x1 = x0 + 1;
							var i00 = faceIndicesV[(y0 * N) + x0];
							var i01 = faceIndicesV[(y0 * N) + x1];
							var i11 = faceIndicesV[(y1 * N) + x1];
							var i10 = faceIndicesV[(y1 * N) + x0];

							groups[ti] = fi;
							triangles.Set(ti++, i00, i01, i11, Clockwise);
							groups[ti] = fi;
							triangles.Set(ti++, i00, i11, i10, Clockwise);
						}
					}
				}


			}

			return this;
		}



	}


}
