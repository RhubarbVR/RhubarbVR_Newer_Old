using System;

namespace RNumerics
{
	// ported from WildMagic5 BSplineBasis
	public sealed class BSplineBasis
	{
		// Defaultructor.  The number of control points is n+1 and the
		// indices i for the control points satisfy 0 <= i <= n.  The degree of
		// the curve is d.  The knot array has n+d+2 elements.  Whether uniform
		// or nonuniform knots, it is required that
		//   knot[i] = 0, 0 <= i <= d
		//   knot[i] = 1, n+1 <= i <= n+d+1
		// BSplineBasis enforces these conditions by not exposing SetKnot for the
		// relevant values of i.
		//
		private BSplineBasis() {
			// [RMS] removed Create(), so default constructor is useless...
		}

		// Open uniform or periodic uniform.  The knot array is internally
		// generated with equally spaced elements.  It is required that
		//   knot[i] = (i-d)/(n+1-d), d+1 <= i <= n
		// BSplineBasis enforces these conditions by not exposing SetKnot for the
		// relevant values of i.  GetKnot(j) will return knot[i] for i = j+d+1.
		public BSplineBasis(in int numCtrlPoints, in int degree, in bool open) {
			// code from c++ Create(int numCtrlPoints, int degree, bool open);
			_mUniform = true;

			int i, numKnots = Initialize(numCtrlPoints, degree, open);
			var factor = 1.0 / (_mNumCtrlPoints - _mDegree);
			if (_mOpen) {
				for (i = 0; i <= _mDegree; ++i) {
					_mKnot[i] = 0;
				}

				for (/**/; i < _mNumCtrlPoints; ++i) {
					_mKnot[i] = (i - _mDegree) * factor;
				}

				for (/**/; i < numKnots; ++i) {
					_mKnot[i] = 1;
				}
			}
			else {
				for (i = 0; i < numKnots; ++i) {
					_mKnot[i] = (i - _mDegree) * factor;
				}
			}
		}


		// Open nonuniform.  
		// if bIsInteriorKnots, the knots array must have n-d-1 nondecreasing
		//   elements in the interval [0,1].  The values are
		//     knot[i] = interiorKnot[j]
		//   with 0 <= j < n-d-1 and i = j+d+1, so d+1 <= i < n.  
		//
		// if bIsInteriorKnots = false, the knot vector is copied directly, and must have
		//   n+d+1 elements
		//
		// An internal copy of knots[] is made, so to dynamically change knots you 
		// must use the SetKnot(j,*) function.
		public BSplineBasis(in int numCtrlPoints, in int degree, in double[] knots, in bool bIsInteriorKnots) {
			//code from c++ Create(int numCtrlPoints, int degree, double* interiorKnot);
			_mUniform = false;
			int i, numKnots = Initialize(numCtrlPoints, degree, true);

			if (bIsInteriorKnots) {
				if (knots.Length != _mNumCtrlPoints - _mDegree - 1) {
					throw new Exception("BSplineBasis nonuniform constructor: invalid interior knot vector");
				}

				for (i = 0; i <= _mDegree; ++i) {
					_mKnot[i] = 0;
				}

				for (var j = 0; i < _mNumCtrlPoints; ++i, ++j) {
					_mKnot[i] = knots[j];
				}

				for (/**/; i < numKnots; ++i) {
					_mKnot[i] = 1;
				}
			}
			else {
				if (_mKnot.Length != knots.Length) {
					throw new Exception("BSplineBasis nonuniform constructor: invalid knot vector");
				}

				Array.Copy(knots, _mKnot, knots.Length);
			}

		}


		public BSplineBasis Clone() {
			var b2 = new BSplineBasis {
				_mNumCtrlPoints = _mNumCtrlPoints,
				_mDegree = _mDegree,
				_mKnot = (double[])_mKnot.Clone(),
				_mOpen = _mOpen,
				_mUniform = _mUniform
			};
			return b2;
		}



		public int GetNumCtrlPoints() {
			return _mNumCtrlPoints;
		}
		public int GetDegree() {
			return _mDegree;
		}
		public bool IsOpen() {
			return _mOpen;
		}
		public bool IsUniform() {
			return _mUniform;
		}


		public int KnotCount => _mNumCtrlPoints + _mDegree + 1;
		public int InteriorKnotCount => _mNumCtrlPoints - _mDegree - 1;

		// For a nonuniform spline, the knot[i] are modified by SetInteriorKnot(j,value)
		// for j = i+d+1.  That is, you specify j with 0 <= j <= n-d-1, i = j+d+1,
		// and knot[i] = value.  SetInteriorKnot(j,value) does nothing for indices outside
		// the j-range or for uniform splines.  
		public void SetInteriorKnot(in int j, in double value) {
			if (!_mUniform) {
				// Access only allowed to elements d+1 <= i <= n.
				var i = j + _mDegree + 1;
				_mKnot[i] = _mDegree + 1 <= i && i <= _mNumCtrlPoints ? value : throw new Exception("BSplineBasis.SetKnot: index out of range: " + j);
			}
			else {
				throw new Exception("BSplineBasis.SetKnot: knots cannot be set for uniform splines");
			}
		}

		public double GetInteriorKnot(in int j) {
			// Access only allowed to elements d+1 <= i <= n.
			var i = j + _mDegree + 1;
			if (_mDegree + 1 <= i && i <= _mNumCtrlPoints) {
				return _mKnot[i];
			}
			//assertion(false, "Knot index out of range.\n");
			throw new Exception("BSplineBasis.GetKnot: index out of range: " + j);
			//return double.MaxValue;
		}

		// [RMS] direct access to all knots. Not sure why this was not allowed in
		//   original code - are there assumptions that some knots are 0/1 ???
		public void SetKnot(in int j, in double value) {
			_mKnot[j] = value;
		}
		public double GetKnot(in int j) {
			return _mKnot[j];
		}

		// Access basis functions and their derivatives.
		public double GetD0(in int i) {
			return _mBD0[_mDegree, i];
		}
		public double GetD1(in int i) {
			return _mBD1[_mDegree, i];
		}
		public double GetD2(in int i) {
			return _mBD2[_mDegree, i];
		}
		public double GetD3(in int i) {
			return _mBD3[_mDegree, i];
		}

		// Evaluate basis functions and their derivatives.
		public void Compute(double t, in int order, ref int minIndex, ref int maxIndex) {
			//assertion(order <= 3, "Only derivatives to third order supported\n");
			if (order > 3) {
				throw new Exception("BSplineBasis.Compute: cannot compute order " + order);
			}

			if (order >= 1) {
				_mBD1 ??= Allocate();

				if (order >= 2) {
					_mBD2 ??= Allocate();

					if (order >= 3) {
						_mBD3 ??= Allocate();
					}
				}
			}

			var i = GetKey(ref t);
			_mBD0[0, i] = (double)1;

			if (order >= 1) {
				_mBD1[0, i] = (double)0;
				if (order >= 2) {
					_mBD2[0, i] = (double)0;
					if (order >= 3) {
						_mBD3[0, i] = (double)0;
					}
				}
			}

			double n0 = t - _mKnot[i], n1 = _mKnot[i + 1] - t;
			double invD0, invD1;
			int j;
			for (j = 1; j <= _mDegree; j++) {
				invD0 = 1.0 / (_mKnot[i + j] - _mKnot[i]);
				invD1 = 1.0 / (_mKnot[i + 1] - _mKnot[i - j + 1]);

				// [RMS] convention is 0/0 = 0. invD0/D1 will be Infinity in these
				// cases, so we set explicitly to 0
				if (_mKnot[i + j] == _mKnot[i]) {
					invD0 = 0;
				}

				if (_mKnot[i + 1] == _mKnot[i - j + 1]) {
					invD1 = 0;
				}

				_mBD0[j, i] = n0 * _mBD0[j - 1, i] * invD0;
				_mBD0[j, i - j] = n1 * _mBD0[j - 1, i - j + 1] * invD1;

				if (order >= 1) {
					_mBD1[j, i] = ((n0 * _mBD1[j - 1, i]) + _mBD0[j - 1, i]) * invD0;
					_mBD1[j, i - j] = ((n1 * _mBD1[j - 1, i - j + 1]) - _mBD0[j - 1, i - j + 1]) * invD1;

					if (order >= 2) {
						_mBD2[j, i] = ((n0 * _mBD2[j - 1, i]) + (((double)2) * _mBD1[j - 1, i])) * invD0;
						_mBD2[j, i - j] = ((n1 * _mBD2[j - 1, i - j + 1]) -
							(((double)2) * _mBD1[j - 1, i - j + 1])) * invD1;

						if (order >= 3) {
							_mBD3[j, i] = ((n0 * _mBD3[j - 1, i]) +
								(((double)3) * _mBD2[j - 1, i])) * invD0;
							_mBD3[j, i - j] = ((n1 * _mBD3[j - 1, i - j + 1]) -
								(((double)3) * _mBD2[j - 1, i - j + 1])) * invD1;
						}
					}
				}
			}

			for (j = 2; j <= _mDegree; ++j) {
				for (var k = i - j + 1; k < i; ++k) {
					n0 = t - _mKnot[k];
					n1 = _mKnot[k + j + 1] - t;
					invD0 = 1.0 / (_mKnot[k + j] - _mKnot[k]);
					invD1 = 1.0 / (_mKnot[k + j + 1] - _mKnot[k + 1]);

					// [RMS] convention is 0/0 = 0. invD0/D1 will be Infinity in these
					// cases, so we set explicitly to 0
					if (_mKnot[k + j] == _mKnot[k]) {
						invD0 = 0;
					}

					if (_mKnot[k + j + 1] == _mKnot[k + 1]) {
						invD1 = 0;
					}

					_mBD0[j, k] = (n0 * _mBD0[j - 1, k] * invD0) + (n1 * _mBD0[j - 1, k + 1] * invD1);

					if (order >= 1) {
						_mBD1[j, k] = (((n0 * _mBD1[j - 1, k]) + _mBD0[j - 1, k]) * invD0) +
							(((n1 * _mBD1[j - 1, k + 1]) - _mBD0[j - 1, k + 1]) * invD1);

						if (order >= 2) {
							_mBD2[j, k] = (((n0 * _mBD2[j - 1, k]) +
								(((double)2) * _mBD1[j - 1, k])) * invD0) +
								(((n1 * _mBD2[j - 1, k + 1]) - (((double)2) * _mBD1[j - 1, k + 1])) * invD1);

							if (order >= 3) {
								_mBD3[j, k] = (((n0 * _mBD3[j - 1, k]) +
									(((double)3) * _mBD2[j - 1, k])) * invD0) +
									(((n1 * _mBD3[j - 1, k + 1]) - (((double)3) *
									_mBD2[j - 1, k + 1])) * invD1);
							}
						}
					}
				}
			}

			minIndex = i - _mDegree;
			maxIndex = i;
		}


		private int Initialize(in int numCtrlPoints, in int degree, in bool open) {
			if (numCtrlPoints < 2) {
				throw new Exception("BSplineBasis.Initialize: only received " + numCtrlPoints + " control points!");
			}

			if (degree < 1 || degree > numCtrlPoints - 1) {
				throw new Exception("BSplineBasis.Initialize: invalid degree " + degree);
			}
			//assertion(numCtrlPoints >= 2, "Invalid input\n");
			//assertion(1 <= degree && degree <= numCtrlPoints - 1, "Invalid input\n");

			_mNumCtrlPoints = numCtrlPoints;
			_mDegree = degree;
			_mOpen = open;

			var numKnots = _mNumCtrlPoints + _mDegree + 1;
			_mKnot = new double[numKnots];

			_mBD0 = Allocate();
			_mBD1 = null;
			_mBD2 = null;
			_mBD3 = null;

			return numKnots;
		}

		private double[,] Allocate() {
			var numRows = _mDegree + 1;
			var numCols = _mNumCtrlPoints + _mDegree;
			var data = new double[numRows, numCols];
			for (var i = 0; i < numRows; ++i) {
				for (var j = 0; j < numCols; ++j) {
					data[i, j] = 0;
				}
			}

			return data;
		}

		// [RMS] not necessary
		//protected void Deallocate(double[,] data);


		// Determine knot index i for which knot[i] <= rfTime < knot[i+1].
		private int GetKey(ref double t) {
			if (_mOpen) {
				// Open splines clamp to [0,1].
				if (t <= 0) {
					t = 0;
					return _mDegree;
				}
				else if (t >= 1) {
					t = 1;
					return _mNumCtrlPoints - 1;
				}
			}
			else {
				// Periodic splines wrap to [0,1).
				if (t is < 0 or >= 1) {
					t -= Math.Floor(t);
				}
			}


			int i;

			if (_mUniform) {
				i = _mDegree + (int)((_mNumCtrlPoints - _mDegree) * t);
			}
			else {
				for (i = _mDegree + 1; i <= _mNumCtrlPoints; ++i) {
					if (t < _mKnot[i]) {
						break;
					}
				}
				--i;
			}

			return i;
		}


		//
		// data members
		//

		private int _mNumCtrlPoints;   // n+1
		private int _mDegree;          // d
		private double[] _mKnot;          // knot[n+d+2]
		private bool _mOpen, _mUniform;

		// Storage for the basis functions and their derivatives first three
		// derivatives.  The basis array is always allocated by theructor
		// calls.  A derivative basis array is allocated on the first call to a
		// derivative member function.
		private double[,] _mBD0;          // bd0[d+1,n+d+1]
		private double[,] _mBD1;  // bd1[d+1,n+d+1]
		private double[,] _mBD2;  // bd2[d+1,n+d+1]
		private double[,] _mBD3;  // bd3[d+1,n+d+1]
	}
}
