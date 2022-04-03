using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RNumerics
{
	/// <summary>
	/// Fast Approximate SVD of 3x3 matrix that returns quaternions. 
	/// Implemented based on https://github.com/benjones/quatSVD/blob/master/quatSVD.hpp
	/// which was re-implemented from http://pages.cs.wisc.edu/~sifakis/project_pages/svd.html
	/// 
	/// By default, only does a small number of diagonalization iterations (4), which limits
	/// the accuracy of the solution. Results are still orthonormal but error when reconstructing
	/// matrix will be larger. This is fine for many applications. Can increase accuracy
	/// by increasing NumJacobiIterations parameter
	/// 
	/// Note: does *not* produce same quaternions as running SingularValueDecomposition on
	/// matrix and converting resulting U/V to quaternions. The numbers will be similar
	/// but the signs will be different
	/// 
	/// Useful properties:
	///   - quaternions are rotations, there are no mirrors like in normal SVD
	///   
	/// 
	/// TODO:
	///   - SymmetricMatrix3d currently a class, could make a struct (see comments)
	/// 
	/// </summary>
	public class FastQuaternionSVD
	{
		int _numJacobiIterations = 4;   // increase this to get higher accuracy
										// TODO: characterize...

		public Quaterniond U;
		public Quaterniond V;
		public Vector3d S;



		public FastQuaternionSVD() {
		}

		public FastQuaternionSVD(Matrix3d matrix, double epsilon = MathUtil.EPSILON, int jacobiIters = 4) {
			Solve(matrix, epsilon, jacobiIters);
		}


		SymmetricMatrix3d _aTA;
		double[] _aV;

		public void Solve(Matrix3d matrix, double epsilon = MathUtil.EPSILON, int jacobiIters = -1) {
			if (jacobiIters != -1) {
				_numJacobiIterations = jacobiIters;
			}

			if (_aTA == null) {
				_aTA = new SymmetricMatrix3d();
			}

			_aTA.SetATA(ref matrix);

			var v = JacobiDiagonalize(_aTA);

			if (_aV == null) {
				_aV = new double[9];
			}

			ComputeAV(ref matrix, ref v, _aV);

			var u = Vector4d.Zero;
			QRFactorize(_aV, ref v, epsilon, ref S, ref u);

			//u,v are quaternions in (s, x, y, z) order
			U = new Quaterniond(u[1], u[2], u[3], u[0]);
			V = new Quaterniond(v[1], v[2], v[3], v[0]);
		}



		/// <summary>
		/// Compute U * S * V^T, useful for error-checking
		/// </summary>
		public Matrix3d ReconstructMatrix() {
			var svdS = new Matrix3d(S[0], S[1], S[2]);
			return U.ToRotationMatrix() * svdS * V.Conjugate().ToRotationMatrix();
		}




		Vector4d JacobiDiagonalize(SymmetricMatrix3d ATA) {
			var V = new Vector4d(1, 0, 0, 0);

			for (var i = 0; i < _numJacobiIterations; ++i) {
				var givens = GivensAngles(ATA, 0, 1);
				ATA.QuatConjugate01(givens.x, givens.y);
				QuatTimesEqualCoordinateAxis(ref V, givens.x, givens.y, 2);

				givens = GivensAngles(ATA, 1, 2);
				ATA.QuatConjugate12(givens.x, givens.y);
				QuatTimesEqualCoordinateAxis(ref V, givens.x, givens.y, 0);

				givens = GivensAngles(ATA, 0, 2);
				ATA.QuatConjugate02(givens.x, givens.y);
				QuatTimesEqualCoordinateAxis(ref V, givens.x, givens.y, 1);
			}

			return V;
		}



		/// <summary>
		/// compute givens angles of B for (p,q). Only 
		/// ever called with p,q as [0,1], [0,2], or [1,2]
		/// </summary>
		Vector2d GivensAngles(SymmetricMatrix3d B, int p, int q) {
			double ch = 0, sh = 0;
			if (p == 0) {
				if (q == 1) {
					ch = B.entries[p] - B.entries[q];
					sh = 0.5 * B.entries[3];
				}
				else {
					ch = B.entries[q] - B.entries[p];
					sh = 0.5 * B.entries[4];
				}
			}
			else if (p == 1 /* && q == 2 */ ) {
				ch = B.entries[p] - B.entries[q];
				sh = 0.5 * B.entries[5];
			}

			// [TODO] can use fast reciprocal square root here...
			var omega = 1.0 / Math.Sqrt((ch * ch) + (sh * sh));
			ch *= omega;
			sh *= omega;

			var approxValid = (GAMMA * sh * sh) < (ch * ch);

			ch = approxValid ? ch : COS_BACKUP;
			sh = approxValid ? sh : SIN_BACKUP;

			return new Vector2d(ch, sh);
		}




		void ComputeAV(ref Matrix3d matrix, ref Vector4d V, double[] buf) {
			var qV = new Quaterniond(V[1], V[2], V[3], V[0]);
			var MV = qV.ToRotationMatrix();
			var AV = matrix * MV;
			AV.ToBuffer(buf);
		}



		void QRFactorize(double[] AV, ref Vector4d V, double eps, ref Vector3d S, ref Vector4d U) {
			PermuteColumns(AV, ref V);

			U = new Vector4d(1, 0, 0, 0);

			var givens10 = ComputeGivensQR(AV, eps, 1, 0);
			GivensQTB2(AV, givens10.x, givens10.y);
			QuatTimesEqualCoordinateAxis(ref U, givens10.x, givens10.y, 2);

			var givens20 = ComputeGivensQR(AV, eps, 2, 0);
			GivensQTB1(AV, givens20.x, -givens20.y);
			QuatTimesEqualCoordinateAxis(ref U, givens20.x, -givens20.y, 1);

			var givens21 = ComputeGivensQR(AV, eps, 2, 1);
			GivensQTB0(AV, givens21.x, givens21.y);
			QuatTimesEqualCoordinateAxis(ref U, givens21.x, givens21.y, 0);

			S = new Vector3d(AV[0], AV[4], AV[8]);
		}



		//returns the 2 components of the quaternion
		//such that Q^T * B has a 0 in element p, q
		Vector2d ComputeGivensQR(double[] B, double eps, int r, int c) {
			var app = B[4 * c];
			var apq = B[(3 * r) + c];

			var rho = Math.Sqrt((app * app) + (apq * apq));
			var sh = rho > eps ? apq : 0;
			var ch = Math.Abs(app) + Math.Max(rho, eps);

			if (app < 0) {
				(ch, sh) = (sh, ch);
			}

			// [TODO] can use fast reciprocal square root here...
			var omega = 1.0 / Math.Sqrt((ch * ch) + (sh * sh));
			ch *= omega;
			sh *= omega;

			return new Vector2d(ch, sh);
		}


		//Q is the rot matrix defined by quaternion (ch, . . . sh .. . ) where sh is coord i
		void GivensQTB2(double[] B, double ch, double sh) {
			//quat is (ch, 0, 0, sh), rotation around Z axis
			var c = (ch * ch) - (sh * sh);
			var s = 2 * sh * ch;
			//Q = [ c -s 0; s c 0; 0 0 1]

			var newb00 = (B[0] * c) + (B[3] * s);
			var newb01 = (B[1] * c) + (B[4] * s);
			var newb02 = (B[2] * c) + (B[5] * s);

			double newb10 = 0;//B[3]*c - B[0]*s; //should be 0... maybe don't compute?
			var newb11 = (B[4] * c) - (B[1] * s);
			var newb12 = (B[5] * c) - (B[2] * s);

			B[0] = newb00;
			B[1] = newb01;
			B[2] = newb02;

			B[3] = newb10;
			B[4] = newb11;
			B[5] = newb12;
		}

		//This will be called after givensQTB<2>, so we know that
		//B10 is 0... which actually doesn't matter since that row won't change
		void GivensQTB1(double[] B, double ch, double sh) {
			var c = (ch * ch) - (sh * sh);
			var s = 2 * sh * ch;
			//Q = [c 0 s; 0 1 0; -s 0 c];
			var newb00 = (B[0] * c) - (B[6] * s);
			var newb01 = (B[1] * c) - (B[7] * s);
			var newb02 = (B[2] * c) - (B[8] * s);

			double newb20 = 0;// B[0]*s + B[6]*c; //should be 0... maybe don't compute?
			var newb21 = (B[1] * s) + (B[7] * c);
			var newb22 = (B[2] * s) + (B[8] * c);

			B[0] = newb00;
			B[1] = newb01;
			B[2] = newb02;

			B[6] = newb20;
			B[7] = newb21;
			B[8] = newb22;
		}

		//B10 and B20 are 0, so don't bother filling in/computing them :)
		void GivensQTB0(double[] B, double ch, double sh) {
			var c = (ch * ch) - (sh * sh);
			var s = 2 * ch * sh;

			/* we may not need to compute the off diags since B should be diagonal
               after this step */
			var newb11 = (B[4] * c) + (B[7] * s);
			//double newb12 = B[5]*c + B[8]*s; 

			//double newb21 = B[7]*c - B[4]*s;
			var newb22 = (B[8] * c) - (B[5] * s);

			B[4] = newb11;
			//B[5] = newb12;

			//B[7] = newb21;
			B[8] = newb22;
		}



		void QuatTimesEqualCoordinateAxis(ref Vector4d lhs, double c, double s, int i) {
			//the quat we're multiplying by is (c, ? s ?)  where s is in slot i of the vector part,
			//and the other entries are 0
			var newS = (lhs.x * c) - (lhs[i + 1] * s);

			// s2*v1
			var newVals = new Vector3d(c * lhs.y, c * lhs.z, c * lhs.w);
			// s1*v2
			newVals[i] += lhs.x * s;
			// cross product
			newVals[(i + 1) % 3] += s * lhs[1 + ((i + 2) % 3)];
			newVals[(i + 2) % 3] -= s * lhs[1 + ((i + 1) % 3)];

			lhs.x = newS;
			lhs.y = newVals.x;
			lhs.z = newVals.y;
			lhs.w = newVals.z;
		}


		const double GAMMA = 3.0 + (2.0 * MathUtil.SQRT_TWO);
		const double SIN_BACKUP = 0.38268343236508973; //0.5 * Math.Sqrt(2.0 - MathUtil.SqrtTwo);
		const double COS_BACKUP = 0.92387953251128674; //0.5 * Math.Sqrt(2.0 + MathUtil.SqrtTwo);

		void PermuteColumns(double[] B, ref Vector4d V) {
			var magx = (B[0] * B[0]) + (B[3] * B[3]) + (B[6] * B[6]);
			var magy = (B[1] * B[1]) + (B[4] * B[4]) + (B[7] * B[7]);
			var magz = (B[2] * B[2]) + (B[5] * B[5]) + (B[8] * B[8]);

			if (magx < magy) {
				SwapColsNeg(B, 0, 1);
				QuatTimesEqualCoordinateAxis(ref V, MathUtil.SQRT_TWO_INV, MathUtil.SQRT_TWO_INV, 2);
				(magy, magx) = (magx, magy);
			}

			if (magx < magz) {
				SwapColsNeg(B, 0, 2);
				QuatTimesEqualCoordinateAxis(ref V, MathUtil.SQRT_TWO_INV, -MathUtil.SQRT_TWO_INV, 1);

				double unused;
				(magz, unused) = (magx, magz);
			}

			if (magy < magz) {
				SwapColsNeg(B, 1, 2);
				QuatTimesEqualCoordinateAxis(ref V, MathUtil.SQRT_TWO_INV, MathUtil.SQRT_TWO_INV, 0);
			}

		}



		void SwapColsNeg(double[] B, int i, int j) {
			var tmp = -B[i];
			B[i] = B[j];
			B[j] = tmp;

			tmp = -B[i + 3];
			B[i + 3] = B[j + 3];
			B[j + 3] = tmp;

			tmp = -B[i + 6];
			B[i + 6] = B[j + 6];
			B[j + 6] = tmp;
		}


	}



	/// <summary>
	/// Simple 3x3 symmetric-matrix class. The 6 values are stored
	/// as [diag_00, diag_11, diag_22, upper_01, upper_02, upper_12]
	/// 
	/// This is a helper class for FastQuaternionSVD, currently not public
	/// </summary>
	class SymmetricMatrix3d
	{
		// [TODO] could replace entries w/ 2 Vector3d, 
		// one for diag and one for off-diags. Then this can be a struct.
		public double[] entries = new double[6];

		public SymmetricMatrix3d() {
		}

		public void SetATA(ref Matrix3d A) {
			Vector3d c0 = A.Column(0), c1 = A.Column(1), c2 = A.Column(2);
			entries[0] = c0.LengthSquared;
			entries[1] = c1.LengthSquared;
			entries[2] = c2.LengthSquared;
			entries[3] = c0.Dot(c1);
			entries[4] = c0.Dot(c2);
			entries[5] = c1.Dot(c2);
		}

		/*
         * These functions compute Q^T * S * Q
         * where Q is represented as the quaterion with c as the scalar and s in the slot that's not p or q
         */
		public void QuatConjugate01(double c, double s) {
			//rotatoin around z axis
			var realC = (c * c) - (s * s);
			var realS = 2 * s * c;

			var cs = realS * realC;
			var cc = realC * realC;
			var ss = realS * realS;

			var newS11 = (cc * entries[0]) + (2 * cs * entries[3]) + (ss * entries[1]);
			var newS22 = (ss * entries[0]) - (2 * cs * entries[3]) + (cc * entries[1]);
			var newS12 = (entries[3] * (cc - ss)) + (cs * (entries[1] - entries[0]));
			var newS13 = (realC * entries[4]) + (realS * entries[5]);
			var newS23 = (realC * entries[5]) - (realS * entries[4]);

			entries[0] = newS11;
			entries[1] = newS22;
			entries[3] = newS12;
			entries[4] = newS13;
			entries[5] = newS23;
		}


		public void QuatConjugate02(double c, double s) {
			//rotation around y axis
			//quat looks like (ch, 0, sh, 0)
			var realC = (c * c) - (s * s);
			var realS = 2 * s * c;

			var cs = realS * realC;
			var cc = realC * realC;
			var ss = realS * realS;

			var newS11 = (cc * entries[0]) - (2 * cs * entries[4]) + (ss * entries[2]);
			var newS33 = (ss * entries[0]) + (2 * cs * entries[4]) + (cc * entries[2]);
			var newS12 = (realC * entries[3]) - (realS * entries[5]);
			var newS13 = (cs * (entries[0] - entries[2])) + ((cc - ss) * entries[4]);
			var newS23 = (realS * entries[3]) + (realC * entries[5]);

			entries[0] = newS11;
			entries[2] = newS33;
			entries[3] = newS12;
			entries[4] = newS13;
			entries[5] = newS23;
		}



		public void QuatConjugate12(double c, double s) {
			//rotation around x axis
			//quat looks like (ch, sh, 0, 0)
			var realC = (c * c) - (s * s);
			var realS = 2 * s * c;

			var cs = realS * realC;
			var cc = realC * realC;
			var ss = realS * realS;
			var newS22 = (cc * entries[1]) + (2 * cs * entries[5]) + (ss * entries[2]);
			var newS33 = (ss * entries[1]) - (2 * cs * entries[5]) + (cc * entries[2]);
			var newS12 = (realC * entries[3]) + (realS * entries[4]);
			var newS13 = (-realS * entries[3]) + (realC * entries[4]);
			var newS23 = ((cc - ss) * entries[5]) + (cs * (entries[2] - entries[1]));

			entries[1] = newS22;
			entries[2] = newS33;
			entries[3] = newS12;
			entries[4] = newS13;
			entries[5] = newS23;
		}



	}

}
