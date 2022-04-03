using System;
using System.Collections.Generic;

namespace RNumerics
{

	public static class MeshUtil
	{
		public static DCurve3 ExtractLoopV(IMesh mesh, IEnumerable<int> vertices)
		{
			var curve = new DCurve3();
			foreach (var vid in vertices) {
				curve.AppendVertex(mesh.GetVertex(vid));
			}

			curve.Closed = true;
			return curve;
		}
		public static DCurve3 ExtractLoopV(IMesh mesh, int[] vertices)
		{
			var curve = new DCurve3();
			for (var i = 0; i < vertices.Length; ++i) {
				curve.AppendVertex(mesh.GetVertex(vertices[i]));
			}

			curve.Closed = true;
			return curve;
		}

	}
}
