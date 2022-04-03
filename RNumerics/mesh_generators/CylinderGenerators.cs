using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// generate a cylinder 
	public class OpenCylinderGenerator : MeshGenerator
	{
		public float BaseRadius = 1.0f;
		public float TopRadius = 1.0f;
		public float Height = 1.0f;
		public float StartAngleDeg = 0.0f;
		public float EndAngleDeg = 360.0f;
		public int Slices = 16;

		// set to true if you are going to texture this cylinder, otherwise
		// last panel will not have UVs going from 1 to 0
		public bool NoSharedVertices = false;

		override public MeshGenerator Generate() {
			var bClosed = (EndAngleDeg - StartAngleDeg) > 359.99f;
			var nRingSize = (NoSharedVertices && bClosed) ? Slices + 1 : Slices;
			vertices = new VectorArray3d(2 * nRingSize);
			uv = new VectorArray2f(vertices.Count);
			normals = new VectorArray3f(vertices.Count);
			triangles = new IndexArray3i(2 * Slices);

			var fTotalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.DEG_2_RADF;
			var fStartRad = StartAngleDeg * MathUtil.DEG_2_RADF;
			var fDelta = bClosed ? fTotalRange / Slices : fTotalRange / (Slices - 1);
			for (var k = 0; k < nRingSize; ++k) {
				var angle = fStartRad + (k * fDelta);
				double cosa = Math.Cos(angle), sina = Math.Sin(angle);
				vertices[k] = new Vector3d(BaseRadius * cosa, 0, BaseRadius * sina);
				vertices[nRingSize + k] = new Vector3d(TopRadius * cosa, Height, TopRadius * sina);
				var t = k / (float)Slices;
				uv[k] = new Vector2f(t, 0.0f);
				uv[nRingSize + k] = new Vector2f(t, 1.0f);
				var n = new Vector3f((float)cosa, 0, (float)sina);
				n.Normalize();
				normals[k] = normals[nRingSize + k] = n;
			}

			var ti = 0;
			for (var k = 0; k < nRingSize - 1; ++k) {
				triangles.Set(ti++, k, k + 1, nRingSize + k + 1, Clockwise);
				triangles.Set(ti++, k, nRingSize + k + 1, nRingSize + k, Clockwise);
			}
			if (bClosed && NoSharedVertices == false) {      // close disc if we went all the way
				triangles.Set(ti++, nRingSize - 1, 0, nRingSize, Clockwise);
				triangles.Set(ti++, nRingSize - 1, nRingSize, (2 * nRingSize) - 1, Clockwise);
			}

			return this;
		}
	}




	/// <summary>
	/// Generate a Cylinder with caps. Supports sections of cylinder as well (eg wedges).
	/// Curently UV islands are overlapping for different mesh components, if NoSharedVertices
	/// Positioned along Y axis such that base-center is at Origin, and top is at Y=Height
	/// You get a cone unless BaseRadius = TopRadius
	/// No subdivisions along top/base rings or height steps.
	/// cylinder triangles have groupid = 1, top cap = 2, bottom cap = 3, wedge faces 5 and 6
	/// </summary>
	public class CappedCylinderGenerator : MeshGenerator
	{
		public float BaseRadius = 1.0f;
		public float TopRadius = 1.0f;
		public float Height = 1.0f;
		public float StartAngleDeg = 0.0f;
		public float EndAngleDeg = 360.0f;
		public int Slices = 16;

		// set to true if you are going to texture this cylinder or want sharp edges
		public bool NoSharedVertices = false;

		override public MeshGenerator Generate() {
			var bClosed = (EndAngleDeg - StartAngleDeg) > 359.99f;
			var nRingSize = (NoSharedVertices && bClosed) ? Slices + 1 : Slices;
			var nCapVertices = NoSharedVertices ? Slices + 1 : 1;
			var nFaceVertices = (NoSharedVertices && bClosed == false) ? 8 : 0;
			vertices = new VectorArray3d((2 * nRingSize) + (2 * nCapVertices) + nFaceVertices);
			uv = new VectorArray2f(vertices.Count);
			normals = new VectorArray3f(vertices.Count);

			var nCylTris = 2 * Slices;
			var nCapTris = 2 * Slices;
			var nFaceTris = (bClosed == false) ? 4 : 0;
			triangles = new IndexArray3i(nCylTris + nCapTris + nFaceTris);
			groups = new int[triangles.Count];

			var fTotalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.DEG_2_RADF;
			var fStartRad = StartAngleDeg * MathUtil.DEG_2_RADF;
			var fDelta = bClosed ? fTotalRange / Slices : fTotalRange / (Slices - 1);

			// generate top and bottom rings for cylinder
			for (var k = 0; k < nRingSize; ++k) {
				var angle = fStartRad + (k * fDelta);
				double cosa = Math.Cos(angle), sina = Math.Sin(angle);
				vertices[k] = new Vector3d(BaseRadius * cosa, 0, BaseRadius * sina);
				vertices[nRingSize + k] = new Vector3d(TopRadius * cosa, Height, TopRadius * sina);
				var t = (float)k / Slices;
				uv[k] = new Vector2f(t, 0.0f);
				uv[nRingSize + k] = new Vector2f(t, 1.0f);
				var n = new Vector3f((float)cosa, 0, (float)sina);
				n.Normalize();
				normals[k] = normals[nRingSize + k] = n;
			}

			// generate cylinder panels
			var ti = 0;
			for (var k = 0; k < nRingSize - 1; ++k) {
				groups[ti] = 1;
				triangles.Set(ti++, k, k + 1, nRingSize + k + 1, Clockwise);
				groups[ti] = 1;
				triangles.Set(ti++, k, nRingSize + k + 1, nRingSize + k, Clockwise);
			}
			if (bClosed && NoSharedVertices == false) {      // close disc if we went all the way
				groups[ti] = 1;
				triangles.Set(ti++, nRingSize - 1, 0, nRingSize, Clockwise);
				groups[ti] = 1;
				triangles.Set(ti++, nRingSize - 1, nRingSize, (2 * nRingSize) - 1, Clockwise);
			}

			var nBottomC = 2 * nRingSize;
			vertices[nBottomC] = new Vector3d(0, 0, 0);
			uv[nBottomC] = new Vector2f(0.5f, 0.5f);
			normals[nBottomC] = new Vector3f(0, -1, 0);

			var nTopC = (2 * nRingSize) + 1;
			vertices[nTopC] = new Vector3d(0, Height, 0);
			uv[nTopC] = new Vector2f(0.5f, 0.5f);
			normals[nTopC] = new Vector3f(0, 1, 0);

			if (NoSharedVertices) {
				var nStartB = (2 * nRingSize) + 2;
				for (var k = 0; k < Slices; ++k) {
					var a = fStartRad + (k * fDelta);
					double cosa = Math.Cos(a), sina = Math.Sin(a);
					vertices[nStartB + k] = new Vector3d(BaseRadius * cosa, 0, BaseRadius * sina);
					uv[nStartB + k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
					normals[nStartB + k] = -Vector3f.AxisY;
				}
				Append_disc(Slices, nBottomC, nStartB, bClosed, Clockwise, ref ti, 2);

				var nStartT = (2 * nRingSize) + 2 + Slices;
				for (var k = 0; k < Slices; ++k) {
					var a = fStartRad + (k * fDelta);
					double cosa = Math.Cos(a), sina = Math.Sin(a);
					vertices[nStartT + k] = new Vector3d(TopRadius * cosa, Height, TopRadius * sina);
					uv[nStartT + k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
					normals[nStartT + k] = Vector3f.AxisY;
				}
				Append_disc(Slices, nTopC, nStartT, bClosed, !Clockwise, ref ti, 3);

				// ugh this is very ugly but hard to see the pattern...
				if (bClosed == false) {
					var nStartF = (2 * nRingSize) + 2 + (2 * Slices);
					vertices[nStartF] = vertices[nStartF + 5] = vertices[nBottomC];
					vertices[nStartF + 1] = vertices[nStartF + 4] = vertices[nTopC];
					vertices[nStartF + 2] = vertices[nRingSize];
					vertices[nStartF + 3] = vertices[0];
					vertices[nStartF + 6] = vertices[nRingSize - 1];
					vertices[nStartF + 7] = vertices[(2 * nRingSize) - 1];
					normals[nStartF] = normals[nStartF + 1] = normals[nStartF + 2] = normals[nStartF + 3]
						= Estimate_normal(nStartF, nStartF + 1, nStartF + 2);
					normals[nStartF + 4] = normals[nStartF + 5] = normals[nStartF + 6] = normals[nStartF + 7]
						= Estimate_normal(nStartF + 4, nStartF + 5, nStartF + 6);

					uv[nStartF] = uv[nStartF + 5] = new Vector2f(0, 0);
					uv[nStartF + 1] = uv[nStartF + 4] = new Vector2f(0, 1);
					uv[nStartF + 2] = uv[nStartF + 7] = new Vector2f(1, 1);
					uv[nStartF + 3] = uv[nStartF + 6] = new Vector2f(1, 0);

					Append_rectangle(nStartF + 0, nStartF + 1, nStartF + 2, nStartF + 3, !Clockwise, ref ti, 4);
					Append_rectangle(nStartF + 4, nStartF + 5, nStartF + 6, nStartF + 7, !Clockwise, ref ti, 5);
				}

			}
			else {
				Append_disc(Slices, nBottomC, 0, bClosed, Clockwise, ref ti, 2);
				Append_disc(Slices, nTopC, nRingSize, bClosed, !Clockwise, ref ti, 3);
				if (bClosed == false) {
					Append_rectangle(nBottomC, 0, nRingSize, nTopC, Clockwise, ref ti, 4);
					Append_rectangle(nRingSize - 1, nBottomC, nTopC, (2 * nRingSize) - 1, Clockwise, ref ti, 5);
				}
			}

			return this;
		}
	}




	// Generate a cone with base caps. Supports sections of cone as well (eg wedges).
	// Curently UV islands are overlapping for different mesh components, if NoSharedVertices
	// Also, if NoSharedVertices, then the 'tip' vertex is duplicated Slices times.
	// This causes the normals to look...weird.
	// For the conical region, we use the planar disc parameterization (ie tip at .5,.5) rather than
	// a cylinder-like projection
	public class ConeGenerator : MeshGenerator
	{
		public float BaseRadius = 1.0f;
		public float Height = 1.0f;
		public float StartAngleDeg = 0.0f;
		public float EndAngleDeg = 360.0f;
		public int Slices = 16;

		// set to true if you are going to texture this cone or want sharp edges
		public bool NoSharedVertices = false;


		override public MeshGenerator Generate() {
			var bClosed = (EndAngleDeg - StartAngleDeg) > 359.99f;
			var nRingSize = (NoSharedVertices && bClosed) ? Slices + 1 : Slices;
			var nTipVertices = NoSharedVertices ? nRingSize : 1;
			var nCapVertices = NoSharedVertices ? Slices + 1 : 1;
			var nFaceVertices = (NoSharedVertices && bClosed == false) ? 6 : 0;
			vertices = new VectorArray3d(nRingSize + nTipVertices + nCapVertices + nFaceVertices);
			uv = new VectorArray2f(vertices.Count);
			normals = new VectorArray3f(vertices.Count);

			var nConeTris = NoSharedVertices ? 2 * Slices : Slices;
			var nCapTris = Slices;
			var nFaceTris = (bClosed == false) ? 2 : 0;
			triangles = new IndexArray3i(nConeTris + nCapTris + nFaceTris);

			var fTotalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.DEG_2_RADF;
			var fStartRad = StartAngleDeg * MathUtil.DEG_2_RADF;
			var fDelta = bClosed ? fTotalRange / Slices : fTotalRange / (Slices - 1);

			// generate rings
			for (var k = 0; k < nRingSize; ++k) {
				var angle = fStartRad + (k * fDelta);
				double cosa = Math.Cos(angle), sina = Math.Sin(angle);
				vertices[k] = new Vector3d(BaseRadius * cosa, 0, BaseRadius * sina);
				uv[k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
				var n = new Vector3f(cosa * Height, BaseRadius / Height, sina * Height);
				n.Normalize();
				normals[k] = n;

				if (NoSharedVertices) {
					vertices[nRingSize + k] = new Vector3d(0, Height, 0);
					uv[nRingSize + k] = new Vector2f(0.5f, 0.5f);
					normals[nRingSize + k] = n;
				}
			}
			if (NoSharedVertices == false) {
				vertices[nRingSize] = new Vector3d(0, Height, 0);
				normals[nRingSize] = Vector3f.AxisY;
				uv[nRingSize] = new Vector2f(0.5f, 0.5f);
			}

			// generate cylinder panels
			var ti = 0;
			if (NoSharedVertices) {
				for (var k = 0; k < nRingSize - 1; ++k) {
					triangles.Set(ti++, k, k + 1, nRingSize + k + 1, Clockwise);
					triangles.Set(ti++, k, nRingSize + k + 1, nRingSize + k, Clockwise);
				}

			}
			else {
				Append_disc(Slices, nRingSize, 0, bClosed, !Clockwise, ref ti);
			}

			var nBottomC = nRingSize + nTipVertices;
			vertices[nBottomC] = new Vector3d(0, 0, 0);
			uv[nBottomC] = new Vector2f(0.5f, 0.5f);
			normals[nBottomC] = new Vector3f(0, -1, 0);

			if (NoSharedVertices) {
				var nStartB = nBottomC + 1;
				for (var k = 0; k < Slices; ++k) {
					var a = fStartRad + (k * fDelta);
					double cosa = Math.Cos(a), sina = Math.Sin(a);
					vertices[nStartB + k] = new Vector3d(BaseRadius * cosa, 0, BaseRadius * sina);
					uv[nStartB + k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
					normals[nStartB + k] = -Vector3f.AxisY;
				}
				Append_disc(Slices, nBottomC, nStartB, bClosed, Clockwise, ref ti);

				// ugh this is very ugly but hard to see the pattern...
				if (bClosed == false) {
					var nStartF = nStartB + Slices;
					vertices[nStartF] = vertices[nStartF + 4] = vertices[nBottomC];
					vertices[nStartF + 1] = vertices[nStartF + 3] = new Vector3d(0, Height, 0);
					;
					vertices[nStartF + 2] = vertices[0];
					;
					vertices[nStartF + 5] = vertices[nRingSize - 1];
					normals[nStartF] = normals[nStartF + 1] = normals[nStartF + 2]
						= Estimate_normal(nStartF, nStartF + 1, nStartF + 2);
					normals[nStartF + 3] = normals[nStartF + 4] = normals[nStartF + 5]
						= Estimate_normal(nStartF + 3, nStartF + 4, nStartF + 5);

					uv[nStartF] = uv[nStartF + 4] = new Vector2f(0, 0);
					uv[nStartF + 1] = uv[nStartF + 3] = new Vector2f(0, 1);
					uv[nStartF + 2] = uv[nStartF + 5] = new Vector2f(1, 0);

					triangles.Set(ti++, nStartF + 0, nStartF + 1, nStartF + 2, !Clockwise);
					triangles.Set(ti++, nStartF + 3, nStartF + 4, nStartF + 5, !Clockwise);
				}

			}
			else {
				Append_disc(Slices, nBottomC, 0, bClosed, Clockwise, ref ti);
				if (bClosed == false) {
					triangles.Set(ti++, nBottomC, nRingSize, 0, !Clockwise);
					triangles.Set(ti++, nBottomC, nRingSize, nRingSize - 1, Clockwise);
				}
			}

			return this;
		}
	}



	public class VerticalGeneralizedCylinderGenerator : MeshGenerator
	{
		public CircularSection[] Sections;
		public int Slices = 16;
		public bool Capped = true;

		// set to true if you are going to texture this cone or want sharp edges
		public bool NoSharedVertices = true;

		public int startCapCenterIndex = -1;
		public int endCapCenterIndex = -1;

		override public MeshGenerator Generate() {
			var nRings = NoSharedVertices ? 2 * (Sections.Length - 1) : Sections.Length;
			var nRingSize = NoSharedVertices ? Slices + 1 : Slices;
			var nCapVertices = NoSharedVertices ? Slices + 1 : 1;
			if (Capped == false) {
				nCapVertices = 0;
			}

			vertices = new VectorArray3d((nRings * nRingSize) + (2 * nCapVertices));
			uv = new VectorArray2f(vertices.Count);
			normals = new VectorArray3f(vertices.Count);

			var nSpanTris = (Sections.Length - 1) * 2 * Slices;
			var nCapTris = Capped ? 2 * Slices : 0;
			triangles = new IndexArray3i(nSpanTris + nCapTris);

			var fDelta = (float)(Math.PI * 2.0 / Slices);

			var fYSpan = Sections.Last().SectionY - Sections[0].SectionY;
			if (fYSpan == 0) {
				fYSpan = 1.0f;
			}

			// generate top and bottom rings for cylinder
			var ri = 0;
			for (var si = 0; si < Sections.Length; ++si) {
				var nStartR = ri * nRingSize;
				var y = Sections[si].SectionY;
				var yt = (y - Sections[0].SectionY) / fYSpan;
				for (var j = 0; j < nRingSize; ++j) {
					var k = nStartR + j;
					var angle = (float)j * fDelta;
					double cosa = Math.Cos(angle), sina = Math.Sin(angle);
					vertices[k] = new Vector3d(Sections[si].Radius * cosa, y, Sections[si].Radius * sina);
					var t = (float)j / (float)(Slices - 1);
					uv[k] = new Vector2f(t, yt);
					var n = new Vector3f((float)cosa, 0, (float)sina);
					n.Normalize();
					normals[k] = n;
				}
				ri++;
				if (NoSharedVertices && si != 0 && si != Sections.Length - 1) {
					Duplicate_vertex_span(nStartR, nRingSize);
					ri++;
				}
			}

			// generate triangles
			var ti = 0;
			ri = 0;
			for (var si = 0; si < Sections.Length - 1; ++si) {
				var r0 = ri * nRingSize;
				var r1 = r0 + nRingSize;
				ri += NoSharedVertices ? 2 : 1;
				for (var k = 0; k < nRingSize - 1; ++k) {
					triangles.Set(ti++, r0 + k, r0 + k + 1, r1 + k + 1, Clockwise);
					triangles.Set(ti++, r0 + k, r1 + k + 1, r1 + k, Clockwise);
				}
				if (NoSharedVertices == false) {      // close disc if we went all the way
					triangles.Set(ti++, r1 - 1, r0, r1, Clockwise);
					triangles.Set(ti++, r1 - 1, r1, r1 + nRingSize - 1, Clockwise);
				}
			}

			if (Capped) {
				// add endcap verts
				var s0 = Sections[0];
				var sN = Sections.Last();
				var nBottomC = nRings * nRingSize;
				vertices[nBottomC] = new Vector3d(0, s0.SectionY, 0);
				uv[nBottomC] = new Vector2f(0.5f, 0.5f);
				normals[nBottomC] = new Vector3f(0, -1, 0);
				startCapCenterIndex = nBottomC;

				var nTopC = nBottomC + 1;
				vertices[nTopC] = new Vector3d(0, sN.SectionY, 0);
				uv[nTopC] = new Vector2f(0.5f, 0.5f);
				normals[nTopC] = new Vector3f(0, 1, 0);
				endCapCenterIndex = nTopC;

				if (NoSharedVertices) {
					var nStartB = nTopC + 1;
					for (var k = 0; k < Slices; ++k) {
						var a = (float)k * fDelta;
						double cosa = Math.Cos(a), sina = Math.Sin(a);
						vertices[nStartB + k] = new Vector3d(s0.Radius * cosa, s0.SectionY, s0.Radius * sina);
						uv[nStartB + k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
						normals[nStartB + k] = -Vector3f.AxisY;
					}
					Append_disc(Slices, nBottomC, nStartB, true, Clockwise, ref ti);

					var nStartT = nStartB + Slices;
					for (var k = 0; k < Slices; ++k) {
						var a = k * fDelta;
						double cosa = Math.Cos(a), sina = Math.Sin(a);
						vertices[nStartT + k] = new Vector3d(sN.Radius * cosa, sN.SectionY, sN.Radius * sina);
						uv[nStartT + k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
						normals[nStartT + k] = Vector3f.AxisY;
					}
					Append_disc(Slices, nTopC, nStartT, true, !Clockwise, ref ti);

				}
				else {
					Append_disc(Slices, nBottomC, 0, true, Clockwise, ref ti);
					Append_disc(Slices, nTopC, nRingSize * (Sections.Length - 1), true, !Clockwise, ref ti);
				}
			}

			return this;
		}
	}



}
