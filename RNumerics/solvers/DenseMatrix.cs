using System;


namespace RNumerics
{
	/// <summary>
	/// Row-major dense matrix
	/// </summary>
	public sealed class DenseMatrix : IMatrix
	{
		public DenseMatrix(in int Nrows, in int Mcols) {
			Buffer = new double[Nrows * Mcols];
			Array.Clear(Buffer, 0, Buffer.Length);
			Rows = Nrows;
			Columns = Mcols;
		}
		public DenseMatrix(in DenseMatrix copy) {
			Rows = copy.Rows;
			Columns = copy.Columns;
			Buffer = new double[Rows * Columns];
			// is there a more efficient way to do this?!?
			Array.Copy(copy.Buffer, Buffer, copy.Buffer.Length);
		}

		public double[] Buffer { get; private set; }


		public void Set(in int r, in int c, in double value) {
			Buffer[(r * Columns) + c] = value;
		}


		public void Set(in double[] values) {
			if (values.Length != Rows * Columns) {
				throw new Exception("DenseMatrix.Set: incorrect length");
			}

			Array.Copy(values, Buffer, Buffer.Length);
		}


		public int Rows { get; private set; }
		public int Columns { get; private set; }
		public Index2i Size => new(Rows, Columns);

		public int Length => Columns * Rows;

		public double this[in int r, in int c]
		{
			get => Buffer[(r * Columns) + c];
			set => Buffer[(r * Columns) + c] = value;
		}
		public double this[in int i]
		{
			get => Buffer[i];
			set => Buffer[i] = value;
		}


		public DenseVector Row(in int r) {
			var row = new DenseVector(Columns);
			var ii = r * Columns;
			for (var i = 0; i < Columns; ++i) {
				row[i] = Buffer[ii + i];
			}

			return row;
		}
		public DenseVector Column(in int c) {
			var col = new DenseVector(Rows);
			for (var i = 0; i < Rows; ++i) {
				col[i] = Buffer[(i * Columns) + c];
			}

			return col;
		}

		public DenseVector Diagonal() {
			if (Columns != Rows) {
				throw new Exception("DenseMatrix.Diagonal: matrix is not square!");
			}

			var diag = new DenseVector(Rows);
			for (var i = 0; i < Rows; ++i) {
				diag[i] = Buffer[(i * Columns) + i];
			}

			return diag;
		}


		public DenseMatrix Transpose() {
			var t = new DenseMatrix(Columns, Rows);
			for (var r = 0; r < Rows; ++r) {
				for (var c = 0; c < Columns; ++c) {
					t.Buffer[(c * Columns) + r] = Buffer[(r * Columns) + c];
				}
			}
			return t;
		}

		public void TransposeInPlace() {
			if (Rows != Columns) {
				// [TODO]: do not need to make new matrix for this case anymore...right?
				var d2 = new double[Columns * Rows];
				for (var r = 0; r < Rows; ++r) {
					for (var c = 0; c < Columns; ++c) {
						d2[(c * Columns) + r] = Buffer[(r * Columns) + c];
					}
				}
				Buffer = d2;
				(Rows, Columns) = (Columns, Rows);
			}
			else {
				for (var r = 0; r < Rows; ++r) {
					for (var c = 0; c < Columns; ++c) {
						if (c != r) {
							int i0 = (r * Columns) + c, i1 = (c * Columns) + r;
							(Buffer[i1], Buffer[i0]) = (Buffer[i0], Buffer[i1]);
						}
					}
				}
			}
		}




		public bool IsSymmetric(in double dTolerance = MathUtil.EPSILON) {
			if (Columns != Rows) {
				throw new Exception("DenseMatrix.IsSymmetric: matrix is not square!");
			}

			for (var i = 0; i < Rows; ++i) {
				for (var j = 0; j < i; ++j) {
					if (Math.Abs(Buffer[(i * Columns) + j] - Buffer[(j * Columns) + i]) > dTolerance) {
						return false;
					}
				}
			}
			return true;
		}


		public bool IsPositiveDefinite() {
			if (Columns != Rows) {
				throw new Exception("DenseMatrix.IsPositiveDefinite: matrix is not square!");
			}

			if (IsSymmetric() == false) {
				throw new Exception("DenseMatrix.IsPositiveDefinite: matrix is not symmetric!");
			}

			for (var i = 0; i < Rows; ++i) {
				var diag = Buffer[(i * Columns) + i];
				double row_sum = 0;
				for (var j = 0; j < Rows; ++j) {
					if (j != i) {
						row_sum += Math.Abs(Buffer[(i * Columns) + j]);
					}
				}
				if (diag < 0 || diag < row_sum) {
					return false;
				}
			}
			return true;
		}



		public bool EpsilonEquals(in DenseMatrix m2, in double epsilon = MathUtil.ZERO_TOLERANCE) {
			if (Rows != m2.Rows || Columns != m2.Columns) {
				throw new Exception("DenseMatrix.Equals: matrices are not the same size!");
			}

			for (var i = 0; i < Buffer.Length; ++i) {
				if (Math.Abs(Buffer[i] - m2.Buffer[i]) > epsilon) {
					return false;
				}
			}
			return true;
		}




		public DenseVector Multiply(in DenseVector X) {
			var R = new DenseVector(X.Length);
			Multiply(X.Buffer, R.Buffer);
			return R;
		}
		public void Multiply(in DenseVector X, in DenseVector R) {
			Multiply(X.Buffer, R.Buffer);
		}
		public void Multiply(in double[] X, in double[] Result) {
			for (var i = 0; i < Rows; ++i) {
				Result[i] = 0;
				var ii = i * Columns;
				for (var j = 0; j < Columns; ++j) {
					Result[i] += Buffer[ii + j] * X[j];
				}
			}
		}



		public void Add(in DenseMatrix M2) {
			if (Rows != M2.Rows || Columns != M2.Columns) {
				throw new Exception("DenseMatrix.Add: matrices have incompatible dimensions");
			}

			for (var i = 0; i < Buffer.Length; ++i) {
				Buffer[i] += M2.Buffer[i];
			}
		}
		public void Add(in IMatrix M2) {
			if (Rows != M2.Rows || Columns != M2.Columns) {
				throw new Exception("DenseMatrix.Add: matrices have incompatible dimensions");
			}

			for (var ri = 0; ri < Rows; ++ri) {
				for (var ci = 0; ci < Columns; ++ci) {
					Buffer[(ri * Columns) + ci] += M2[ri, ci];
				}
			}
		}


		public void MulAdd(in DenseMatrix M2, in double s) {
			if (Rows != M2.Rows || Columns != M2.Columns) {
				throw new Exception("DenseMatrix.MulAdd: matrices have incompatible dimensions");
			}

			for (var i = 0; i < Buffer.Length; ++i) {
				Buffer[i] += s * M2.Buffer[i];
			}
		}
		public void MulAdd(in IMatrix M2, in double s) {
			if (Rows != M2.Rows || Columns != M2.Columns) {
				throw new Exception("DenseMatrix.MulAdd: matrices have incompatible dimensions");
			}

			for (var ri = 0; ri < Rows; ++ri) {
				for (var ci = 0; ci < Columns; ++ci) {
					Buffer[(ri * Columns) + ci] += s * M2[ri, ci];
				}
			}
		}



		public DenseMatrix Multiply(in DenseMatrix M2, in bool bParallel = true) {
			var R = new DenseMatrix(Rows, M2.Columns);
			Multiply(M2, ref R, bParallel);
			return R;
		}
		public void Multiply(DenseMatrix M2, ref DenseMatrix R, in bool bParallel = true) {
			int rows1 = Rows, cols1 = Columns;
			int rows2 = M2.Rows, cols2 = M2.Columns;

			if (cols1 != rows2) {
				throw new Exception("DenseMatrix.Multiply: matrices have incompatible dimensions");
			}

			R ??= new DenseMatrix(Rows, M2.Columns);

			if (R.Rows != rows1 || R.Columns != cols2) {
				throw new Exception("DenseMatrix.Multiply: Result matrix has incorrect dimensions");
			}

			if (bParallel) {
				var Rt = R;
				GParallel.ForEach(Interval1i.Range(0, rows1), (r1i) => {
					var ii = r1i * Columns;
					for (var c2i = 0; c2i < cols2; c2i++) {
						double v = 0;
						for (var k = 0; k < cols1; ++k) {
							v += Buffer[ii + k] * M2.Buffer[(k * Columns) + c2i];
						}

						Rt[ii + c2i] = v;
					}
				});
			}
			else {
				for (var r1i = 0; r1i < rows1; r1i++) {
					var ii = r1i * Columns;
					for (var c2i = 0; c2i < cols2; c2i++) {
						double v = 0;
						for (var k = 0; k < cols1; ++k) {
							v += Buffer[ii + k] * M2.Buffer[(k * Columns) + c2i];
						}

						R[ii + c2i] = 0;
					}
				}
			}

		}

	}

}
