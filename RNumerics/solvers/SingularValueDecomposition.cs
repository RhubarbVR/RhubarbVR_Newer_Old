using System;
using System.Collections.Generic;

namespace RNumerics
{
	/// <summary>
	/// Singular Value Decomposition of arbitrary matrix A
	/// Computes U/S/V of  A = U * S * V^T
	/// 
	/// Useful Properties:
	///  S = square-roots of eigenvalues of A
	///  U = eigenvectors of A * A^T
	///  V = eigenvectors of A^T * A
	///  U * V^T = rotation matrix closest to A 
	///  V * Inv(S) * U^T = psuedoinverse of A
	///  
	/// U and/or V are rotation matrices but may also contain reflections
	/// Detection: det(U) or det(v) == -1
	/// Removal: if ( det(U) == -1 ) { U *= -1; S *= -1 }
	///          if ( det(V) == -1 ) { V *= -1; S *= -1 }     (right? seems to work)
	///  
	/// </summary>
	public sealed class SingularValueDecomposition
	{
		// port of WildMagic5 SingularValueDecomposition class (which is a back-port
		// of GTEngine SVD class) see geometrictools.com


		// The solver processes MxN symmetric matrices, where M >= N > 1
		// ('numRows' is M and 'numCols' is N) and the matrix is stored in
		// row-major order.  The maximum number of iterations ('maxIterations')
		// must be specified for the reduction of a bidiagonal matrix to a
		// diagonal matrix.  The goal is to compute MxM orthogonal U, NxN
		// orthogonal V, and MxN matrix S for which U^T*A*V = S.  The only
		// nonzero entries of S are on the diagonal; the diagonal entries are
		// the singular values of the original matrix.
		public SingularValueDecomposition(in int numRows, in int numCols, in int maxIterations) {
			_mNumRows = _mNumCols = _mMaxIterations = 0;
			if (numCols > 1 && numRows >= numCols && maxIterations > 0) {
				_mNumRows = numRows;
				_mNumCols = numCols;
				_mMaxIterations = maxIterations;
				_mMatrix = new double[(numRows * numCols)];
				_mDiagonal = new double[numCols];
				_mSuperdiagonal = new double[(numCols - 1)];
				_mRGivens = new List<GivensRotation>(maxIterations * (numCols - 1));
				_mLGivens = new List<GivensRotation>(maxIterations * (numCols - 1));
				_mFixupDiagonal = new double[numCols];
				_mPermutation = new int[numCols];
				_mVisited = new int[numCols];
				_mTwoInvUTU = new double[numCols];
				_mTwoInvVTV = new double[(numCols - 2)];
				_mUVector = new double[numRows];
				_mVVector = new double[numCols];
				_mWVector = new double[numRows];
			}
		}

		// A copy of the MxN input is made internally.  The order of the singular
		// values is specified by sortType: -1 (decreasing), 0 (no sorting), or +1
		// (increasing).  When sorted, the columns of the orthogonal matrices
		// are ordered accordingly.  The return value is the number of iterations
		// consumed when convergence occurred, 0xFFFFFFFF when convergence did not
		// occur or 0 when N <= 1 or M < N was passed to theructor.
		public uint Solve(in double[] input, in int sortType = -1) {
			if (_mNumRows > 0) {
				var numElements = _mNumRows * _mNumCols;
				Array.Copy(input, _mMatrix, numElements);
				Bidiagonalize();

				// Compute 'threshold = multiplier*epsilon*|B|' as the threshold for
				// diagonal entries effectively zero; that is, |d| <= |threshold|
				// implies that d is (effectively) zero.  TODO: Allow the caller to
				// pass 'multiplier' to the constructor.
				//
				// We will use the L2-norm |B|, which is the length of the elements
				// of B treated as an NM-tuple.  The following code avoids overflow
				// when accumulating the squares of the elements when those elements
				// are large.
				var maxAbsComp = Math.Abs(input[0]);
				for (var i = 1; i < numElements; ++i) {
					var absComp = Math.Abs(input[i]);
					if (absComp > maxAbsComp) {
						maxAbsComp = absComp;
					}
				}

				var norm = (double)0;
				if (maxAbsComp > 0) {
					var invMaxAbsComp = 1 / maxAbsComp;
					for (var i = 0; i < numElements; ++i) {
						var ratio = input[i] * invMaxAbsComp;
						norm += ratio * ratio;
					}
					norm = maxAbsComp * Math.Sqrt(norm);
				}

				var multiplier = (double)8;  // TODO: Expose to caller.
				var epsilon = double.Epsilon;
				var threshold = multiplier * epsilon * norm;

				_mRGivens.Clear();
				_mLGivens.Clear();
				for (uint j = 0; j < _mMaxIterations; ++j) {
					int imin = -1, imax = -1;
					for (var i = _mNumCols - 2; i >= 0; --i) {
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
						EnsureNonnegativeDiagonal();
						ComputePermutation(sortType);
						return j;
					}

					// We need to test diagonal entries of B for zero.  For each zero
					// diagonal entry, zero the superdiagonal.
					if (DiagonalEntriesNonzero(imin, imax, threshold)) {
						// Process the lower-right-most unreduced bidiagonal block.
						DoGolubKahanStep(imin, imax);
					}
				}
				return 0xFFFFFFFF;
			}
			else {
				return 0;
			}
		}

		// Get the singular values of the matrix passed to Solve(...).  The input
		// 'singularValues' must have N elements.
		public void GetSingularValues(in double[] singularValues) {
			if (singularValues != null && _mNumCols > 0) {
				if (_mPermutation[0] >= 0) {
					// Sorting was requested.
					for (var i = 0; i < _mNumCols; ++i) {
						var p = _mPermutation[i];
						singularValues[i] = _mDiagonal[p];
					}
				}
				else {
					// Sorting was not requested.
					for (var i = 0; i < _mNumCols; ++i) {
						singularValues[i] = _mDiagonal[i];
					}
				}
			}
		}

		// Accumulate the Householder reflections, the Givens rotations, and the
		// diagonal fix-up matrix to compute the orthogonal matrices U and V for
		// which U^T*A*V = S.  The input uMatrix must be MxM and the input vMatrix
		// must be NxN, both stored in row-major order.
		public void GetU(in double[] uMatrix) {
			if (uMatrix == null || _mNumCols == 0) {
				// Invalid input or the constructor failed.
				return;
			}

			// Start with the identity matrix.
			Array.Clear(uMatrix, 0, uMatrix.Length);
			for (var d = 0; d < _mNumRows; ++d) {
				uMatrix[d + (_mNumRows * d)] = (double)1;
			}

			// Multiply the Householder reflections using backward accumulation.
			int r, c;
			for (int i0 = _mNumCols - 1, i1 = i0 + 1; i0 >= 0; --i0, --i1) {
				// Copy the u vector and 2/Dot(u,u) from the matrix.
				var twoinvudu = _mTwoInvUTU[i0];
				//double const* column = &mMatrix[i0];
				_mUVector[i0] = (double)1;
				for (r = i1; r < _mNumRows; ++r) {
					//mUVector[r] = column[mNumCols * r];
					_mUVector[r] = _mMatrix[i0 + (_mNumCols * r)];
				}

				// Compute the w vector.
				_mWVector[i0] = twoinvudu;
				for (r = i1; r < _mNumRows; ++r) {
					_mWVector[r] = 0;
					for (c = i1; c < _mNumRows; ++c) {
						_mWVector[r] += _mUVector[c] * uMatrix[r + (_mNumRows * c)];
					}
					_mWVector[r] *= twoinvudu;
				}

				// Update the matrix, U <- U - u*w^T.
				for (r = i0; r < _mNumRows; ++r) {
					for (c = i0; c < _mNumRows; ++c) {
						uMatrix[c + (_mNumRows * r)] -= _mUVector[r] * _mWVector[c];
					}
				}
			}

			// Multiply the Givens rotations.
			foreach (var givens in _mLGivens) {
				var j0 = givens.index0;
				var j1 = givens.index1;
				for (r = 0; r < _mNumRows; ++r, j0 += _mNumRows, j1 += _mNumRows) {
					var q0 = uMatrix[j0];
					var q1 = uMatrix[j1];
					var prd0 = (givens.cs * q0) - (givens.sn * q1);
					var prd1 = (givens.sn * q0) + (givens.cs * q1);
					uMatrix[j0] = prd0;
					uMatrix[j1] = prd1;
				}
			}

			if (_mPermutation[0] >= 0) {
				// Sorting was requested.
				Array.Clear(_mVisited, 0, _mVisited.Length);
				for (c = 0; c < _mNumCols; ++c) {
					if (_mVisited[c] == 0 && _mPermutation[c] != c) {
						// The item starts a cycle with 2 or more elements.
						int start = c, current = c, next;
						for (r = 0; r < _mNumRows; ++r) {
							_mWVector[r] = uMatrix[c + (_mNumRows * r)];
						}
						while ((next = _mPermutation[current]) != start) {
							_mVisited[current] = 1;
							for (r = 0; r < _mNumRows; ++r) {
								uMatrix[current + (_mNumRows * r)] =
									uMatrix[next + (_mNumRows * r)];
							}
							current = next;
						}
						_mVisited[current] = 1;
						for (r = 0; r < _mNumRows; ++r) {
							uMatrix[current + (_mNumRows * r)] = _mWVector[r];
						}
					}
				}
			}
		}


		public void GetV(in double[] vMatrix) {
			if (vMatrix == null || _mNumCols == 0) {
				// Invalid input or the constructor failed.
				return;
			}

			// Start with the identity matrix.
			Array.Clear(vMatrix, 0, vMatrix.Length);
			for (var d = 0; d < _mNumCols; ++d) {
				vMatrix[d + (_mNumCols * d)] = (double)1;
			}

			// Multiply the Householder reflections using backward accumulation.
			var i0 = _mNumCols - 3;
			var i1 = i0 + 1;
			var i2 = i0 + 2;
			int r, c;
			for (/**/; i0 >= 0; --i0, --i1, --i2) {
				// Copy the v vector and 2/Dot(v,v) from the matrix.
				var twoinvvdv = _mTwoInvVTV[i0];
				//double const* row = &mMatrix[mNumCols * i0];      // [RMS] port
				_mVVector[i1] = (double)1;
				for (r = i2; r < _mNumCols; ++r) {
					//mVVector[r] = row[r];         // [RMS] port
					_mVVector[r] = _mMatrix[(_mNumCols * i0) + r];
				}

				// Compute the w vector.
				_mWVector[i1] = twoinvvdv;
				for (r = i2; r < _mNumCols; ++r) {
					_mWVector[r] = 0;
					for (c = i2; c < _mNumCols; ++c) {
						_mWVector[r] += _mVVector[c] * vMatrix[r + (_mNumCols * c)];
					}
					_mWVector[r] *= twoinvvdv;
				}

				// Update the matrix, V <- V - v*w^T.
				for (r = i1; r < _mNumCols; ++r) {
					for (c = i1; c < _mNumCols; ++c) {
						vMatrix[c + (_mNumCols * r)] -= _mVVector[r] * _mWVector[c];
					}
				}
			}

			// Multiply the Givens rotations.
			foreach (var givens in _mRGivens) {
				var j0 = givens.index0;
				var j1 = givens.index1;
				for (c = 0; c < _mNumCols; ++c, j0 += _mNumCols, j1 += _mNumCols) {
					var q0 = vMatrix[j0];
					var q1 = vMatrix[j1];
					var prd0 = (givens.cs * q0) - (givens.sn * q1);
					var prd1 = (givens.sn * q0) + (givens.cs * q1);
					vMatrix[j0] = prd0;
					vMatrix[j1] = prd1;
				}
			}

			// Fix-up the diagonal.
			for (r = 0; r < _mNumCols; ++r) {
				for (c = 0; c < _mNumCols; ++c) {
					vMatrix[c + (_mNumCols * r)] *= _mFixupDiagonal[c];
				}
			}

			if (_mPermutation[0] >= 0) {
				// Sorting was requested.
				Array.Clear(_mVisited, 0, _mVisited.Length);
				for (c = 0; c < _mNumCols; ++c) {
					if (_mVisited[c] == 0 && _mPermutation[c] != c) {
						// The item starts a cycle with 2 or more elements.
						int start = c, current = c, next;
						for (r = 0; r < _mNumCols; ++r) {
							_mWVector[r] = vMatrix[c + (_mNumCols * r)];
						}
						while ((next = _mPermutation[current]) != start) {
							_mVisited[current] = 1;
							for (r = 0; r < _mNumCols; ++r) {
								vMatrix[current + (_mNumCols * r)] =
									vMatrix[next + (_mNumCols * r)];
							}
							current = next;
						}
						_mVisited[current] = 1;
						for (r = 0; r < _mNumCols; ++r) {
							vMatrix[current + (_mNumCols * r)] = _mWVector[r];
						}
					}
				}
			}
		}



		//
		// internals
		//


		// Bidiagonalize using Householder reflections.  On input, mMatrix is a
		// copy of the input matrix and has one extra row.  On output, the
		// diagonal and superdiagonal contain the bidiagonalized results.  The
		// lower-triangular portion stores the essential parts of the Householder
		// u vectors (the elements of u after the leading 1-valued component) and
		// the upper-triangular portion stores the essential parts of the
		// Householder v vectors.  To avoid recomputing 2/Dot(u,u) and 2/Dot(v,v),
		// these quantities are stored in mTwoInvUTU and mTwoInvVTV.
		void Bidiagonalize() {
			int r, c;
			for (int i = 0, ip1 = 1; i < _mNumCols; ++i, ++ip1) {
				// Compute the U-Householder vector.
				var length = (double)0;
				for (r = i; r < _mNumRows; ++r) {
					var ur = _mMatrix[i + (_mNumCols * r)];
					_mUVector[r] = ur;
					length += ur * ur;
				}
				double udu = 1;
				length = Math.Sqrt(length);
				if (length > 0) {
					var u1 = _mUVector[i];
					var sgn = u1 >= 0 ? 1 : (double)-1;
					var invDenom = 1 / (u1 + (sgn * length));
					_mUVector[i] = 1;
					for (r = ip1; r < _mNumRows; ++r) {
						_mUVector[r] *= invDenom;
						udu += _mUVector[r] * _mUVector[r];
					}
				}

				// Compute the rank-1 offset u*w^T.
				var invudu = 1 / udu;
				var twoinvudu = invudu * 2;
				for (c = i; c < _mNumCols; ++c) {
					_mWVector[c] = 0;
					for (r = i; r < _mNumRows; ++r) {
						_mWVector[c] += _mMatrix[c + (_mNumCols * r)] * _mUVector[r];
					}
					_mWVector[c] *= twoinvudu;
				}

				// Update the input matrix.
				for (r = i; r < _mNumRows; ++r) {
					for (c = i; c < _mNumCols; ++c) {
						_mMatrix[c + (_mNumCols * r)] -= _mUVector[r] * _mWVector[c];
					}
				}

				if (i < _mNumCols - 2) {
					// Compute the V-Householder vectors.
					length = 0;
					for (c = ip1; c < _mNumCols; ++c) {
						var vc = _mMatrix[c + (_mNumCols * i)];
						_mVVector[c] = vc;
						length += vc * vc;
					}
					var vdv = (double)1;
					length = Math.Sqrt(length);
					if (length > 0) {
						var v1 = _mVVector[ip1];
						var sgn = v1 >= 0 ? 1 : (double)-1;
						var invDenom = 1 / (v1 + (sgn * length));
						_mVVector[ip1] = 1;
						for (c = ip1 + 1; c < _mNumCols; ++c) {
							_mVVector[c] *= invDenom;
							vdv += _mVVector[c] * _mVVector[c];
						}
					}

					// Compute the rank-1 offset w*v^T.
					var invvdv = 1 / vdv;
					var twoinvvdv = invvdv * 2;
					for (r = i; r < _mNumRows; ++r) {
						_mWVector[r] = 0;
						for (c = ip1; c < _mNumCols; ++c) {
							_mWVector[r] += _mMatrix[c + (_mNumCols * r)] * _mVVector[c];
						}
						_mWVector[r] *= twoinvvdv;
					}

					// Update the input matrix.
					for (r = i; r < _mNumRows; ++r) {
						for (c = ip1; c < _mNumCols; ++c) {
							_mMatrix[c + (_mNumCols * r)] -= _mWVector[r] * _mVVector[c];
						}
					}

					_mTwoInvVTV[i] = twoinvvdv;
					for (c = i + 2; c < _mNumCols; ++c) {
						_mMatrix[c + (_mNumCols * i)] = _mVVector[c];
					}
				}

				_mTwoInvUTU[i] = twoinvudu;
				for (r = ip1; r < _mNumRows; ++r) {
					_mMatrix[i + (_mNumCols * r)] = _mUVector[r];
				}
			}

			// Copy the diagonal and subdiagonal for cache coherence in the
			// Golub-Kahan iterations.
			int k, ksup = _mNumCols - 1, index = 0, delta = _mNumCols + 1;
			for (k = 0; k < ksup; ++k, index += delta) {
				_mDiagonal[k] = _mMatrix[index];
				_mSuperdiagonal[k] = _mMatrix[index + 1];
			}
			_mDiagonal[k] = _mMatrix[index];
		}

		// A helper for generating Givens rotation sine and cosine robustly.
		static void GetSinCos(in double x, in double y, out double cs, out double sn) {
			// Solves sn*x + cs*y = 0 robustly.
			double tau;
			if (y != (double)0) {
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
				cs = (double)1;
				sn = (double)0;
			}
		}

		// Test for (effectively) zero-valued diagonal entries (through all but
		// the last).  For each such entry, the B matrix decouples.  Perform
		// that decoupling.  If there are no zero-valued entries, then the
		// Golub-Kahan step must be performed.
		bool DiagonalEntriesNonzero(in int imin, in int imax, in double threshold) {
			for (var i = imin; i <= imax; ++i) {
				if (Math.Abs(_mDiagonal[i]) <= threshold) {
					// Use planar rotations to case the superdiagonal entry out of
					// the matrix, thus producing a row of zeros.
					double x, z;
					var y = _mSuperdiagonal[i];
					_mSuperdiagonal[i] = (double)0;
					for (var j = i + 1; j <= imax + 1; ++j) {
						x = _mDiagonal[j];
						GetSinCos(x, y, out var cs, out var sn);
						_mLGivens.Add(new GivensRotation(i, j, cs, sn));
						_mDiagonal[j] = (cs * x) - (sn * y);
						if (j <= imax) {
							z = _mSuperdiagonal[j];
							_mSuperdiagonal[j] = cs * z;
							y = sn * z;
						}
					}
					return false;
				}
			}
			return true;
		}

		// This is Algorithm 8.3.1 in "Matrix Computations, 2nd edition" by
		// G. H. Golub and C. F. Van Loan.
		void DoGolubKahanStep(in int imin, in int imax) {
			// The implicit shift.  Compute the eigenvalue u of the lower-right 2x2
			// block of A = B^T*B that is closer to b11.
			var f0 = imax >= (double)1 ? _mSuperdiagonal[imax - 1] : (double)0;
			var d1 = _mDiagonal[imax];
			var f1 = _mSuperdiagonal[imax];
			var d2 = _mDiagonal[imax + 1];
			var a00 = (d1 * d1) + (f0 * f0);
			var a01 = d1 * f1;
			var a11 = (d2 * d2) + (f1 * f1);
			var dif = (a00 - a11) * (double)0.5;
			var sgn = dif >= (double)0 ? (double)1 : (double)-1;
			var a01sqr = a01 * a01;
			var u = a11 - (a01sqr / (dif + (sgn * Math.Sqrt((dif * dif) + a01sqr))));
			var x = (_mDiagonal[imin] * _mDiagonal[imin]) - u;
			var y = _mDiagonal[imin] * _mSuperdiagonal[imin];

			double a12, a21, a22, a23;
			var a02 = (double)0;
			int i0 = imin - 1, i1 = imin, i2 = imin + 1;
			for (/**/; i1 <= imax; ++i0, ++i1, ++i2) {
				// Compute the Givens rotation G and save it for use in computing
				// V in U^T*A*V = S.
				GetSinCos(x, y, out var cs, out var sn);
				_mRGivens.Add(new GivensRotation(i1, i2, cs, sn));

				// Update B0 = B*G.
				if (i1 > imin) {
					_mSuperdiagonal[i0] = (cs * _mSuperdiagonal[i0]) - (sn * a02);
				}

				a11 = _mDiagonal[i1];
				a12 = _mSuperdiagonal[i1];
				a22 = _mDiagonal[i2];
				_mDiagonal[i1] = (cs * a11) - (sn * a12);
				_mSuperdiagonal[i1] = (sn * a11) + (cs * a12);
				_mDiagonal[i2] = cs * a22;
				a21 = -sn * a22;

				// Update the parameters for the next Givens rotations.
				x = _mDiagonal[i1];
				y = a21;

				// Compute the Givens rotation G and save it for use in computing
				// U in U^T*A*V = S.
				GetSinCos(x, y, out cs, out sn);
				_mLGivens.Add(new GivensRotation(i1, i2, cs, sn));

				// Update B1 = G^T*B0.
				a11 = _mDiagonal[i1];
				a12 = _mSuperdiagonal[i1];
				a22 = _mDiagonal[i2];
				_mDiagonal[i1] = (cs * a11) - (sn * a21);
				_mSuperdiagonal[i1] = (cs * a12) - (sn * a22);
				_mDiagonal[i2] = (sn * a12) + (cs * a22);

				if (i1 < imax) {
					a23 = _mSuperdiagonal[i2];
					a02 = -sn * a23;
					_mSuperdiagonal[i2] = cs * a23;

					// Update the parameters for the next Givens rotations.
					x = _mSuperdiagonal[i1];
					y = a02;
				}
			}
		}

		// The diagonal entries are not guaranteed to be nonnegative during the
		//ruction.  After convergence to a diagonal matrix S, test for
		// negative entries and build a diagonal matrix that reverses the sign
		// on the S-entry.
		void EnsureNonnegativeDiagonal() {
			for (var i = 0; i < _mNumCols; ++i) {
				if (_mDiagonal[i] >= 0) {
					_mFixupDiagonal[i] = 1.0;
				}
				else {
					_mDiagonal[i] = -_mDiagonal[i];
					_mFixupDiagonal[i] = -1.0;
				}
			}
		}

		// Sort the singular values and compute the corresponding permutation of
		// the indices of the array storing the singular values.  The permutation
		// is used for reordering the singular values and the corresponding
		// columns of the orthogonal matrix in the calls to GetSingularValues(...)
		// and GetOrthogonalMatrices(...).
		void ComputePermutation(in int sortType) {
			if (sortType == 0) {
				// Set a flag for GetSingularValues() and GetOrthogonalMatrices() to
				// know that sorted output was not requested.
				_mPermutation[0] = -1;
				return;
			}

			var singularValues = new double[_mNumCols];
			var indices = new int[_mNumCols];
			for (var i = 0; i < _mNumCols; ++i) {
				singularValues[i] = _mDiagonal[i];
				indices[i] = i;
			}
			Array.Sort(singularValues, indices);
			if (sortType < 0) {
				Array.Reverse(indices);
			}

			_mPermutation = indices;

			// GetOrthogonalMatrices() has nontrivial code for computing the
			// orthogonal U and V from the reflections and rotations.  To avoid
			// complicating the code further when sorting is requested, U and V are
			// computed as in the unsorted case.  We then need to swap columns of
			// U and V to be consistent with the sorting of the singular values.  To
			// minimize copying due to column swaps, we use permutation P.  The
			// minimum number of transpositions to obtain P from I is N minus the
			// number of cycles of P.  Each cycle is reordered with a minimum number
			// of transpositions; that is, the singular items are cyclically swapped,
			// leading to a minimum amount of copying.  For example, if there is a
			// cycle i0 -> i1 -> i2 -> i3, then the copying is
			//   save = singularitem[i0];
			//   singularitem[i1] = singularitem[i2];
			//   singularitem[i2] = singularitem[i3];
			//   singularitem[i3] = save;
		}

		// The number rows and columns of the matrices to be processed.
		private readonly int _mNumRows;
		private readonly int _mNumCols;

		// The maximum number of iterations for reducing the bidiagonal matrix
		// to a diagonal matrix.
		readonly int _mMaxIterations;

		// The internal copy of a matrix passed to the solver.  See the comments
		// about function Bidiagonalize() about what is stored in the matrix.
		readonly double[] _mMatrix;  // MxN elements

		// After the initial bidiagonalization by Householder reflections, we no
		// longer need the full mMatrix.  Copy the diagonal and superdiagonal
		// entries to linear arrays in order to be cache friendly.
		readonly double[] _mDiagonal;  // N elements
		readonly double[] _mSuperdiagonal;  // N-1 elements

		// The Givens rotations used to reduce the initial bidiagonal matrix to
		// a diagonal matrix.  A rotation is the identity with the following
		// replacement entries:  R(index0,index0) = cs, R(index0,index1) = sn,
		// R(index1,index0) = -sn, and R(index1,index1) = cs.  If N is the
		// number of matrix columns and K is the maximum number of iterations, the
		// maximum number of right or left Givens rotations is K*(N-1).  The
		// maximum amount of memory is allocated to store these.  However, we also
		// potentially need left rotations to decouple the matrix when a diagonal
		// terms are zero.  Worst case is a number of matrices quadratic in N, so
		// for now we just use std::vector<Rotation> whose initial capacity is
		// K*(N-1).
		struct GivensRotation
		{
			public GivensRotation(in int inIndex0, in int inIndex1, in double inCs, in double inSn) {
				index0 = inIndex0;
				index1 = inIndex1;
				cs = inCs;
				sn = inSn;
			}
			public int index0, index1;
			public double cs, sn;
		};

		readonly List<GivensRotation> _mRGivens;
		readonly List<GivensRotation> _mLGivens;

		// The diagonal matrix that is used to convert S-entries to nonnegative.
		readonly double[] _mFixupDiagonal;  // N elements

		private int[] _mPermutation;  // N elements
		readonly int[] _mVisited;  // N elements

		// Temporary storage to compute Householder reflections and to support
		// sorting of columns of the orthogonal matrices.
		readonly double[] _mTwoInvUTU;  // N elements
		readonly double[] _mTwoInvVTV;  // N-2 elements
		readonly double[] _mUVector;  // M elements
		readonly double[] _mVVector;  // N elements
		readonly double[] _mWVector;  // max(M,N) elements
	}
}
