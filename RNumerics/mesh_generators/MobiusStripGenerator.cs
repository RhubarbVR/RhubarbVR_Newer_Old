using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace RNumerics
{
	// https://github.com/JPBotelho/Mobius-Strip
	public class MobiusStripGenerator : MeshGenerator
	{
		public int planeResolution = 100;
		private List<Vector2f> uvs = new List<Vector2f>();
		private List<Vector3f> vertices = new List<Vector3f>();
		private List<int> triangles = new List<int>();

		override public MeshGenerator Generate() {
			vertices = new List<Vector3f>(planeResolution * planeResolution);
			float u = 0;
			float v = -1;
			float uStepSize = MathUtil.TWO_P_IF / planeResolution;
			float vStepSize = 2.0f / planeResolution;
			v += vStepSize;
			float currX = 0;
			while (u <= MathUtil.TWO_P_IF) {
				float currY = 0;

				while (v <= 1) {
					uvs.Add(new Vector2f(currX / (planeResolution - 1), currY / (planeResolution - 1)));
					float x = (float)((float) (1 + ((v / 2.0f) * Math.Cos(u / 2.0f))) * Math.Cos(u));
					float y = (float)((1 + ((v / 2.0f) * Math.Cos(u / 2.0f))) * Math.Sin(u));
					float z = (float)((v / 2.0f) * Math.Sin((u) / 2.0f));
					Vector3f position = new Vector3f(x, y, z);
					vertices.Add(position);
					v += vStepSize;
					currY++;
				}
				currX++;
				v = -1 + vStepSize;
				u += uStepSize;
			}

			for (int i = 0; i < vertices.Count; i++) {
				if (!((i + 1) % planeResolution == 0)) {
					int index1 = i + 1;
					int index2 = i + planeResolution;
					int index3 = i + planeResolution + 1;
					if (index1 % vertices.Count != index1) {
						index1 %= vertices.Count;
						index1 = planeResolution - index1 - 1;
					}
					if (index2 % vertices.Count != index2) {
						index2 %= vertices.Count;
						index2 = planeResolution - index2 - 1;
					}
					if (index3 % vertices.Count != index3) {
						index3 %= vertices.Count;
						index3 = planeResolution - index3 - 1;
					}

					triangles.Add(i);
					triangles.Add(index1);
					triangles.Add(index2);

					triangles.Add(index2);
					triangles.Add(index1);
					triangles.Add(index3);
				}
			}

			base.vertices = new VectorArray3d(vertices.Count);
			for (int i = 0; i < vertices.Count; i++) {
				base.vertices[i] = vertices[i];
			}

			base.triangles = new IndexArray3i(triangles.Count);
			for (int i = 0; i < triangles.Count; i += 3) {
				base.triangles[i] = new Index3i(
					triangles[i],
					triangles[i + 1],
					triangles[i + 2]
				);
			}

			base.uv = new VectorArray2f(uvs.Count);
			for (int i = 0; i < uvs.Count; i++) {
				base.uv[i] = new Vector2f(
					uvs[i]
				);
			}

			// Wonky? PBR shader goes pitch-black with this
			base.normals = new VectorArray3f(vertices.Count);
			for (int i = 0; i < vertices.Count; i++) {
				base.normals[i] = new Vector3f(Estimate_normal(
					base.triangles[i].a,
					base.triangles[i].b,
					base.triangles[i].c
					)
				);
			}

			return this;
		}
	}
}
