using System;
using System.Collections.Generic;

namespace RNumerics
{
	public static class IndexUtil
	{
		// test if [a0,a1] and [b0,b1] are the same pair, ignoring order
		public static bool Same_pair_unordered(in int a0, in int a1, in int b0, in int b1) {
			return (a0 == b0) ?
				(a1 == b1) :
				(a0 == b1 && a1 == b0);
		}

		// find the vtx that is the same in both ev0 and ev1
		public static int Find_shared_edge_v(ref Index2i ev0, ref Index2i ev1) {
			return ev0.a == ev1.a ? ev0.a : ev0.a == ev1.b ? ev0.a : ev0.b == ev1.a ? ev0.b : ev0.b == ev1.b ? ev0.b : int.MinValue;
		}


		// find the vtx that is the same in both ev0 and ev1
		public static int Find_edge_other_v(in Index2i ev, in int v) {
			return ev.a == v ? ev.b : ev.b == v ? ev.a : int.MinValue;
		}


		// return index of a in tri_verts, or InvalidID if not found
		public static int Find_tri_index(in int a, in int[] tri_verts) {
			return tri_verts[0] == a ? 0 : tri_verts[1] == a ? 1 : tri_verts[2] == a ? 2 : int.MinValue;
		}
		public static int Find_tri_index(in int a, in Index3i tri_verts) {
			return tri_verts.a == a ? 0 : tri_verts.b == a ? 1 : tri_verts.c == a ? 2 : int.MinValue;
		}


		// return index of a in tri_verts, or InvalidID if not found
		public static int Find_edge_index_in_tri(in int a, in int b, in int[] tri_verts) {
			return Same_pair_unordered(a, b, tri_verts[0], tri_verts[1])
				? 0
				: Same_pair_unordered(a, b, tri_verts[1], tri_verts[2])
				? 1
				: Same_pair_unordered(a, b, tri_verts[2], tri_verts[0]) ? 2 : int.MinValue;
		}

		public static int Find_edge_index_in_tri(in int a, in int b, in Index3i tri_verts) {
			return Same_pair_unordered(a, b, tri_verts.a, tri_verts.b)
				? 0
				: Same_pair_unordered(a, b, tri_verts.b, tri_verts.c)
				? 1
				: Same_pair_unordered(a, b, tri_verts.c, tri_verts.a) ? 2 : int.MinValue;
		}

		// find sequence [a,b] in tri_verts (mod3) and return index of a, or InvalidID if not found
		public static int Find_tri_ordered_edge(in int a, in int b, in int[] tri_verts) {
			return tri_verts[0] == a && tri_verts[1] == b
				? 0
				: tri_verts[1] == a && tri_verts[2] == b ? 1 : tri_verts[2] == a && tri_verts[0] == b ? 2 : int.MinValue;
		}

		/// <summary>
		///  find sequence [a,b] in tri_verts (mod3) and return index of a, or InvalidID if not found
		/// </summary>
		public static int Find_tri_ordered_edge(in int a, in int b, in Index3i tri_verts) {
			return tri_verts.a == a && tri_verts.b == b
				? 0
				: tri_verts.b == a && tri_verts.c == b ? 1 : tri_verts.c == a && tri_verts.a == b ? 2 : int.MinValue;
		}

		/// <summary>
		/// assuming a is in tri-verts, returns other two vertices, in correct order (or Index2i.Max if not found)
		/// </summary>
		public static Index2i Find_tri_other_verts(in int a, ref Index3i tri_verts) {
			if (tri_verts.a == a) {
				return new Index2i(tri_verts.b, tri_verts.c);
			}
			else if (tri_verts.b == a) {
				return new Index2i(tri_verts.c, tri_verts.a);
			}
			else if (tri_verts.c == a) {
				return new Index2i(tri_verts.a, tri_verts.b);
			}

			return Index2i.Max;
		}


		// Set [a,b] to order found in tri_verts (mod3). return true if we swapped.
		// Assumes that a and b are in tri_verts, if not the result is garbage!
		public static bool Orient_tri_edge(ref int a, ref int b, ref Index3i tri_verts) {
			if (a == tri_verts.a) {
				if (tri_verts.c == b) {
					(b, a) = (a, b);
					return true;
				}
			}
			else if (a == tri_verts.b) {
				if (tri_verts.a == b) {
					(b, a) = (a, b);
					return true;
				}
			}
			else if (a == tri_verts.c) {
				if (tri_verts.b == b) {
					(b, a) = (a, b);
					return true;
				}
			}
			return false;
		}
		public static bool Orient_tri_edge(ref int a, ref int b, Index3i tri_verts) {
			return Orient_tri_edge(ref a, ref b, ref tri_verts);
		}

		public static bool Is_ordered(in int a, in int b, ref Index3i tri_verts) {
			return (tri_verts.a == a && tri_verts.b == b) ||
				   (tri_verts.b == a && tri_verts.c == b) ||
				   (tri_verts.c == a && tri_verts.a == b);
		}


		public static bool Is_same_triangle(in int a, in int b, in int c, ref Index3i tri) {
			return tri.a == a
				? Same_pair_unordered(tri.b, tri.c, b, c)
				: tri.b == a ? Same_pair_unordered(tri.a, tri.c, b, c) : tri.c == a && Same_pair_unordered(tri.a, tri.b, b, c);
		}


		public static void Cycle_indices_minfirst(ref Index3i tri) {
			if (tri.b < tri.a && tri.b < tri.c) {
				int a = tri.a, b = tri.b, c = tri.c;
				tri.a = b;
				tri.b = c;
				tri.c = a;
			}
			else if (tri.c < tri.a && tri.c < tri.b) {
				int a = tri.a, b = tri.b, c = tri.c;
				tri.a = c;
				tri.b = a;
				tri.c = b;
			}
		}


		public static void Sort_indices(ref Index3i tri) {
			// possibly this can be re-ordered to have fewer tests? ...
			if (tri.a < tri.b && tri.a < tri.c) {
				if (tri.b > tri.c) {
					(tri.c, tri.b) = (tri.b, tri.c);
				}
			}
			else if (tri.b < tri.a && tri.b < tri.c) {
				if (tri.a < tri.c) {
					(tri.a, tri.b) = (tri.b, tri.a);
				}
				else {
					int a = tri.a, b = tri.b, c = tri.c;
					tri.a = b;
					tri.b = c;
					tri.c = a;
				}
			}
			else if (tri.c < tri.a && tri.c < tri.b) {
				if (tri.b < tri.a) {
					(tri.a, tri.c) = (tri.c, tri.a);
				}
				else {
					int a = tri.a, b = tri.b, c = tri.c;
					tri.a = c;
					tri.b = a;
					tri.c = b;
				}
			}
		}



		public static Vector3i ToGrid3Index(in int idx, in int nx, in int ny) {
			var x = idx % nx;
			var y = idx / nx % ny;
			var z = idx / (nx * ny);
			return new Vector3i(x, y, z);
		}

		public static int ToGrid3Linear(in int i, in int j, in int k, in int nx, in int ny) {
			return i + (nx * (j + (ny * k)));
		}
		public static int ToGrid3Linear(in Vector3i ijk, in int nx, in int ny) {
			return ijk.x + (nx * (ijk.y + (ny * ijk.z)));
		}



		/// <summary>
		/// Filter out invalid entries in indices[] list. Will return indices itself if 
		/// none invalid, and bForceCopy == false
		/// </summary>
		public static int[] FilterValid(in int[] indices, in Func<int, bool> FilterF, in bool bForceCopy = false) {
			var nValid = 0;
			for (var i = 0; i < indices.Length; ++i) {
				if (FilterF(indices[i])) {
					++nValid;
				}
			}
			if (nValid == indices.Length && bForceCopy == false) {
				return indices;
			}

			var valid = new int[nValid];
			var vi = 0;
			for (var i = 0; i < indices.Length; ++i) {
				if (FilterF(indices[i])) {
					valid[vi++] = indices[i];
				}
			}
			return valid;
		}



		/// <summary>
		/// return trune if CheckF returns true for all members of indices list
		/// </summary>
		public static bool IndicesCheck(in int[] indices, in Func<int, bool> CheckF) {
			for (var i = 0; i < indices.Length; ++i) {
				if (CheckF(indices[i]) == false) {
					return false;
				}
			}
			return true;
		}



		/// <summary>
		/// Apply map to indices
		/// </summary>
		public static void Apply(in List<int> indices, in IIndexMap map) {
			var N = indices.Count;
			for (var i = 0; i < N; ++i) {
				indices[i] = map[indices[i]];
			}
		}

		public static void Apply(in int[] indices, in IIndexMap map) {
			var N = indices.Length;
			for (var i = 0; i < N; ++i) {
				indices[i] = map[indices[i]];
			}
		}

		public static void Apply(in int[] indices, in IList<int> map) {
			var N = indices.Length;
			for (var i = 0; i < N; ++i) {
				indices[i] = map[indices[i]];
			}
		}
	}





	public static class GIndices
	{
		// integer indices offsets in x/y directions
		public static readonly Vector2i[] GridOffsets4 = new Vector2i[] {
			new Vector2i( -1, 0), new Vector2i( 1, 0),
			new Vector2i( 0, -1), new Vector2i( 0, 1)
		};

		// integer indices offsets in x/y directions and diagonals
		public static readonly Vector2i[] GridOffsets8 = new Vector2i[] {
			new Vector2i( -1, 0), new Vector2i( 1, 0),
			new Vector2i( 0, -1), new Vector2i( 0, 1),
			new Vector2i( -1, 1), new Vector2i( 1, 1),
			new Vector2i( -1, -1), new Vector2i( 1, -1)
		};



		// Corner vertices of box faces  -  see Box.Corner for points associated w/ indexing
		// Note that 
		public static readonly int[,] BoxFaces = new int[6, 4] {
			{ 1, 0, 3, 2 },     // back, -z
            { 4, 5, 6, 7 },     // front, +z
            { 0, 4, 7, 3 },     // left, -x
            { 5, 1, 2, 6 },     // right, +x,
            { 0, 1, 5, 4 },     // bottom, -y
            { 7, 6, 2, 3 }      // top, +y
        };

		// Box Face normal. Use Sign(BoxFaceNormals[i]) * Box.Axis( Abs(BoxFaceNormals[i])-1 )
		//  (+1 is so we can have a sign on X)
		public static readonly int[] BoxFaceNormals = new int[6] { -3, 3, -1, 1, -2, 2 };


		// integer indices offsets in x/y/z directions, corresponds w/ BoxFaces directions
		public static readonly Vector3i[] GridOffsets6 = new Vector3i[] {
			new Vector3i( 0, 0,-1), new Vector3i( 0, 0, 1),
			new Vector3i(-1, 0, 0), new Vector3i( 1, 0, 0),
			new Vector3i( 0,-1, 0), new Vector3i( 0, 1, 0)
		};

		// integer indices offsets in x/y/z directions and diagonals
		public static readonly Vector3i[] GridOffsets26 = new Vector3i[] {
			// face-nbrs
			new Vector3i( 0, 0,-1), new Vector3i( 0, 0, 1),
			new Vector3i(-1, 0, 0), new Vector3i( 1, 0, 0),
			new Vector3i( 0,-1, 0), new Vector3i( 0, 1, 0),
			// edge-nbrs (+y, 0, -y)
			new Vector3i(1, 1, 0), new Vector3i(-1, 1, 0),
			new Vector3i(0, 1, 1), new Vector3i( 0, 1,-1),
			new Vector3i(1, 0, 1), new Vector3i(-1, 0, 1),
			new Vector3i(1, 0,-1), new Vector3i(-1, 0,-1),
			new Vector3i(1, -1, 0), new Vector3i(-1,-1, 0),
			new Vector3i(0, -1, 1), new Vector3i( 0,-1,-1),
			// corner-nbrs (+y,-y)
			new Vector3i(1, 1, 1), new Vector3i(-1, 1, 1),
			new Vector3i(1, 1,-1), new Vector3i(-1, 1,-1),
			new Vector3i(1,-1, 1), new Vector3i(-1,-1, 1),
			new Vector3i(1,-1,-1), new Vector3i(-1,-1,-1)
		};



		public static IEnumerable<Vector3i> Grid3Indices(int nx, int ny, int nz) {
			for (var z = 0; z < nz; ++z) {
				for (var y = 0; y < ny; ++y) {
					for (var x = 0; x < nx; ++x) {
						yield return new Vector3i(x, y, z);
					}
				}
			}
		}


		public static IEnumerable<Vector3i> Grid3IndicesYZ(int ny, int nz) {
			for (var z = 0; z < nz; ++z) {
				for (var y = 0; y < ny; ++y) {
					yield return new Vector3i(0, y, z);
				}
			}
		}

		public static IEnumerable<Vector3i> Grid3IndicesXZ(int nx, int nz) {
			for (var z = 0; z < nz; ++z) {
				for (var x = 0; x < nx; ++x) {
					yield return new Vector3i(x, 0, z);
				}
			}
		}

		public static IEnumerable<Vector3i> Grid3IndicesXY(int nx, int ny) {
			for (var y = 0; y < ny; ++y) {
				for (var x = 0; x < nx; ++x) {
					yield return new Vector3i(x, y, 0);
				}
			}
		}


	}



}
