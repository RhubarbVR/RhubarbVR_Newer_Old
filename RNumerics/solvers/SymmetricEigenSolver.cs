using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RNumerics
{
	// port of WildMagic5 Wm5SymmetricEigensolverGTE class (which is a back-port
	// of GTEngine Symmetric Eigensolver class) see geometrictools.com

	// The SymmetricEigensolver class is an implementation of Algorithm 8.2.3
	// (Symmetric QR Algorithm) described in "Matrix Computations, 2nd edition"
	// by G. H. Golub and C. F. Van Loan, The Johns Hopkins University Press,
	// Baltimore MD, Fourth Printing 1993.  Algorithm 8.2.1 (Householder
	// Tridiagonalization) is used to reduce matrix A to tridiagonal T.
	// Algorithm 8.2.2 (Implicit Symmetric QR Step with Wilkinson Shift) is
	// used for the iterative reduction from tridiagonal to diagonal.  If A is
	// the original matrix, D is the diagonal matrix of eigenvalues, and Q is
	// the orthogonal matrix of eigenvectors, then theoretically Q^T*A*Q = D.
	// Numerically, we have errors E = Q^T*A*Q - D.  Algorithm 8.2.3 mentions
	// that one expects |E| is approximately u*|A|, where |M| denotes the
	// Frobenius norm of M and where u is the unit roundoff for the
	// floating-point arithmetic: 2^{-23} for 'float', which is FLT_EPSILON
	// = 1.192092896e-7f, and 2^{-52} for'double', which is DBL_EPSILON
	// = 2.2204460492503131e-16.
	//
	// The condition |a(i,i+1)| <= epsilon*(|a(i,i) + a(i+1,i+1)|) used to
	// determine when the reduction decouples to smaller problems is implemented
	// as:  sum = |a(i,i)| + |a(i+1,i+1)|; sum + |a(i,i+1)| == sum.  The idea is
	// that the superdiagonal term is small relative to its diagonal neighbors,
	// and so it is effectively zero.  The unit tests have shown that this
	// interpretation of decoupling is effective.
	//
	// The authors suggest that once you have the tridiagonal matrix, a practical
	// implementation will store the diagonal and superdiagonal entries in linear
	// arrays, ignoring the theoretically zero values not in the 3-band.  This is
	// good for cache coherence.  The authors also suggest storing the Householder
	// vectors in the lower-triangular portion of the matrix to save memory.  The
	// implementation uses both suggestions.

	public sealed class SymmetricEigenSolver
	{
		// The solver processes NxN symmetric matrices, where N > 1 ('size' is N)
		// and the matrix is stored in row-major order.  The maximum number of
		// iterations ('maxIterations') must be specified for the reduction of a
		// tridiagonal matrix to a diagonal matrix.  The goal is to compute
		// NxN orthogonal Q and NxN diagonal D for which Q^T*A*Q = D.
		public SymmetricEigenSolver(in int size,in int maxIterations) {
			_mSize = _mMaxIterations = 0;
			_mIsRotation = -1;
			if (size > 1 && maxIterations > 0) {
				_mSize = size;
				_mMaxIterations = maxIterations;
				_mMatrix = new double[size * size];
				_mDiagonal = new double[size];
				_mSuperdiagonal = new double[size - 1];
				_mGivens = new List<GivensRotation>(maxIterations * (size - 1));
				_mPermutation = new int[size];
				_mVisited = new int[size];
				_mPVector = new double[size];
				_mVVector = new double[size];
				_mWVector = new double[size];
			}
		}

		// A copy of the NxN symmetric input is made internally.  The order of
		// the eigenvalues is specified by sortType: -1 (decreasing), 0 (no
		// sorting), or +1 (increasing).  When sorted, the eigenvectors are
		// ordered accordingly.  The return value is the number of iterations
		// consumed when convergence occurred, 0xFFFFFFFF when convergence did
		// not occur, or 0 when N <= 1 was passed to the constructor.
		public enum SortType
		{
			Decreasing = -1,
			NoSorting = 0,
			Increasing = 1
		}
		public const int NO_CONVERGENCE = int.MaxValue;
		public int Solve(in double[] input, in SortType eSort) {
			var sortType = (int)eSort;
			if (_mSize > 0) {
				Array.Copy(input, _mMatrix, _mSize * _mSize);
				Tridiagonalize();

				_mGivens.Clear();
				for (var j = 0; j < _mMaxIterations; ++j) {
					int imin = -1, imax = -1;
					for (var i = _mSize - 2; i >= 0; --i) {
						// When a01 is much smaller than its diagonal neighbors, it is
						// effectively zero.
						var a00 = _mDiagonal[i];
						var a01 = _mSuperdiagonal[i];
						var a11 = _mDiagonal[i + 1];
						var sum = Math.Abs(a00) + Math.Abs(a11);
						if (sum + Math.Abs(a01) != sum) {
							if (imax == -1) {
								imax = i;
							}
							imin = i;
						}
						else {
							// The superdiagonal term is effectively zero compared to
							// the neighboring diagonal terms.
							if (imin >= 0) {
								break;
							}
						}
					}

					if (imax == -1) {
						// The algorithm has converged.
						ComputePermutation(sortType);
						return j;
					}

					// Process the lower-right-most unreduced tridiagonal block.
					DoQRImplicitShift(imin, imax);
				}
				return NO_CONVERGENCE;
			}
			else {
				return 0;
			}
		}

		// Get the eigenvalues of the matrix passed to Solve(...).  The input
		// 'eigenvalues' must have N elements.
		public void GetEigenvalues(in double[] eigenvalues) {
			if (eigenvalues != null && _mSize > 0) {
				if (_mPermutation[0] >= 0) {
					// Sorting was requested.
					for (var i = 0; i < _mSize; ++i) {
						var p = _mPermutation[i];
						eigenvalues[i] = _mDiagonal[p];
					}
				}
				else {
					// Sorting was not requested.
					Array.Copy(_mDiagonal, eigenvalues, _mSize);
				}
			}
		}
		public double[] GetEigenvalues() {
			var eigenvalues = new double[_mSize];
			GetEigenvalues(eigenvalues);
			return eigenvalues;
		}
		public double GetEigenvalue(in int c) {
			if (_mSize > 0) {
				if (_mPermutation[0] >= 0) {
					// Sorting was requested.
					return _mDiagonal[_mPermutation[c]];
				}
				else {
					// Sorting was not requested.
					return _mDiagonal[c];
				}
			}
			else {
				return double.MaxValue;
			}
		}

		// Accumulate the Householder reflections and Givens rotations to produce
		// the orthogonal matrix Q for which Q^T*A*Q = D.  The input
		// 'eigenvectors' must be NxN and stored in row-major order.
		public void GetEigenvectors(in double[] eigenvectors) {
			if (eigenvectors != null && _mSize > 0) {
				// Start with the identity matrix.
				Array.Clear(eigenvectors, 0, _mSize * _mSize);
				for (var d = 0; d < _mSize; ++d) {
					eigenvectors[d + (_mSize * d)] = 1;
				}

				// Multiply the Householder reflections using backward accumulation.
				int r, c;
				for (int i = _mSize - 3, rmin = i + 1; i >= 0; --i, --rmin) {
					// Copy the v vector and 2/Dot(v,v) from the matrix.
					//double const* column = &mMatrix[i];
					var column = new ArrayAlias<double>(_mMatrix, i);
					var twoinvvdv = column[_mSize * (i + 1)];
					for (r = 0; r < i + 1; ++r) {
						_mVVector[r] = 0;
					}
					_mVVector[r] = 1;
					for (++r; r < _mSize; ++r) {
						_mVVector[r] = column[_mSize * r];
					}

					// Compute the w vector.
					for (r = 0; r < _mSize; ++r) {
						_mWVector[r] = 0;
						for (c = rmin; c < _mSize; ++c) {
							_mWVector[r] += _mVVector[c] * eigenvectors[r + (_mSize * c)];
						}
						_mWVector[r] *= twoinvvdv;
					}

					// Update the matrix, Q <- Q - v*w^T.
					for (r = rmin; r < _mSize; ++r) {
						for (c = 0; c < _mSize; ++c) {
							eigenvectors[c + (_mSize * r)] -= _mVVector[r] * _mWVector[c];
						}
					}
				}

				// Multiply the Givens rotations.
				foreach (var givens in _mGivens) {
					for (r = 0; r < _mSize; ++r) {
						var j = givens.index + (_mSize * r);
						var q0 = eigenvectors[j];
						var q1 = eigenvectors[j + 1];
						var prd0 = (givens.cs * q0) - (givens.sn * q1);
						var prd1 = (givens.sn * q0) + (givens.cs * q1);
						eigenvectors[j] = prd0;
						eigenvectors[j + 1] = prd1;
					}
				}

				_mIsRotation = 1 - (_mSize & 1);
				if (_mPermutation[0] >= 0) {
					// Sorting was requested.
					Array.Clear(_mVisited, 0, _mVisited.Length);
					for (var i = 0; i < _mSize; ++i) {
						if (_mVisited[i] == 0 && _mPermutation[i] != i) {
							// The item starts a cycle with 2 or more elements.
							_mIsRotation = 1 - _mIsRotation;
							int start = i, current = i, j, next;
							for (j = 0; j < _mSize; ++j) {
								_mPVector[j] = eigenvectors[i + (_mSize * j)];
							}
							while ((next = _mPermutation[current]) != start) {
								_mVisited[current] = 1;
								for (j = 0; j < _mSize; ++j) {
									eigenvectors[current + (_mSize * j)] =
										eigenvectors[next + (_mSize * j)];
								}
								current = next;
							}
							_mVisited[current] = 1;
							for (j = 0; j < _mSize; ++j) {
								eigenvectors[current + (_mSize * j)] = _mPVector[j];
							}
						}
					}
				}
			}
		}
		public double[] GetEigenvectors() {
			var eigenvectors = new double[_mSize * _mSize];
			GetEigenvectors(eigenvectors);
			return eigenvectors;
		}

		// With no sorting, when N is odd the matrix returned by GetEigenvectors
		// is a reflection and when N is even it is a rotation.  With sorting
		// enabled, the type of matrix returned depends on the permutation of
		// columns.  If the permutation has C cycles, the minimum number of column
		// transpositions is T = N-C.  Thus, when C is odd the matrix is a
		// reflection and when C is even the matrix is a rotation.
		public bool IsRotation() {
			if (_mSize > 0) {
				if (_mIsRotation == -1) {
					// Without sorting, the matrix is a rotation when size is even.
					_mIsRotation = 1 - (_mSize & 1);
					if (_mPermutation[0] >= 0) {
						// With sorting, the matrix is a rotation when the number of
						// cycles in the permutation is even.
						Array.Clear(_mVisited, 0, _mVisited.Length);
						for (var i = 0; i < _mSize; ++i) {
							if (_mVisited[i] == 0 && _mPermutation[i] != i) {
								// The item starts a cycle with 2 or more elements.
								int start = i, current = i, next;
								while ((next = _mPermutation[current]) != start) {
									_mVisited[current] = 1;
									current = next;
								}
								_mVisited[current] = 1;
							}
						}
					}
				}
				return _mIsRotation == 1;
			}
			else {
				return false;
			}
		}

		// Compute a single eigenvector, which amounts to computing column c
		// of matrix Q.  The reflections and rotations are applied incrementally.
		// This is useful when you want only a small number of the eigenvectors.
		public void GetEigenvector(in int c, in double[] eigenvector) {
			if (0 <= c && c < _mSize) {
				// y = H*x, then x and y are swapped for the next H
				var x = eigenvector;
				var y = _mPVector;

				// Start with the Euclidean basis vector.
				Array.Clear(x, 0, _mSize);
				if (_mPermutation[c] >= 0) {
					x[_mPermutation[c]] = 1;
				}
				else {
					x[c] = 1;
				}

				// Apply the Givens rotations.
				// [RMS] C# doesn't support reverse iterator so I replaced w/ loop...right?
				//typename std::vector < GivensRotation >::const_reverse_iterator givens = mGivens.rbegin();
				//for (/**/; givens != mGivens.rend(); ++givens) {
				for (var i = _mGivens.Count - 1; i >= 0; --i) {
					var givens = _mGivens[i];
					var xr = x[givens.index];
					var xrp1 = x[givens.index + 1];
					var tmp0 = (givens.cs * xr) + (givens.sn * xrp1);
					var tmp1 = (-givens.sn * xr) + (givens.cs * xrp1);
					x[givens.index] = tmp0;
					x[givens.index + 1] = tmp1;
				}

				// Apply the Householder reflections.
				for (var i = _mSize - 3; i >= 0; --i) {
					// Get the Householder vector v.
					//double const* column = &mMatrix[i];
					var column = new ArrayAlias<double>(_mMatrix, i);
					var twoinvvdv = column[_mSize * (i + 1)];
					int r;
					for (r = 0; r < i + 1; ++r) {
						y[r] = x[r];
					}

					// Compute s = Dot(x,v) * 2/v^T*v.
					var s = x[r];  // r = i+1, v[i+1] = 1
					for (var j = r + 1; j < _mSize; ++j) {
						s += x[j] * column[_mSize * j];
					}
					s *= twoinvvdv;

					y[r] = x[r] - s;  // v[i+1] = 1

					// Compute the remaining components of y.
					for (++r; r < _mSize; ++r) {
						y[r] = x[r] - (s * column[_mSize * r]);
					}

					//std::swap(x, y);
					(y, x) = (x, y);
				}
				// The final product is stored in x.

				if (x != eigenvector) {
					Array.Copy(x, eigenvector, _mSize);
				}
			}
		}
		public double[] GetEigenvector(in int c) {
			var eigenvector = new double[_mSize];
			GetEigenvector(c, eigenvector);
			return eigenvector;
		}

		// Tridiagonalize using Householder reflections.  On input, mMatrix is a
		// copy of the input matrix.  On output, the upper-triangular part of
		// mMatrix including the diagonal stores the tridiagonalization.  The
		// lower-triangular part contains 2/Dot(v,v) that are used in computing
		// eigenvectors and the part below the subdiagonal stores the essential
		// parts of the Householder vectors v (the elements of v after the
		// leading 1-valued component).
		private void Tridiagonalize() {
			int r, c;
			for (int i = 0, ip1 = 1; i < _mSize - 2; ++i, ++ip1) {
				// Compute the Householder vector.  Read the initial vector from the
				// row of the matrix.
				double length = 0;
				for (r = 0; r < ip1; ++r) {
					_mVVector[r] = 0;
				}
				for (r = ip1; r < _mSize; ++r) {
					var vr = _mMatrix[r + (_mSize * i)];
					_mVVector[r] = vr;
					length += vr * vr;
				}
				double vdv = 1;
				length = Math.Sqrt(length);
				if (length > 0) {
					var v1 = _mVVector[ip1];
					double sgn = v1 >= 0 ? 1 : -1;
					var invDenom = 1 / (v1 + (sgn * length));
					_mVVector[ip1] = (double)1;
					for (r = ip1 + 1; r < _mSize; ++r) {
						_mVVector[r] *= invDenom;
						vdv += _mVVector[r] * _mVVector[r];
					}
				}

				// Compute the rank-1 offsets v*w^T and w*v^T.
				var invvdv = 1 / vdv;
				var twoinvvdv = invvdv * 2;
				double pdvtvdv = 0;
				for (r = i; r < _mSize; ++r) {
					_mPVector[r] = 0;
					for (c = i; c < r; ++c) {
						_mPVector[r] += _mMatrix[r + (_mSize * c)] * _mVVector[c];
					}
					for (/**/; c < _mSize; ++c) {
						_mPVector[r] += _mMatrix[c + (_mSize * r)] * _mVVector[c];
					}
					_mPVector[r] *= twoinvvdv;
					pdvtvdv += _mPVector[r] * _mVVector[r];
				}

				pdvtvdv *= invvdv;
				for (r = i; r < _mSize; ++r) {
					_mWVector[r] = _mPVector[r] - (pdvtvdv * _mVVector[r]);
				}

				// Update the input matrix.
				for (r = i; r < _mSize; ++r) {
					var vr = _mVVector[r];
					var wr = _mWVector[r];
					var offset = vr * wr * 2;
					_mMatrix[r + (_mSize * r)] -= offset;
					for (c = r + 1; c < _mSize; ++c) {
						offset = (vr * _mWVector[c]) + (wr * _mVVector[c]);
						_mMatrix[c + (_mSize * r)] -= offset;
					}
				}

				// Copy the vector to column i of the matrix.  The 0-valued components
				// at indices 0 through i are not stored.  The 1-valued component at
				// index i+1 is also not stored; instead, the quantity 2/Dot(v,v) is
				// stored for use in eigenvector construction. That construction must
				// take into account the implied components that are not stored.
				_mMatrix[i + (_mSize * ip1)] = twoinvvdv;
				for (r = ip1 + 1; r < _mSize; ++r) {
					_mMatrix[i + (_mSize * r)] = _mVVector[r];
				}
			}

			// Copy the diagonal and subdiagonal entries for cache coherence in
			// the QR iterations.
			int k, ksup = _mSize - 1, index = 0, delta = _mSize + 1;
			for (k = 0; k < ksup; ++k, index += delta) {
				_mDiagonal[k] = _mMatrix[index];
				_mSuperdiagonal[k] = _mMatrix[index + 1];
			}
			_mDiagonal[k] = _mMatrix[index];
		}

		// A helper for generating Givens rotation sine and cosine robustly.
		private void GetSinCos(in double x,in double y, ref double cs, ref double sn) {
			// Solves sn*x + cs*y = 0 robustly.
			double tau;
			if (y != 0) {
				if (Math.Abs(y) > Math.Abs(x)) {
					tau = -x / y;
					sn = 1 / Math.Sqrt(1 + (tau * tau));
					cs = sn * tau;
				}
				else {
					tau = -y / x;
					cs = 1 / Math.Sqrt(1 + (tau * tau));
					sn = cs * tau;
				}
			}
			else {
				cs = 1;
				sn = 0;
			}
		}

		// The QR step with implicit shift.  Generally, the initial T is unreduced
		// tridiagonal (all subdiagonal entries are nonzero).  If a QR step causes
		// a superdiagonal entry to become zero, the matrix decouples into a block
		// diagonal matrix with two tridiagonal blocks.  These blocks can be
		// reduced independently of each other, which allows for parallelization
		// of the algorithm.  The inputs imin and imax identify the subblock of T
		// to be processed.   That block has upper-left element T(imin,imin) and
		// lower-right element T(imax,imax).
		private void DoQRImplicitShift(in int imin, in int imax) {
			// The implicit shift.  Compute the eigenvalue u of the lower-right 2x2
			// block that is closer to a11.
			var a00 = _mDiagonal[imax];
			var a01 = _mSuperdiagonal[imax];
			var a11 = _mDiagonal[imax + 1];
			var dif = (a00 - a11) * 0.5;
			var sgn = dif >= 0 ? 1 : -1;
			var a01sqr = a01 * a01;
			var u = a11 - (a01sqr / (dif + (sgn * Math.Sqrt((dif * dif) + a01sqr))));
			var x = _mDiagonal[imin] - u;
			var y = _mSuperdiagonal[imin];

			double a12, a22, a23, tmp11, tmp12, tmp21, tmp22, cs = 0, sn = 0;
			double a02 = 0;
			int i0 = imin - 1, i1 = imin, i2 = imin + 1;
			for (/**/; i1 <= imax; ++i0, ++i1, ++i2) {
				// Compute the Givens rotation and save it for use in computing the
				// eigenvectors.
				GetSinCos(x, y, ref cs, ref sn);
				_mGivens.Add(new GivensRotation(i1, cs, sn));

				// Update the tridiagonal matrix.  This amounts to updating a 4x4
				// subblock,
				//   b00 b01 b02 b03
				//   b01 b11 b12 b13
				//   b02 b12 b22 b23
				//   b03 b13 b23 b33
				// The four corners (b00, b03, b33) do not change values.  The
				// The interior block {{b11,b12},{b12,b22}} is updated on each pass.
				// For the first pass, the b0c values are out of range, so only
				// the values (b13, b23) change.  For the last pass, the br3 values
				// are out of range, so only the values (b01, b02) change.  For
				// passes between first and last, the values (b01, b02, b13, b23)
				// change.
				if (i1 > imin) {
					_mSuperdiagonal[i0] = (cs * _mSuperdiagonal[i0]) - (sn * a02);
				}

				a11 = _mDiagonal[i1];
				a12 = _mSuperdiagonal[i1];
				a22 = _mDiagonal[i2];
				tmp11 = (cs * a11) - (sn * a12);
				tmp12 = (cs * a12) - (sn * a22);
				tmp21 = (sn * a11) + (cs * a12);
				tmp22 = (sn * a12) + (cs * a22);
				_mDiagonal[i1] = (cs * tmp11) - (sn * tmp12);
				_mSuperdiagonal[i1] = (sn * tmp11) + (cs * tmp12);
				_mDiagonal[i2] = (sn * tmp21) + (cs * tmp22);

				if (i1 < imax) {
					a23 = _mSuperdiagonal[i2];
					a02 = -sn * a23;
					_mSuperdiagonal[i2] = cs * a23;

					// Update the parameters for the next Givens rotation.
					x = _mSuperdiagonal[i1];
					y = a02;
				}
			}
		}

		// Sort the eigenvalues and compute the corresponding permutation of the
		// indices of the array storing the eigenvalues.  The permutation is used
		// for reordering the eigenvalues and eigenvectors in the calls to
		// GetEigenvalues(...) and GetEigenvectors(...).
		private void ComputePermutation(in int sortType) {
			_mIsRotation = -1;

			if (sortType == 0) {
				// Set a flag for GetEigenvalues() and GetEigenvectors() to know
				// that sorted output was not requested.
				_mPermutation[0] = -1;
				return;
			}

			// Compute the permutation induced by sorting.  Initially, we start with
			// the identity permutation I = (0,1,...,N-1).
			var items = new SortItem[_mSize];
			for (var i = 0; i < _mSize; ++i) {
				items[i].eigenvalue = _mDiagonal[i];
				items[i].index = i;
			}

			if (sortType > 0) {
				//std::sort(items.begin(), items.end(), std::less<SortItem>());
				Array.Sort(items, (a, b) => a.eigenvalue == b.eigenvalue ? 0 : a.eigenvalue < b.eigenvalue ? -1 : 1);
			}
			else {
				//std::sort(items.begin(), items.end(), std::greater<SortItem>());
				Array.Sort(items, (a, b) => a.eigenvalue == b.eigenvalue ? 0 : a.eigenvalue > b.eigenvalue ? -1 : 1);
			}

			for (var i = 0; i < _mSize; ++i) {
				_mPermutation[i] = items[i].index;
			}
			//typename std::vector < SortItem >::const_iterator item = items.begin();
			//for (i = 0; item != items.end(); ++item, ++i) {
			//    mPermutation[i] = item->index;
			//}

			// GetEigenvectors() has nontrivial code for computing the orthogonal Q
			// from the reflections and rotations.  To avoid complicating the code
			// further when sorting is requested, Q is computed as in the unsorted
			// case.  We then need to swap columns of Q to be consistent with the
			// sorting of the eigenvalues.  To minimize copying due to column swaps,
			// we use permutation P.  The minimum number of transpositions to obtain
			// P from I is N minus the number of cycles of P.  Each cycle is reordered
			// with a minimum number of transpositions; that is, the eigenitems are
			// cyclically swapped, leading to a minimum amount of copying.  For
			// example, if there is a cycle i0 -> i1 -> i2 -> i3, then the copying is
			//   save = eigenitem[i0];
			//   eigenitem[i1] = eigenitem[i2];
			//   eigenitem[i2] = eigenitem[i3];
			//   eigenitem[i3] = save;
		}

		// The number N of rows and columns of the matrices to be processed.
		private readonly int _mSize;

		// The maximum number of iterations for reducing the tridiagonal mtarix
		// to a diagonal matrix.
		private readonly int _mMaxIterations;

		// The internal copy of a matrix passed to the solver.  See the comments
		// about function Tridiagonalize() about what is stored in the matrix.
		private readonly double[] _mMatrix;  // NxN elements

		// After the initial tridiagonalization by Householder reflections, we no
		// longer need the full mMatrix.  Copy the diagonal and superdiagonal
		// entries to linear arrays in order to be cache friendly.
		private readonly double[] _mDiagonal;  // N elements
		private readonly double[] _mSuperdiagonal;  // N-1 elements

		// The Givens rotations used to reduce the initial tridiagonal matrix to
		// a diagonal matrix.  A rotation is the identity with the following
		// replacement entries:  R(index,index) = cs, R(index,index+1) = sn,
		// R(index+1,index) = -sn, and R(index+1,index+1) = cs.  If N is the
		// matrix size and K is the maximum number of iterations, the maximum
		// number of Givens rotations is K*(N-1).  The maximum amount of memory
		// is allocated to store these.
		private struct GivensRotation
		{
			public GivensRotation(in int inIndex, in double inCs, in double inSn) {
				index = inIndex;
				cs = inCs;
				sn = inSn;
			}
			public int index;
			public double cs, sn;
		};

		private readonly List<GivensRotation> _mGivens;  // K*(N-1) elements

		// When sorting is requested, the permutation associated with the sort is
		// stored in mPermutation.  When sorting is not requested, mPermutation[0]
		// is set to -1.  mVisited is used for finding cycles in the permutation.
		private struct SortItem
		{
			public double eigenvalue;
			public int index;
		};
		private readonly int[] _mPermutation;  // N elements
		private readonly int[] _mVisited;  // N elements
		private int _mIsRotation;  // 1 = rotation, 0 = reflection, -1 = unknown

		// Temporary storage to compute Householder reflections and to support
		// sorting of eigenvectors.
		private readonly double[] _mPVector;  // N elements
		private readonly double[] _mVVector;  // N elements
		private readonly double[] _mWVector;  // N elements

	}
}
