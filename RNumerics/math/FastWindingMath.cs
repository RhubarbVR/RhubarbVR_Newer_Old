using System;
using System.Collections.Generic;

namespace RNumerics
{


	/// <summary>
	/// Formulas for triangle winding number approximation
	/// </summary>
	public static class FastTriWinding
	{
	

		/// <summary>
		/// Evaluate first-order FWN approximation at point q, relative to center c
		/// </summary>
		public static double EvaluateOrder1Approx(ref Vector3d center, ref Vector3d order1Coeff, ref Vector3d q) {
			var dpq = center - q;
			var len = dpq.Length;

			return 1.0 / MathUtil.FOUR_PI * order1Coeff.Dot(dpq / (len * len * len));
		}


		/// <summary>
		/// Evaluate second-order FWN approximation at point q, relative to center c
		/// </summary>
		public static double EvaluateOrder2Approx(ref Vector3d center, ref Vector3d order1Coeff, ref Matrix3d order2Coeff, ref Vector3d q) {
			var dpq = center - q;
			var len = dpq.Length;
			var len3 = len * len * len;
			var fourPi_len3 = 1.0 / (MathUtil.FOUR_PI * len3);

			var order1 = fourPi_len3 * order1Coeff.Dot(dpq);

			// second-order hessian \grad^2(G)
			var c = -3.0 / (MathUtil.FOUR_PI * len3 * len * len);

			// expanded-out version below avoids extra constructors
			//Matrix3d xqxq = new Matrix3d(ref dpq, ref dpq);
			//Matrix3d hessian = new Matrix3d(fourPi_len3, fourPi_len3, fourPi_len3) - c * xqxq;
			var hessian = new Matrix3d(
				fourPi_len3 + (c * dpq.x * dpq.x), c * dpq.x * dpq.y, c * dpq.x * dpq.z,
				c * dpq.y * dpq.x, fourPi_len3 + (c * dpq.y * dpq.y), c * dpq.y * dpq.z,
				c * dpq.z * dpq.x, c * dpq.z * dpq.y, fourPi_len3 + (c * dpq.z * dpq.z));

			var order2 = order2Coeff.InnerProduct( hessian);

			return order1 + order2;
		}




		// triangle-winding-number first-order approximation. 
		// t is triangle, p is 'center' of cluster of dipoles, q is evaluation point
		// (This is really just for testing)
		public static double Order1Approx(ref Vector3d p, ref Vector3d xn, ref double xA, ref Vector3d q) {
			var at0 = xA * xn;

			var dpq = p - q;
			var len = dpq.Length;
			var len3 = len * len * len;

			return 1.0 / MathUtil.FOUR_PI * at0.Dot(dpq / (len * len * len));
		}


		// triangle-winding-number second-order approximation
		// t is triangle, p is 'center' of cluster of dipoles, q is evaluation point
		// (This is really just for testing)
		public static double Order2Approx(ref Triangle3d t, ref Vector3d p, ref Vector3d xn, ref double xA, ref Vector3d q) {
			var dpq = p - q;

			var len = dpq.Length;
			var len3 = len * len * len;

			// first-order approximation - integrated_normal_area * \grad(G)
			var order1 = xA / MathUtil.FOUR_PI * xn.Dot(dpq / len3);

			// second-order hessian \grad^2(G)
			var xqxq = new Matrix3d( dpq,  dpq);
			xqxq *= 3.0 / (MathUtil.FOUR_PI * len3 * len * len);
			var diag = 1 / (MathUtil.FOUR_PI * len3);
			var hessian = new Matrix3d(diag, diag, diag) - xqxq;

			// second-order LHS - integrated second-order area matrix (formula 26)
			var centroid = new Vector3d(
				(t.V0.x + t.V1.x + t.V2.x) / 3.0, (t.V0.y + t.V1.y + t.V2.y) / 3.0, (t.V0.z + t.V1.z + t.V2.z) / 3.0);
			var dcp = centroid - p;
			var o2_lhs = new Matrix3d( dcp,  xn);
			var order2 = xA * o2_lhs.InnerProduct( hessian);

			return order1 + order2;
		}
	}




	/// <summary>
	/// Formulas for point-set winding number approximation
	/// </summary>
	public static class FastPointWinding
	{
		/// <summary>
		/// precompute constant coefficients of point winding number approximation
		/// pointAreas must be provided, and pointSet must have vertex normals!
		/// p: 'center' of expansion for points (area-weighted point avg)
		/// r: max distance from p to points
		/// order1: first-order vector coeff
		/// order2: second-order matrix coeff
		/// </summary>
		public static void ComputeCoeffs(
			in IPointSet pointSet, in IEnumerable<int> points, in double[] pointAreas,
			ref Vector3d p, ref double r,
			ref Vector3d order1, ref Matrix3d order2) {
			if (pointSet.HasVertexNormals == false) {
				throw new Exception("FastPointWinding.ComputeCoeffs: point set does not have normals!");
			}

			p = Vector3d.Zero;
			order1 = Vector3d.Zero;
			order2 = Matrix3d.Zero;
			r = 0;

			// compute area-weighted centroid of points, we use this as the expansion point
			double sum_area = 0;
			foreach (var vid in points) {
				sum_area += pointAreas[vid];
				p += pointAreas[vid] * pointSet.GetVertex(vid);
			}
			p /= sum_area;

			// compute first and second-order coefficients of FWN taylor expansion, as well as
			// 'radius' value r, which is max dist from any tri vertex to p  
			foreach (var vid in points) {
				var p_i = pointSet.GetVertex(vid);
				Vector3d n_i = pointSet.GetVertexNormal(vid);
				var a_i = pointAreas[vid];

				order1 += a_i * n_i;

				var dcp = p_i - p;
				order2 += a_i * new Matrix3d( dcp,  n_i);

				// this is just for return value...
				r = Math.Max(r, p_i.Distance(p));
			}
		}


		/// <summary>
		/// Evaluate first-order FWN approximation at point q, relative to center c
		/// </summary>
		public static double EvaluateOrder1Approx(ref Vector3d center, ref Vector3d order1Coeff, ref Vector3d q) {
			var dpq = center - q;
			var len = dpq.Length;

			return 1.0 / MathUtil.FOUR_PI * order1Coeff.Dot(dpq / (len * len * len));
		}



		/// <summary>
		/// Evaluate second-order FWN approximation at point q, relative to center c
		/// </summary>
		public static double EvaluateOrder2Approx(ref Vector3d center, ref Vector3d order1Coeff, ref Matrix3d order2Coeff, ref Vector3d q) {
			var dpq = center - q;
			var len = dpq.Length;
			var len3 = len * len * len;
			var fourPi_len3 = 1.0 / (MathUtil.FOUR_PI * len3);

			var order1 = fourPi_len3 * order1Coeff.Dot(dpq);

			// second-order hessian \grad^2(G)
			var c = -3.0 / (MathUtil.FOUR_PI * len3 * len * len);

			// expanded-out version below avoids extra constructors
			//Matrix3d xqxq = new Matrix3d(ref dpq, ref dpq);
			//Matrix3d hessian = new Matrix3d(fourPi_len3, fourPi_len3, fourPi_len3) - c * xqxq;
			var hessian = new Matrix3d(
				fourPi_len3 + (c * dpq.x * dpq.x), c * dpq.x * dpq.y, c * dpq.x * dpq.z,
				c * dpq.y * dpq.x, fourPi_len3 + (c * dpq.y * dpq.y), c * dpq.y * dpq.z,
				c * dpq.z * dpq.x, c * dpq.z * dpq.y, fourPi_len3 + (c * dpq.z * dpq.z));

			var order2 = order2Coeff.InnerProduct( hessian);

			return order1 + order2;
		}



		public static double ExactEval(ref Vector3d x, ref Vector3d xn, in double xA, ref Vector3d q) {
			var dv = x - q;
			var len = dv.Length;
			return xA / MathUtil.FOUR_PI * xn.Dot(dv / (len * len * len));
		}

		// point-winding-number first-order approximation. 
		// x is dipole point, p is 'center' of cluster of dipoles, q is evaluation point
		public static double Order1Approx(ref Vector3d p, ref Vector3d xn, in double xA, ref Vector3d q) {
			var dpq = p - q;
			var len = dpq.Length;
			var len3 = len * len * len;

			return xA / MathUtil.FOUR_PI * xn.Dot(dpq / (len * len * len));
		}


		// point-winding-number second-order approximation
		// x is dipole point, p is 'center' of cluster of dipoles, q is evaluation point
		public static double Order2Approx(ref Vector3d x, ref Vector3d p, ref Vector3d xn, in double xA, ref Vector3d q) {
			var dpq = p - q;
			var dxp = x - p;

			var len = dpq.Length;
			var len3 = len * len * len;

			// first-order approximation - area*normal*\grad(G)
			var order1 = xA / MathUtil.FOUR_PI * xn.Dot(dpq / len3);

			// second-order hessian \grad^2(G)
			var xqxq = new Matrix3d( dpq,  dpq);
			xqxq *= 3.0 / (MathUtil.FOUR_PI * len3 * len * len);
			var diag = 1 / (MathUtil.FOUR_PI * len3);
			var hessian = new Matrix3d(diag, diag, diag) - xqxq;

			// second-order LHS area * \outer(x-p, normal)
			var o2_lhs = new Matrix3d( dxp,  xn);
			var order2 = xA * o2_lhs.InnerProduct( hessian);

			return order1 + order2;
		}
	}


}
