using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace RNumerics
{
	public class IcosphereGenerator : MeshGenerator
	{
		public int iterations = 1;
		public float radius = 1f;
		public override MeshGenerator Generate() {
			int nn = iterations * 4;
			int vertexNum = nn * nn / 16 * 24;
			Vector3d[] verts = new Vector3d[vertexNum];
			int[] tris = new int[vertexNum];
			Vector2d[] uvs = new Vector2d[vertexNum];

			Quaternion[] init_vectors = new Quaternion[24];
			// 0
			init_vectors[0] = new Quaternion(0, 1, 0, 0);   //the triangle vertical to (1,1,1)
			init_vectors[1] = new Quaternion(0, 0, 1, 0);
			init_vectors[2] = new Quaternion(1, 0, 0, 0);
			// 1
			init_vectors[3] = new Quaternion(0, -1, 0, 0);  //to (1,-1,1)
			init_vectors[4] = new Quaternion(1, 0, 0, 0);
			init_vectors[5] = new Quaternion(0, 0, 1, 0);
			// 2
			init_vectors[6] = new Quaternion(0, 1, 0, 0);   //to (-1,1,1)
			init_vectors[7] = new Quaternion(-1, 0, 0, 0);
			init_vectors[8] = new Quaternion(0, 0, 1, 0);
			// 3
			init_vectors[9] = new Quaternion(0, -1, 0, 0);  //to (-1,-1,1)
			init_vectors[10] = new Quaternion(0, 0, 1, 0);
			init_vectors[11] = new Quaternion(-1, 0, 0, 0);
			// 4
			init_vectors[12] = new Quaternion(0, 1, 0, 0);  //to (1,1,-1)
			init_vectors[13] = new Quaternion(1, 0, 0, 0);
			init_vectors[14] = new Quaternion(0, 0, -1, 0);
			// 5
			init_vectors[15] = new Quaternion(0, 1, 0, 0); //to (-1,1,-1)
			init_vectors[16] = new Quaternion(0, 0, -1, 0);
			init_vectors[17] = new Quaternion(-1, 0, 0, 0);
			// 6
			init_vectors[18] = new Quaternion(0, -1, 0, 0); //to (-1,-1,-1)
			init_vectors[19] = new Quaternion(-1, 0, 0, 0);
			init_vectors[20] = new Quaternion(0, 0, -1, 0);
			// 7
			init_vectors[21] = new Quaternion(0, -1, 0, 0);  //to (1,-1,-1)
			init_vectors[22] = new Quaternion(0, 0, -1, 0);
			init_vectors[23] = new Quaternion(1, 0, 0, 0);

			int j = 0;  //index on vectors[]

			for (int i = 0; i < 24; i += 3) {
				/*
				 *                   c _________d
				 *    ^ /\           /\        /
				 *   / /  \         /  \      /
				 *  p /    \       /    \    /
				 *   /      \     /      \  /
				 *  /________\   /________\/
				 *     q->       a         b
				 */
				for (int p = 0; p < iterations; p++) {
					//edge index 1
					Quaternion edge_p1 = Quaternion.Lerp(init_vectors[i], init_vectors[i + 2], (float)p / iterations);
					Quaternion edge_p2 = Quaternion.Lerp(init_vectors[i + 1], init_vectors[i + 2], (float)p / iterations);
					Quaternion edge_p3 = Quaternion.Lerp(init_vectors[i], init_vectors[i + 2], (float)(p + 1) / iterations);
					Quaternion edge_p4 = Quaternion.Lerp(init_vectors[i + 1], init_vectors[i + 2], (float)(p + 1) / iterations);

					for (int q = 0; q < (iterations - p); q++) {
						//edge index 2
						Quaternion a = Quaternion.Lerp(edge_p1, edge_p2, (float)q / (iterations - p));
						Quaternion b = Quaternion.Lerp(edge_p1, edge_p2, (float)(q + 1) / (iterations - p));
						Quaternion c, d;
						if (edge_p3 == edge_p4) {
							c = edge_p3;
							d = edge_p3;
						}
						else {
							c = Quaternion.Lerp(edge_p3, edge_p4, (float)q / (iterations - p - 1));
							d = Quaternion.Lerp(edge_p3, edge_p4, (float)(q + 1) / (iterations - p - 1));
						}

						tris[j] = j;
						verts[j++] = new Vector3d(a.X, a.Y, a.Z);
						tris[j] = j;
						verts[j++] = new Vector3d(b.X, b.Y, b.Z);
						tris[j] = j;
						verts[j++] = new Vector3d(c.X, c.Y, c.Z);
						if (q < iterations - p - 1) {
							tris[j] = j;
							verts[j++] = new Vector3d(c.X, c.Y, c.Z);
							tris[j] = j;
							verts[j++] = new Vector3d(b.X, b.Y, b.Z);
							tris[j] = j;
							verts[j++] = new Vector3d(d.X, d.Y, d.Z);
						}
					}
				}
			}

			CreateUV(iterations, verts, uvs);
			for (int i = 0; i < vertexNum; i++) {
				verts[i] *= radius;
			}

			vertices = new VectorArray3d(verts.Length);
			for (int i = 0; i < verts.Length; i++) {
				vertices[i] = verts[i];
			}

			triangles = new IndexArray3i(tris.Length);
			for (int i = 0; i < tris.Length; i += 3) {
				triangles[i] = new Index3i(
					tris[i],
					tris[i + 1],
					tris[i + 2]
				);
			}

			uv = new VectorArray2f(uvs.Length);
			for (int i = 0; i < uvs.Length; i++) {
				uv[i] = new Vector2f(
					uvs[i]
				);
			}

			// Wonky? PBR shader goes pitch-black with this
			normals = new VectorArray3f(vertices.Count);
			for (int i = 0; i < vertices.Count; i++) {
				normals[i] = new Vector3f(Estimate_normal(
					triangles[i].a,
					triangles[i].b,
					triangles[i].c
					)
				);
			}

			// An alternative for calcuating the normals, credit to jeana
/*			for (int i = 0; i < vertices.Count; i++) {
				normals[i] = new Vector3f(
					new Vector3d (vertices[triangles[i].b] - vertices[triangles[i].a]).Cross
					(vertices[triangles[i].c] - vertices[triangles[i].a])
				);
			}*/

			return this;
		}
		private static void CreateUV(int iterations, Vector3d[] vertices, Vector2d[] uv) {
			int tri = iterations * iterations;        // devided triangle count (1,4,9...)
			int uvLimit = tri * 6;  // range of wrap UV.x 

			for (int i = 0; i < vertices.Length; i++) {
				Vector3d v = vertices[i];

				Vector2d textureCoordinates;
				if ((v.x == 0f) && (i < uvLimit)) {
					textureCoordinates.x = 1f;
				}
				else {
					textureCoordinates.x = (float) (Math.Atan2(v.x, v.z) / -2f * Math.PI);
				}

				if (textureCoordinates.x < 0f) {
					textureCoordinates.x += 1f;
				}

				textureCoordinates.y = (float) (Math.Asin(v.y) / Math.PI + 0.5f);
				uv[i] = textureCoordinates;
			}

			int tt = tri * 3;
			uv[0 * tt + 0].x = 0.875f;
			uv[1 * tt + 0].x = 0.875f;
			uv[2 * tt + 0].x = 0.125f;
			uv[3 * tt + 0].x = 0.125f;
			uv[4 * tt + 0].x = 0.625f;
			uv[5 * tt + 0].x = 0.375f;
			uv[6 * tt + 0].x = 0.375f;
			uv[7 * tt + 0].x = 0.625f;

		}
	}
}
