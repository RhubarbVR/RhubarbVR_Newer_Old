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

		// TODO Make this in-place with base.triangles
		// No, do not make this readonly. It will only result in suffering
		private readonly List<int> _tris = new();

		override public MeshGenerator Generate() {
			base.vertices = new VectorArray3d(planeResolution * planeResolution);
			base.uv = new VectorArray2f(planeResolution * planeResolution);
			float u = 0;
			float v = -1;
			var uStepSize = MathUtil.TWO_P_IF / planeResolution;
			var vStepSize = 2.0f / planeResolution;
			v += vStepSize;
			float currX = 0;
			var vertexCounter = 0;
			while (u <= MathUtil.TWO_P_IF) {
				float currY = 0;

				while (v <= 1) {
					base.uv[vertexCounter] = new Vector2f(currX / (planeResolution - 1), currY / (planeResolution - 1));
					var x = (float)((float) (1 + (v / 2.0f * Math.Cos(u / 2.0f))) * Math.Cos(u));
					var y = (float)((1 + (v / 2.0f * Math.Cos(u / 2.0f))) * Math.Sin(u));
					var z = (float)(v / 2.0f * Math.Sin(u / 2.0f));
					var position = new Vector3f(x, y, z);
					base.vertices[vertexCounter] = position;
					v += vStepSize;
					vertexCounter++;
					currY++;
				}
				currX++;
				v = -1 + vStepSize;
				u += uStepSize;
			}

			for (var i = 0; i < vertices.Count; i++) {
				if (!((i + 1) % planeResolution == 0)) {
					var index1 = i + 1;
					var index2 = i + planeResolution;
					var index3 = i + planeResolution + 1;
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

					_tris.Add(i);
					_tris.Add(index1);
					_tris.Add(index2);

					_tris.Add(index2);
					_tris.Add(index1);
					_tris.Add(index3);
				}
			}

			base.triangles = new IndexArray3i(_tris.Count);
			for (var i = 0; i < _tris.Count; i += 3) {
				base.triangles[i] = new Index3i(
					_tris[i],
					_tris[i + 1],
					_tris[i + 2]
				);
			}

			base.normals = new VectorArray3f(vertices.Count);
			for (var i = 0; i < vertices.Count; i++) {
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
