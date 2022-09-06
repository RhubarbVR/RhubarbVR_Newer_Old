using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace RNumerics
{
	/// <summary>
	/// Basic sparse-symmetric-matrix class. Stores upper-triangular portion.
	/// Uses Dictionary as sparsifying data structure, which is probably
	/// not a good option. But it is easy.
	/// </summary>
	public class SymmetricSparseMatrix : IMatrix
	{
		readonly Dictionary<Index2i, double> _d = new();

		public SymmetricSparseMatrix(int setN = 0) {
			Rows = setN;
		}

		public SymmetricSparseMatrix(DenseMatrix m) {
			if (m.Rows != m.Columns) {
				throw new Exception("SymmetricSparseMatrix(DenseMatrix): Matrix is not square!");
			}

			if (m.IsSymmetric() == false) {
				throw new Exception("SymmetricSparseMatrix(DenseMatrix): Matrix is not symmetric!");
			}

			Rows = m.Rows;
			for (var i = 0; i < Rows; ++i) {
				for (var j = i; j < Rows; ++j) {
					Set(i, j, m[i, j]);
				}
			}
		}


		public SymmetricSparseMatrix(SymmetricSparseMatrix m) {
			Rows = m.Rows;
			_d = new Dictionary<Index2i, double>(m._d);
		}


		public void Set(int r, int c, double value) {
			var v = new Index2i(Math.Min(r, c), Math.Max(r, c));
			_d[v] = value;
			if (r >= Rows) {
				Rows = r + 1;
			}

			if (c >= Rows) {
				Rows = c + 1;
			}
		}


		public int Rows { get; private set; }
		public int Columns => Rows;
		public Index2i Size => new(Rows, Rows);


		public double this[int r, int c]
		{
			get {
				var v = new Index2i(Math.Min(r, c), Math.Max(r, c));
				return _d.TryGetValue(v, out var value) ? value : 0;
			}
			set => Set(r, c, value);
		}


		public void Multiply(double[] X, double[] Result) {
			Array.Clear(Result, 0, Result.Length);

			foreach (var v in _d) {
				var i = v.Key.a;
				var j = v.Key.b;
				Result[i] += v.Value * X[j];
				if (i != j) {
					Result[j] += v.Value * X[i];
				}
			}
		}



		// returns this*this (requires less memory)
		public SymmetricSparseMatrix Square(bool bParallel = true) {
			var R = new SymmetricSparseMatrix();
			var M = new PackedSparseMatrix(this);
			M.Sort();

			// Parallel variant is vastly faster, uses spinlock to control access to R
			if (bParallel) {

				// goddamn SpinLock is in .Net 4
				//SpinLock spin = new SpinLock();
				GParallel.ForEach(Interval1i.Range(Rows), (r1i) => {
					for (var c2i = r1i; c2i < Rows; c2i++) {
						var v = M.DotRowColumn(r1i, c2i, M);
						if (Math.Abs(v) > MathUtil.ZERO_TOLERANCE) {
							//bool taken = false;
							//spin.Enter(ref taken);
							//Debug.Assert(taken);
							//R[r1i, c2i] = v;
							//spin.Exit();
							lock (R) {
								R[r1i, c2i] = v;
							}
						}
					}
				});

			}
			else {
				for (var r1i = 0; r1i < Rows; r1i++) {
					for (var c2i = r1i; c2i < Rows; c2i++) {
						var v = M.DotRowColumn(r1i, c2i, M);
						if (Math.Abs(v) > MathUtil.ZERO_TOLERANCE) {
							R[r1i, c2i] = v;
						}
					}
				}
			}

			return R;
		}







		/// <summary>
		/// Returns this*this, as a packed sparse matrix. Computes in parallel.
		/// </summary>
		public PackedSparseMatrix SquarePackedParallel() {
			var M = new PackedSparseMatrix(this);
			M.Sort();
			return M.Square();
		}





		public SymmetricSparseMatrix Multiply(SymmetricSparseMatrix M2) {
			var R = new SymmetricSparseMatrix();
			Multiply(M2, ref R);
			return R;
		}
		public void Multiply(SymmetricSparseMatrix M2, ref SymmetricSparseMatrix R, bool bParallel = true) {
			// testing code
			//multiply_slow(M2, ref R);
			//SymmetricSparseMatrix R2 = new SymmetricSparseMatrix();
			//multiply_fast(M2, ref R2);
			//Debug.Assert(R.EpsilonEqual(R2));

			Multiply_fast(M2, ref R, bParallel);
		}


		/// <summary>
		/// Construct packed versions of input matrices, and then use sparse row/column dot
		/// to compute elements of output matrix. This is faster. But still relatively expensive.
		/// </summary>
		void Multiply_fast(SymmetricSparseMatrix M2in, ref SymmetricSparseMatrix Rin, bool bParallel) {
			var N = Rows;
			if (M2in.Rows != N) {
				throw new Exception("SymmetricSparseMatrix.Multiply: matrices have incompatible dimensions");
			}

			Rin ??= new SymmetricSparseMatrix();

			var R = Rin;      // require alias for use in lambda below

			var M = new PackedSparseMatrix(this);
			M.Sort();
			var M2 = new PackedSparseMatrix(M2in, true);
			M2.Sort();

			// Parallel variant is vastly faster, uses spinlock to control access to R
			if (bParallel) {

				// goddamn SpinLock is in .Net 4
				//SpinLock spin = new SpinLock();
				GParallel.ForEach(Interval1i.Range(N), (r1i) => {
					for (var c2i = r1i; c2i < N; c2i++) {
						var v = M.DotRowColumn(r1i, c2i, M2);
						if (Math.Abs(v) > MathUtil.ZERO_TOLERANCE) {
							//bool taken = false;
							//spin.Enter(ref taken);
							//Debug.Assert(taken);
							//R[r1i, c2i] = v;
							//spin.Exit();
							lock (R) {
								R[r1i, c2i] = v;
							}
						}
					}
				});

			}
			else {

				for (var r1i = 0; r1i < N; r1i++) {
					for (var c2i = r1i; c2i < N; c2i++) {
						var v = M.DotRowColumn(r1i, c2i, M2);
						if (Math.Abs(v) > MathUtil.ZERO_TOLERANCE) {
							R[r1i, c2i] = v;
						}
					}
				}
			}
		}


		public IEnumerable<KeyValuePair<Index2i, double>> NonZeros() {
			return _d;
		}
		public IEnumerable<Index2i> NonZeroIndices() {
			return _d.Keys;
		}


		public bool EpsilonEqual(SymmetricSparseMatrix B, double eps = MathUtil.EPSILON) {
			foreach (var val in _d) {
				if (Math.Abs(B[val.Key.a, val.Key.b] - val.Value) > eps) {
					return false;
				}
			}
			foreach (var val in B._d) {
				if (Math.Abs(this[val.Key.a, val.Key.b] - val.Value) > eps) {
					return false;
				}
			}
			return true;
		}
	}






	public class DiagonalMatrix
	{
		public double[] D;

		public DiagonalMatrix(int N) {
			D = new double[N];
		}

		public void Clear() {
			Array.Clear(D, 0, D.Length);
		}

		public void Set(int r, int c, double value) {
			D[r] = r == c ? value : throw new Exception("DiagonalMatrix.Set: tried to set off-diagonal entry!");
		}


		public int Rows => D.Length;
		public int Columns => D.Length;
		public Index2i Size => new(D.Length, D.Length);


		public double this[int r, int c]
		{
			get {
				Debug.Assert(r == c);
				return D[r];
			}
			set => Set(r, c, value);
		}


		public void Multiply(double[] X, double[] Result) {
			//Array.Clear(Result, 0, Result.Length);
			for (var i = 0; i < X.Length; ++i) {
				//Result[i] += d[i] * X[i];
				Result[i] = D[i] * X[i];
			}
		}
	}

}
