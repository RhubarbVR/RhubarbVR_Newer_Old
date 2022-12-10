using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// Port of Wm5Query2Int64 from WildMagic5 library by David Eberly / geometrictools.com

	/// <summary>
	/// 2D queries for integer coordinates. 
	/// Note that input Vector2d values are directly cast to int64 - you must
	/// scale them to suitable coordinates yourself!
	/// </summary>
	public sealed class Query2Int64 : Query2d
	{

		public Query2Int64(in IList<Vector2d> Vertices) : base(Vertices) {
		}


		public override int ToLine(ref Vector2d test, int v0, int v1) {
			var vec0 = mVertices[v0];
			var vec1 = mVertices[v1];

			var x0 = (long)test.x - (long)vec0.x;
			var y0 = (long)test.y - (long)vec0.y;
			var x1 = (long)vec1.x - (long)vec0.x;
			var y1 = (long)vec1.y - (long)vec0.y;

			var det = Det2(x0, y0, x1, y1);
			return det > 0 ? +1 : (det < 0 ? -1 : 0);
		}


		public override int ToCircumcircle(ref Vector2d test, int v0, int v1, int v2) {
			var vec0 = mVertices[v0];
			var vec1 = mVertices[v1];
			var vec2 = mVertices[v2];

			var iTest = new Vector2l((long)test.x, (long)test.y);
			var iV0 = new Vector2l((long)vec0.x, (long)vec0.y);
			var iV1 = new Vector2l((long)vec1.x, (long)vec1.y);
			var iV2 = new Vector2l((long)vec2.x, (long)vec2.y);

			var s0x = iV0.x + iTest.x;
			var d0x = iV0.x - iTest.x;
			var s0y = iV0.y + iTest.y;
			var d0y = iV0.y - iTest.y;
			var s1x = iV1.x + iTest.x;
			var d1x = iV1.x - iTest.x;
			var s1y = iV1.y + iTest.y;
			var d1y = iV1.y - iTest.y;
			var s2x = iV2.x + iTest.x;
			var d2x = iV2.x - iTest.x;
			var s2y = iV2.y + iTest.y;
			var d2y = iV2.y - iTest.y;
			var z0 = (s0x * d0x) + (s0y * d0y);
			var z1 = (s1x * d1x) + (s1y * d1y);
			var z2 = (s2x * d2x) + (s2y * d2y);
			var det = Det3(d0x, d0y, z0, d1x, d1y, z1, d2x, d2y, z2);
			return det < 0 ? 1 : (det > 0 ? -1 : 0);
		}

		static long Det2(in long x0, in long y0, in long x1, in long y1) {
			return (x0 * y1) - (x1 * y0);
		}

		static long Det3(in long x0, in long y0, in long z0, in long x1, in long y1, in long z1, in long x2, in long y2, in long z2) {
			var c00 = (y1 * z2) - (y2 * z1);
			var c01 = (y2 * z0) - (y0 * z2);
			var c02 = (y0 * z1) - (y1 * z0);
			return (x0 * c00) + (x1 * c01) + (x2 * c02);
		}













	}
}
