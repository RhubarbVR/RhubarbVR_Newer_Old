using System;

namespace RNumerics
{
	public static class MeshWeights
	{
		// tan(theta/2) = +/- sqrt( (1-cos(theta)) / (1+cos(theta)) )
		// (in context above we never want negative value!)
		public static double VectorTanHalfAngle(in Vector3d a, in Vector3d b)
		{
			var cosAngle = a.Dot(b);
			var sqr = (1 - cosAngle) / (1 + cosAngle);
			sqr = MathUtil.Clamp(sqr, 0, double.MaxValue);
			return Math.Sqrt(sqr);
		}


	}
}
