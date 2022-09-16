using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// 2D Bezier curve of arbitrary degree
	/// Ported from WildMagic5 Wm5BezierCurve2
	/// </summary>
	public sealed class BezierCurve2 : BaseCurve2, IParametricCurve2d
	{
		int _mNumCtrlPoints;
		Vector2d[] _mDer1CtrlPoint;
		Vector2d[] _mDer2CtrlPoint;
		Vector2d[] _mDer3CtrlPoint;
		DenseMatrix _mChoose;


		public int Degree { get; private set; }
		public Vector2d[] ControlPoints { get; private set; }


		public BezierCurve2(in int degree, in Vector2d[] ctrlPoint, in bool bTakeOwnership = false) : base(0, 1) {
			if (degree < 2) {
				throw new Exception("BezierCurve2() The degree must be three or larger\n");
			}

			int i, j;

			Degree = degree;
			_mNumCtrlPoints = Degree + 1;
			if (bTakeOwnership) {
				ControlPoints = ctrlPoint;
			}
			else {
				ControlPoints = new Vector2d[ctrlPoint.Length];
				Array.Copy(ctrlPoint, ControlPoints, ctrlPoint.Length);
			}

			// Compute first-order differences.
			_mDer1CtrlPoint = new Vector2d[_mNumCtrlPoints - 1];
			for (i = 0; i < _mNumCtrlPoints - 1; ++i) {
				_mDer1CtrlPoint[i] = ControlPoints[i + 1] - ControlPoints[i];
			}

			// Compute second-order differences.
			_mDer2CtrlPoint = new Vector2d[_mNumCtrlPoints - 2];
			for (i = 0; i < _mNumCtrlPoints - 2; ++i) {
				_mDer2CtrlPoint[i] = _mDer1CtrlPoint[i + 1] - _mDer1CtrlPoint[i];
			}

			// Compute third-order differences.
			if (degree >= 3) {
				_mDer3CtrlPoint = new Vector2d[_mNumCtrlPoints - 3];
				for (i = 0; i < _mNumCtrlPoints - 3; ++i) {
					_mDer3CtrlPoint[i] = _mDer2CtrlPoint[i + 1] - _mDer2CtrlPoint[i];
				}
			}
			else {
				_mDer3CtrlPoint = null;
			}

			// Compute combinatorial values Choose(N,K), store in mChoose[N,K].
			// The values mChoose[r,c] are invalid for r < c (use only the
			// entries for r >= c).
			_mChoose = new DenseMatrix(_mNumCtrlPoints, _mNumCtrlPoints);

			_mChoose[0, 0] = 1.0;
			_mChoose[1, 0] = 1.0;
			_mChoose[1, 1] = 1.0;
			for (i = 2; i <= Degree; ++i) {
				_mChoose[i, 0] = 1.0;
				_mChoose[i, i] = 1.0;
				for (j = 1; j < i; ++j) {
					_mChoose[i, j] = _mChoose[i - 1, j - 1] + _mChoose[i - 1, j];
				}
			}
		}


		// used in Clone()
		private BezierCurve2() : base(0, 1) {
		}


		public override Vector2d GetPosition(in double t) {
			var oneMinusT = 1 - t;
			var powT = t;
			var result = oneMinusT * ControlPoints[0];

			for (var i = 1; i < Degree; ++i) {
				var coeff = _mChoose[Degree, i] * powT;
				result = (result + (ControlPoints[i] * coeff)) * oneMinusT;
				powT *= t;
			}

			result += ControlPoints[Degree] * powT;

			return result;
		}


		public override Vector2d GetFirstDerivative(in double t) {
			var oneMinusT = 1 - t;
			var powT = t;
			var result = oneMinusT * _mDer1CtrlPoint[0];

			var degreeM1 = Degree - 1;
			for (var i = 1; i < degreeM1; ++i) {
				var coeff = _mChoose[degreeM1, i] * powT;
				result = (result + (_mDer1CtrlPoint[i] * coeff)) * oneMinusT;
				powT *= t;
			}

			result += _mDer1CtrlPoint[degreeM1] * powT;
			result *= (double)Degree;

			return result;
		}


		public override Vector2d GetSecondDerivative(in double t) {
			var oneMinusT = 1 - t;
			var powT = t;
			var result = oneMinusT * _mDer2CtrlPoint[0];

			var degreeM2 = Degree - 2;
			for (var i = 1; i < degreeM2; ++i) {
				var coeff = _mChoose[degreeM2, i] * powT;
				result = (result + (_mDer2CtrlPoint[i] * coeff)) * oneMinusT;
				powT *= t;
			}

			result += _mDer2CtrlPoint[degreeM2] * powT;
			result *= (double)(Degree * (Degree - 1));

			return result;
		}


		public override Vector2d GetThirdDerivative(in double t) {
			if (Degree < 3) {
				return Vector2d.Zero;
			}

			var oneMinusT = 1 - t;
			var powT = t;
			var result = oneMinusT * _mDer3CtrlPoint[0];

			var degreeM3 = Degree - 3;
			for (var i = 1; i < degreeM3; ++i) {
				var coeff = _mChoose[degreeM3, i] * powT;
				result = (result + (_mDer3CtrlPoint[i] * coeff)) * oneMinusT;
				powT *= t;
			}

			result += _mDer3CtrlPoint[degreeM3] * powT;
			result *= (double)(Degree * (Degree - 1) * (Degree - 2));

			return result;
		}



		/*
         * IParametricCurve2d implementation
         */

		// TODO: could support closed bezier?
		public bool IsClosed => false;

		// can call SampleT in range [0,ParamLength]
		public double ParamLength => mTMax - mTMin;
		public Vector2d SampleT(in double t) {
			return GetPosition(t);
		}

		public Vector2d TangentT(in double t) {
			return GetFirstDerivative(t).Normalized;
		}

		public bool HasArcLength => true;
		public double ArcLength => GetTotalLength();
		public Vector2d SampleArcLength(in double a) {
			var t = GetTime(a);
			return GetPosition(t);
		}

		public void Reverse() {
			throw new NotSupportedException("NURBSCurve2.Reverse: how to reverse?!?");
		}

		public IParametricCurve2d Clone() {
			var c2 = new BezierCurve2 {
				Degree = Degree,
				_mNumCtrlPoints = _mNumCtrlPoints,

				ControlPoints = (Vector2d[])ControlPoints.Clone(),
				_mDer1CtrlPoint = (Vector2d[])_mDer1CtrlPoint.Clone(),
				_mDer2CtrlPoint = (Vector2d[])_mDer2CtrlPoint.Clone(),
				_mDer3CtrlPoint = (Vector2d[])_mDer3CtrlPoint.Clone(),
				_mChoose = new DenseMatrix(_mChoose)
			};
			return c2;
		}


		public bool IsTransformable => true;
		public void Transform(in ITransform2 xform) {
			for (var k = 0; k < ControlPoints.Length; ++k) {
				ControlPoints[k] = xform.TransformP(ControlPoints[k]);
			}

			// update derivatives
			for (var i = 0; i < _mNumCtrlPoints - 1; ++i) {
				_mDer1CtrlPoint[i] = ControlPoints[i + 1] - ControlPoints[i];
			}

			for (var i = 0; i < _mNumCtrlPoints - 2; ++i) {
				_mDer2CtrlPoint[i] = _mDer1CtrlPoint[i + 1] - _mDer1CtrlPoint[i];
			}

			if (Degree >= 3) {
				for (var i = 0; i < _mNumCtrlPoints - 3; ++i) {
					_mDer3CtrlPoint[i] = _mDer2CtrlPoint[i + 1] - _mDer2CtrlPoint[i];
				}
			}
		}


	}
}
