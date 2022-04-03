using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RNumerics
{
	/// <summary>
	/// Variant of LaplacianMeshDeformer that can be applied to 3D curve.
	/// 
	/// Solve in each dimension can be disabled using .SolveX/Y/Z
	/// 
	/// Currently only supports uniform weights (in Initialize)
	/// 
	/// </summary>
	public class LaplacianCurveDeformer
	{
		public DCurve3 Curve;


		public bool SolveX = true;
		public bool SolveY = true;
		public bool SolveZ = true;


		// indicates that solve did not converge in at least one dimension
		public bool ConvergeFailed = false;


		// info that is fixed based on mesh
		PackedSparseMatrix _packedM;
		int _n;
		int[] _toCurveV, _toIndex;
		double[] _px, _py, _pz;
		int[] _nbr_counts;
		double[] _mLx, _mLy, _mLz;

		// constraints
		public struct SoftConstraintV
		{
			public Vector3d Position;
			public double Weight;
			public bool PostFix;
		}

		readonly Dictionary<int, SoftConstraintV> _softConstraints = new();
		bool _havePostFixedConstraints = false;


		// needs to be updated after constraints
		bool _need_solve_update;
		DiagonalMatrix _weightsM;
		double[] _cx, _cy, _cz;
		double[] _bx, _by, _bz;
		DiagonalMatrix _preconditioner;


		// Appendix C from http://sites.fas.harvard.edu/~cs277/papers/deformation_survey.pdf
		public bool UseSoftConstraintNormalEquations = true;


		// result
		double[] _sx, _sy, _sz;


		public LaplacianCurveDeformer(DCurve3 curve) {
			Curve = curve;
		}


		public void SetConstraint(int vID, Vector3d targetPos, double weight, bool bForceToFixedPos = false) {
			_softConstraints[vID] = new SoftConstraintV() { Position = targetPos, Weight = weight, PostFix = bForceToFixedPos };
			_havePostFixedConstraints = _havePostFixedConstraints || bForceToFixedPos;
			_need_solve_update = true;
		}

		public bool IsConstrained(int vID) {
			return _softConstraints.ContainsKey(vID);
		}

		public void ClearConstraints() {
			_softConstraints.Clear();
			_havePostFixedConstraints = false;
			_need_solve_update = true;
		}


		public void Initialize() {
			var NV = Curve.VertexCount;
			_toCurveV = new int[NV];
			_toIndex = new int[NV];

			_n = 0;
			for (var k = 0; k < NV; k++) {
				var vid = k;
				_toCurveV[_n] = vid;
				_toIndex[vid] = _n;
				_n++;
			}

			_px = new double[_n];
			_py = new double[_n];
			_pz = new double[_n];
			_nbr_counts = new int[_n];
			var M = new SymmetricSparseMatrix();

			for (var i = 0; i < _n; ++i) {
				var vid = _toCurveV[i];
				var v = Curve.GetVertex(vid);
				_px[i] = v.x;
				_py[i] = v.y;
				_pz[i] = v.z;
				_nbr_counts[i] = (i == 0 || i == _n - 1) ? 1 : 2;
			}

			// construct laplacian matrix
			for (var i = 0; i < _n; ++i) {
				var vid = _toCurveV[i];
				var n = _nbr_counts[i];

				var nbrs = Curve.Neighbours(vid);

				double sum_w = 0;
				for (var k = 0; k < 2; ++k) {
					var nbrvid = nbrs[k];
					if (nbrvid == -1) {
						continue;
					}

					var j = _toIndex[nbrvid];
					var n2 = _nbr_counts[j];

					// weight options
					double w = -1;
					//double w = -1.0 / Math.Sqrt(n + n2);
					//double w = -1.0 / n;

					M.Set(i, j, w);
					sum_w += w;
				}
				sum_w = -sum_w;
				M.Set(vid, vid, sum_w);
			}

			// transpose(L) * L, but matrix is symmetric...
			if (UseSoftConstraintNormalEquations) {
				//M = M.Multiply(M);
				// only works if M is symmetric!!
				_packedM = M.SquarePackedParallel();
			}
			else {
				_packedM = new PackedSparseMatrix(M);
			}

			// compute laplacian vectors of initial mesh positions
			_mLx = new double[_n];
			_mLy = new double[_n];
			_mLz = new double[_n];
			_packedM.Multiply(_px, _mLx);
			_packedM.Multiply(_py, _mLy);
			_packedM.Multiply(_pz, _mLz);

			// allocate memory for internal buffers
			_preconditioner = new DiagonalMatrix(_n);
			_weightsM = new DiagonalMatrix(_n);
			_cx = new double[_n];
			_cy = new double[_n];
			_cz = new double[_n];
			_bx = new double[_n];
			_by = new double[_n];
			_bz = new double[_n];
			_sx = new double[_n];
			_sy = new double[_n];
			_sz = new double[_n];

			_need_solve_update = true;
			UpdateForSolve();
		}




		void UpdateForSolve() {
			if (_need_solve_update == false) {
				return;
			}

			// construct constraints matrix and RHS
			_weightsM.Clear();
			Array.Clear(_cx, 0, _n);
			Array.Clear(_cy, 0, _n);
			Array.Clear(_cz, 0, _n);
			foreach (var constraint in _softConstraints) {
				var vid = constraint.Key;
				var i = _toIndex[vid];
				var w = constraint.Value.Weight;

				if (UseSoftConstraintNormalEquations) {
					w *= w;
				}

				_weightsM.Set(i, i, w);
				var pos = constraint.Value.Position;
				_cx[i] = w * pos.x;
				_cy[i] = w * pos.y;
				_cz[i] = w * pos.z;
			}

			// add RHS vectors
			for (var i = 0; i < _n; ++i) {
				_bx[i] = _mLx[i] + _cx[i];
				_by[i] = _mLy[i] + _cy[i];
				_bz[i] = _mLz[i] + _cz[i];
			}

			// update basic preconditioner
			// [RMS] currently not using this...it actually seems to make things worse!! 
			for (var i = 0; i < _n; i++) {
				var diag_value = _packedM[i, i] + _weightsM[i, i];
				_preconditioner.Set(i, i, 1.0 / diag_value);
			}

			_need_solve_update = false;
		}



		// Result must be as large as Mesh.MaxVertexID
		public bool SolveMultipleCG(Vector3d[] Result) {
			if (_weightsM == null) {
				Initialize();       // force initialize...
			}

			UpdateForSolve();

			// use initial positions as initial solution. 
			Array.Copy(_px, _sx, _n);
			Array.Copy(_py, _sy, _n);
			Array.Copy(_pz, _sz, _n);


			void CombinedMultiply(double[] X, double[] B) {
				//PackedM.Multiply(X, B);
				_packedM.Multiply_Parallel(X, B);

				for (var i = 0; i < _n; ++i) {
					B[i] += _weightsM[i, i] * X[i];
				}
			}

			var Solvers = new List<SparseSymmetricCG>();
			if (SolveX) {
				Solvers.Add(new SparseSymmetricCG() {
					B = _bx,
					X = _sx,
					MultiplyF = CombinedMultiply,
					PreconditionMultiplyF = _preconditioner.Multiply,
					UseXAsInitialGuess = true
				});
			}
			if (SolveY) {
				Solvers.Add(new SparseSymmetricCG() {
					B = _by,
					X = _sy,
					MultiplyF = CombinedMultiply,
					PreconditionMultiplyF = _preconditioner.Multiply,
					UseXAsInitialGuess = true
				});
			}
			if (SolveZ) {
				Solvers.Add(new SparseSymmetricCG() {
					B = _bz,
					X = _sz,
					MultiplyF = CombinedMultiply,
					PreconditionMultiplyF = _preconditioner.Multiply,
					UseXAsInitialGuess = true
				});
			}
			var ok = new bool[Solvers.Count];

			GParallel.ForEach(Interval1i.Range(Solvers.Count), (i) => ok[i] = Solvers[i].Solve());

			ConvergeFailed = false;
			foreach (var b in ok) {
				if (b == false) {
					ConvergeFailed = true;
				}
			}

			for (var i = 0; i < _n; ++i) {
				var vid = _toCurveV[i];
				Result[vid] = new Vector3d(_sx[i], _sy[i], _sz[i]);
			}

			// apply post-fixed constraints
			if (_havePostFixedConstraints) {
				foreach (var constraint in _softConstraints) {
					if (constraint.Value.PostFix) {
						var vid = constraint.Key;
						Result[vid] = constraint.Value.Position;
					}
				}
			}

			return true;
		}




		// Result must be as large as Mesh.MaxVertexID
		public bool SolveMultipleRHS(Vector3d[] Result) {
			if (_weightsM == null) {
				Initialize();       // force initialize...
			}

			UpdateForSolve();

			// use initial positions as initial solution. 
			var B = BufferUtil.InitNxM(3, _n, new double[][] { _bx, _by, _bz });
			var X = BufferUtil.InitNxM(3, _n, new double[][] { _px, _py, _pz });

			void CombinedMultiply(double[][] Xt, double[][] Bt) {
				_packedM.Multiply_Parallel_3(Xt, Bt);
				GParallel.ForEach(Interval1i.Range(3), (j) => BufferUtil.MultiplyAdd(Bt[j], _weightsM.D, Xt[j]));
			}

			var Solver = new SparseSymmetricCGMultipleRHS() {
				B = B,
				X = X,
				MultiplyF = CombinedMultiply,
				PreconditionMultiplyF = null,
				UseXAsInitialGuess = true
			};

			var ok = Solver.Solve();

			if (ok == false) {
				return false;
			}

			for (var i = 0; i < _n; ++i) {
				var vid = _toCurveV[i];
				Result[vid] = new Vector3d(X[0][i], X[1][i], X[2][i]);
			}

			// apply post-fixed constraints
			if (_havePostFixedConstraints) {
				foreach (var constraint in _softConstraints) {
					if (constraint.Value.PostFix) {
						var vid = constraint.Key;
						Result[vid] = constraint.Value.Position;
					}
				}
			}

			return true;
		}





		public bool Solve(Vector3d[] Result) {
			// for small problems, faster to use separate CGs?
			return Curve.VertexCount < 10000 ? SolveMultipleCG(Result) : SolveMultipleRHS(Result);
		}



		public bool SolveAndUpdateCurve() {
			var N = Curve.VertexCount;
			var Result = new Vector3d[N];
			if (Solve(Result) == false) {
				return false;
			}

			for (var i = 0; i < N; ++i) {
				Curve[i] = Result[i];
			}
			return true;
		}



	}
}
