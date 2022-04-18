using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public class CurvedPlaneMeshGenerator : MeshGenerator
	{
		public float bRadius = 1f;
		public float tRadius = 1f;
		public float Width = 1f;
		public int Slices = 1;
		public float Height = 1f;

		override public MeshGenerator Generate()
		{
			var height = Height;
			var segments = Slices * 2;

			var bray = (bRadius == 0) ? 1f : bRadius;
			bray = (float)(bray / 180f * Math.PI) * 2;

			var tray = (tRadius == 0) ? 1f : bRadius;
			tray = (float)(tray / 180f * Math.PI) * 2;

			vertices = new VectorArray3d(segments * 4);
			uv = new VectorArray2f(segments * 4);
			normals = new VectorArray3f(segments * 4);
			triangles = new IndexArray3i(segments * 2);
			var bdes = Width / 2f / (bRadius * (1f / 180f));
			var tdes = Width / 2f / (tRadius * (1f / 180f));

			var bangle = -(bray / 4);
			var bstep = bray / segments;
			var tangle = -(tray / 4);
			var tstep = tray / segments;
			for (var i = 0; i < segments; i++)
			{
				var bsin = (float)Math.Sin(bangle + (bstep * i));
				var bcos = (float)Math.Cos(bangle + (bstep * i));
				var tsin = (float)Math.Sin(tangle + (tstep * i));
				var tcos = (float)Math.Cos(tangle + (tstep * i));
				vertices[i * 2] = new Vector3d(bsin * bdes, bcos * bdes, height / 2) - new Vector3d(0, bdes, 0);
				vertices[(i * 2) + 1] = new Vector3d(tsin * tdes, tcos * tdes, -height / 2) - new Vector3d(0, bdes, 0);
				var upos = ((float)i) * (1f / ((float)Slices));
				uv[i * 2] = new Vector2f(upos, 1);
				uv[(i * 2) + 1] = new Vector2f(upos, 0);

				normals[i * 2] = new Vector3f(-bsin, -bcos, 0);
				normals[(i * 2) + 1] = new Vector3f(-tsin, -tcos, 0);

				if (i != segments - 1)
				{
					triangles[i * 2] = new Index3i(i, i + 1, i + 2);
					triangles[(i * 2) + 1] = new Index3i(i + 1, i + 3, i + 2);
				}

			}
			return this;
		}
	}

	// generate a triangle fan, no subdvisions
	// it has NO HOLE in the middle -dfg
	public class TrivialDiscGenerator : MeshGenerator
	{
		public float Radius = 1.0f;
		public float StartAngleDeg = 0.0f;
		public float EndAngleDeg = 360.0f;
		public int Slices = 32;

		override public MeshGenerator Generate()
		{
			vertices = new VectorArray3d(Slices + 1);
			uv = new VectorArray2f(Slices + 1);
			normals = new VectorArray3f(Slices + 1);
			triangles = new IndexArray3i(Slices);

			var vi = 0;
			vertices[vi] = Vector3d.Zero;
			uv[vi] = new Vector2f(0.5f, 0.5f);
			normals[vi] = Vector3f.AxisY;
			vi++;

			var bFullDisc = (EndAngleDeg - StartAngleDeg) > 359.99f;
			var fTotalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.DEG_2_RADF;
			var fStartRad = StartAngleDeg * MathUtil.DEG_2_RADF;
			var fDelta = bFullDisc ? fTotalRange / Slices : fTotalRange / (Slices - 1);
			for (var k = 0; k < Slices; ++k)
			{
				var a = fStartRad + ((float)k * fDelta);
				double cosa = Math.Cos(a), sina = Math.Sin(a);
				vertices[vi] = new Vector3d(Radius * cosa, 0, Radius * sina);
				uv[vi] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1 + sina));
				normals[vi] = Vector3f.AxisY;
				vi++;
			}

			var ti = 0;
			for (var k = 1; k < Slices; ++k) {
				triangles.Set(ti++, k, 0, k + 1, Clockwise);
			}

			if (bFullDisc)      // close disc if we went all the way
{
				triangles.Set(ti++, Slices, 0, 1, Clockwise);
			}

			return this;
		}
	}

	// generate a triangle fan, no subdvisions
	// it has a HOLE in the middle -dfg
	public class PuncturedDiscGenerator : MeshGenerator
	{
		public float OuterRadius = 1.0f;
		public float InnerRadius = 0.5f;
		public float StartAngleDeg = 0.0f;
		public float EndAngleDeg = 360.0f;
		public int Slices = 32;

		override public MeshGenerator Generate()
		{
			vertices = new VectorArray3d(2 * Slices);
			uv = new VectorArray2f(2 * Slices);
			normals = new VectorArray3f(2 * Slices);
			triangles = new IndexArray3i(2 * Slices);

			var bFullDisc = (EndAngleDeg - StartAngleDeg) > 359.99f;
			var fTotalRange = (EndAngleDeg - StartAngleDeg) * MathUtil.DEG_2_RADF;
			var fStartRad = StartAngleDeg * MathUtil.DEG_2_RADF;
			var fDelta = bFullDisc ? fTotalRange / Slices : fTotalRange / (Slices - 1);
			var fUVRatio = InnerRadius / OuterRadius;
			for (var k = 0; k < Slices; ++k)
			{
				var angle = fStartRad + (k * fDelta);
				double cosa = Math.Cos(angle), sina = Math.Sin(angle);
				vertices[k] = new Vector3d(InnerRadius * cosa, 0, InnerRadius * sina);
				vertices[Slices + k] = new Vector3d(OuterRadius * cosa, 0, OuterRadius * sina);
				uv[k] = new Vector2f(0.5f * (1.0f + (fUVRatio * cosa)), 0.5f * (1.0f + (fUVRatio * sina)));
				uv[Slices + k] = new Vector2f(0.5f * (1.0f + cosa), 0.5f * (1.0f + sina));
				normals[k] = normals[Slices + k] = Vector3f.AxisY;
			}

			var ti = 0;
			for (var k = 0; k < Slices - 1; ++k)
			{
				triangles.Set(ti++, k, k + 1, Slices + k + 1, Clockwise);
				triangles.Set(ti++, k, Slices + k + 1, Slices + k, Clockwise);
			}
			if (bFullDisc)
			{      // close disc if we went all the way
				triangles.Set(ti++, Slices - 1, 0, Slices, Clockwise);
				triangles.Set(ti++, Slices - 1, Slices, (2 * Slices) - 1, Clockwise);
			}

			return this;
		}
	}


}
