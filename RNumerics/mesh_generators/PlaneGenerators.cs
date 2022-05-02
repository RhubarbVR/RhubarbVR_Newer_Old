using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// generate a two-triangle rect, centered at origin
	public class TrivialRectGenerator : MeshGenerator
	{
		public float Width = 1.0f;
		public float Height = 1.0f;

		public Vector3f Normal = Vector3f.AxisY;

		/// <summary>
		/// How to map 2D indices to 3D. Default is (x,0,z). Set this value to (1,2) if you want (x,y,0).
		/// Set values to negative to mirror on that axis.
		/// </summary>
		public Index2i IndicesMap = new(1, 3);

		public enum UVModes
		{
			FullUVSquare,
			CenteredUVRectangle,
			BottomCornerUVRectangle
		}
		public UVModes UVMode = UVModes.FullUVSquare;


		virtual protected Vector3d Make_vertex(float x, float y) {
			var v = Vector3d.Zero;
			v[Math.Abs(IndicesMap.a) - 1] = (IndicesMap.a < 0) ? -x : x;
			v[Math.Abs(IndicesMap.b) - 1] = (IndicesMap.b < 0) ? -y : y;
			return v;
		}

		override public MeshGenerator Generate() {
			if (MathUtil.InRange(IndicesMap.a, 1, 3) == false || MathUtil.InRange(IndicesMap.b, 1, 3) == false) {
				throw new Exception("TrivialRectGenerator: Invalid IndicesMap!");
			}

			vertices = new VectorArray3d(4);
			uv = new VectorArray2f(4);
			normals = new VectorArray3f(4);
			triangles = new IndexArray3i(2);

			vertices[0] = Make_vertex(-Width / 2.0f, -Height / 2.0f);
			vertices[1] = Make_vertex(Width / 2.0f, -Height / 2.0f);
			vertices[2] = Make_vertex(Width / 2.0f, Height / 2.0f);
			vertices[3] = Make_vertex(-Width / 2.0f, Height / 2.0f);

			normals[0] = normals[1] = normals[2] = normals[3] = Normal;

			float uvleft = 0.0f, uvright = 1.0f, uvbottom = 0.0f, uvtop = 1.0f;

			// if we want the UV subregion, we assume it is 
			if (UVMode != UVModes.FullUVSquare) {
				if (Width > Height) {
					var a = Height / Width;
					if (UVMode == UVModes.CenteredUVRectangle) {
						uvbottom = 0.5f - (a / 2.0f);
						uvtop = 0.5f + (a / 2.0f);
					}
					else {
						uvtop = a;
					}
				}
				else if (Height > Width) {
					var a = Width / Height;
					if (UVMode == UVModes.CenteredUVRectangle) {
						uvleft = 0.5f - (a / 2.0f);
						uvright = 0.5f + (a / 2.0f);
					}
					else {
						uvright = a;
					}
				}
			}

			uv[0] = new Vector2f(uvleft, uvbottom);
			uv[1] = new Vector2f(uvright, uvbottom);
			uv[2] = new Vector2f(uvright, uvtop);
			uv[3] = new Vector2f(uvleft, uvtop);

			if (Clockwise == true) {
				triangles.Set(0, 0, 1, 2);
				triangles.Set(1, 0, 2, 3);
			}
			else {
				triangles.Set(0, 0, 2, 1);
				triangles.Set(1, 0, 3, 2);
			}

			return this;
		}
	}






	/// <summary>
	/// Generate a mesh of a rect that has "gridded" faces, ie grid of triangulated quads, 
	/// with EdgeVertices verts along each edge.
	/// [TODO] allow varying EdgeVertices in each dimension (tricky...)
	/// </summary>
	public class GriddedRectGenerator : TrivialRectGenerator
	{
		public int EdgeVertices = 8;

		override public MeshGenerator Generate() {
			if (MathUtil.InRange(IndicesMap.a, 1, 3) == false || MathUtil.InRange(IndicesMap.b, 1, 3) == false) {
				throw new Exception("GriddedRectGenerator: Invalid IndicesMap!");
			}

			var N = (EdgeVertices > 1) ? EdgeVertices : 2;
			int NT = N - 1, N2 = N * N;
			vertices = new VectorArray3d(N2);
			uv = new VectorArray2f(vertices.Count);
			normals = new VectorArray3f(vertices.Count);
			triangles = new IndexArray3i(2 * NT * NT);
			groups = new int[triangles.Count];

			// corner vertices
			var v00 = Make_vertex(-Width / 2.0f, -Height / 2.0f);
			var v01 = Make_vertex(Width / 2.0f, -Height / 2.0f);
			var v11 = Make_vertex(Width / 2.0f, Height / 2.0f);
			var v10 = Make_vertex(-Width / 2.0f, Height / 2.0f);

			// corner UVs
			float uvleft = 0.0f, uvright = 1.0f, uvbottom = 0.0f, uvtop = 1.0f;

			if (UVMode != UVModes.FullUVSquare) {
				if (Width > Height) {
					var a = Height / Width;
					if (UVMode == UVModes.CenteredUVRectangle) {
						uvbottom = 0.5f - (a / 2.0f);
						uvtop = 0.5f + (a / 2.0f);
					}
					else {
						uvtop = a;
					}
				}
				else if (Height > Width) {
					var a = Width / Height;
					if (UVMode == UVModes.CenteredUVRectangle) {
						uvleft = 0.5f - (a / 2.0f);
						uvright = 0.5f + (a / 2.0f);
					}
					else {
						uvright = a;
					}
				}
			}

			var uv00 = new Vector2f(uvleft, uvbottom);
			var uv01 = new Vector2f(uvright, uvbottom);
			var uv11 = new Vector2f(uvright, uvtop);
			var uv10 = new Vector2f(uvleft, uvtop);

			var vi = 0;
			var ti = 0;

			// add vertex rows
			var start_vi = vi;
			for (var yi = 0; yi < N; ++yi) {
				var ty = (double)yi / (double)(N - 1);
				for (var xi = 0; xi < N; ++xi) {
					var tx = (double)xi / (double)(N - 1);
					normals[vi] = Normal;
					uv[vi] = Bilerp(ref uv00, ref uv01, ref uv11, ref uv10, (float)tx, (float)ty);
					vertices[vi++] = Bilerp(ref v00, ref v01, ref v11, ref v10, tx, ty);
				}
			}

			// add faces
			for (var y0 = 0; y0 < NT; ++y0) {
				for (var x0 = 0; x0 < NT; ++x0) {
					var i00 = start_vi + (y0 * N) + x0;
					var i10 = start_vi + ((y0 + 1) * N) + x0;
					int i01 = i00 + 1, i11 = i10 + 1;

					groups[ti] = 0;
					triangles.Set(ti++, i00, i11, i01, Clockwise);
					groups[ti] = 0;
					triangles.Set(ti++, i00, i10, i11, Clockwise);
				}
			}

			return this;
		}
	}












	// Generate a rounded rect centered at origin.
	// Force individual corners to be sharp using the SharpCorners flags field.
	public class RoundRectGenerator : MeshGenerator
	{
		public float Width = 1.0f;
		public float Height = 1.0f;
		public float Radius = 0.1f;
		public int CornerSteps = 4;


		[Flags]
		public enum Corner
		{
			BottomLeft = 1,
			BottomRight = 2,
			TopRight = 4,
			TopLeft = 8
		}
		public Corner SharpCorners = 0;


		public enum UVModes
		{
			FullUVSquare,
			CenteredUVRectangle,
			BottomCornerUVRectangle
		}
		public UVModes UVMode = UVModes.FullUVSquare;

		// order is [inner_corner, outer_1, outer_2]
		static readonly int[] _corner_spans = new int[] { 0, 11, 4, 1, 5, 6, 2, 7, 8, 3, 9, 10 };

		override public MeshGenerator Generate() {
			int corner_v = 0, corner_t = 0;
			for (var k = 0; k < 4; ++k) {
				if (((int)SharpCorners & (1 << k)) != 0) {
					corner_v += 1;
					corner_t += 2;
				}
				else {
					corner_v += CornerSteps;
					corner_t += CornerSteps + 1;
				}
			}

			vertices = new VectorArray3d(12 + corner_v);
			uv = new VectorArray2f(vertices.Count);
			normals = new VectorArray3f(vertices.Count);
			triangles = new IndexArray3i(10 + corner_t);

			var innerW = Width - (2 * Radius);
			var innerH = Height - (2 * Radius);

			// make vertices for inner "cross" (ie 5 squares)
			vertices[0] = new Vector3d(-innerW / 2.0f, 0, -innerH / 2.0f);
			vertices[1] = new Vector3d(innerW / 2.0f, 0, -innerH / 2.0f);
			vertices[2] = new Vector3d(innerW / 2.0f, 0, innerH / 2.0f);
			vertices[3] = new Vector3d(-innerW / 2.0f, 0, innerH / 2.0f);

			vertices[4] = new Vector3d(-innerW / 2, 0, -Height / 2);
			vertices[5] = new Vector3d(innerW / 2, 0, -Height / 2);

			vertices[6] = new Vector3d(Width / 2, 0, -innerH / 2);
			vertices[7] = new Vector3d(Width / 2, 0, innerH / 2);

			vertices[8] = new Vector3d(innerW / 2, 0, Height / 2);
			vertices[9] = new Vector3d(-innerW / 2, 0, Height / 2);

			vertices[10] = new Vector3d(-Width / 2, 0, innerH / 2);
			vertices[11] = new Vector3d(-Width / 2, 0, -innerH / 2);

			// make triangles for inner cross
			var cycle = !Clockwise;
			var ti = 0;
			Append_rectangle(0, 1, 2, 3, cycle, ref ti);
			Append_rectangle(4, 5, 1, 0, cycle, ref ti);
			Append_rectangle(1, 6, 7, 2, cycle, ref ti);
			Append_rectangle(3, 2, 8, 9, cycle, ref ti);
			Append_rectangle(11, 0, 3, 10, cycle, ref ti);

			var vi = 12;
			for (var j = 0; j < 4; ++j) {
				var sharp = ((int)SharpCorners & (1 << j)) > 0;
				if (sharp) {
					Append_2d_disc_segment(_corner_spans[3 * j], _corner_spans[(3 * j) + 1], _corner_spans[(3 * j) + 2], 1,
						cycle, ref vi, ref ti, -1, MathUtil.SQRT_TWO * Radius);
				}
				else {
					Append_2d_disc_segment(_corner_spans[3 * j], _corner_spans[(3 * j) + 1], _corner_spans[(3 * j) + 2], CornerSteps,
						cycle, ref vi, ref ti);
				}
			}


			for (var k = 0; k < vertices.Count; ++k) {
				normals[k] = Vector3f.AxisY;
			}

			float uvleft = 0.0f, uvright = 1.0f, uvbottom = 0.0f, uvtop = 1.0f;

			// if we want the UV subregion, we assume it is 
			if (UVMode != UVModes.FullUVSquare) {
				if (Width > Height) {
					var a = Height / Width;
					if (UVMode == UVModes.CenteredUVRectangle) {
						uvbottom = 0.5f - (a / 2.0f);
						uvtop = 0.5f + (a / 2.0f);
					}
					else {
						uvtop = a;
					}
				}
				else if (Height > Width) {
					var a = Width / Height;
					if (UVMode == UVModes.CenteredUVRectangle) {
						uvleft = 0.5f - (a / 2.0f);
						uvright = 0.5f + (a / 2.0f);
					}
					else {
						uvright = a;
					}
				}
			}

			var c = new Vector3d(-Width / 2, 0, -Height / 2);
			for (var k = 0; k < vertices.Count; ++k) {
				var v = vertices[k];
				var tx = (v.x - c.x) / Width;
				var ty = (v.y - c.y) / Height;
				uv[k] = new Vector2f(((1 - tx) * uvleft) + (tx * uvright),
									  ((1 - ty) * uvbottom) + (ty * uvtop));
			}

			return this;
		}



		static readonly float[] _signx = new float[] { 1, 1, -1, -1 };
		static readonly float[] _signy = new float[] { -1, 1, 1, -1 };
		static readonly float[] _startangle = new float[] { 270, 0, 90, 180 };
		static readonly float[] _endangle = new float[] { 360, 90, 180, 270 };

		/// <summary>
		/// This is a utility function that returns the set of border points, which
		/// is useful when we use a roundrect as a UI element and want the border
		/// </summary>
		public Vector3d[] GetBorderLoop() {
			var corner_v = 0;
			for (var k = 0; k < 4; ++k) {
				if (((int)SharpCorners & (1 << k)) != 0) {
					corner_v += 1;
				}
				else {
					corner_v += CornerSteps;
				}
			}

			var innerW = Width - (2 * Radius);
			var innerH = Height - (2 * Radius);

			var vertices = new Vector3d[4 + corner_v];
			var vi = 0;

			for (var i = 0; i < 4; ++i) {
				vertices[vi++] = new Vector3d(_signx[i] * Width / 2, 0, _signy[i] * Height / 2);

				var sharp = ((int)SharpCorners & (1 << i)) > 0;
				var arc = new Arc2d(new Vector2d(_signx[i] * innerW, _signy[i] * innerH),
					sharp ? MathUtil.SQRT_TWO * Radius : Radius,
					_startangle[i], _endangle[i]);
				var use_steps = sharp ? 1 : CornerSteps;
				for (var k = 0; k < use_steps; ++k) {
					var t = (double)(i + 1) / (double)(use_steps + 1);
					var pos = arc.SampleT(t);
					vertices[vi++] = new Vector3d(pos.x, 0, pos.y);
				}
			}

			return vertices;
		}


	}

}
