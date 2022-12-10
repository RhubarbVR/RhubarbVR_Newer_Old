using System;

namespace RNumerics
{
	// ported from WildMagic5 Wm5LinearSystem.cpp
	public sealed class SparseSymmetricCG
	{
		// Compute B = A*X, where inputs are ordered <X,B>
		public Action<double[], double[]> MultiplyF;

		// Compute B = A*X, where inputs are ordered <X,B>
		public Action<double[], double[]> PreconditionMultiplyF;

		// B is not modified!
		public double[] B;

		// X will be used as initial guess if non-null and UseXAsInitialGuess is true
		// After Solve(), solution will be available in X
		public double[] X;
		public bool UseXAsInitialGuess = true;

		public int MaxIterations = 1024;
		public int Iterations;

		// internal
		double[] _r, _p, _aP, _z;




		public bool Solve()
		{
			Iterations = 0;
			var size = B.Length;

			// Based on the algorithm in "Matrix Computations" by Golum and Van Loan.
			_r = new double[size];
			_p = new double[size];
			_aP = new double[size];

			if (X == null || UseXAsInitialGuess == false)
			{
				X ??= new double[size];

				Array.Clear(X, 0, X.Length);
				Array.Copy(B, _r, B.Length);
			}
			else
			{
				// hopefully is X is a decent initialization...
				InitializeR(_r);
			}

			// [RMS] these were inside loop but they are constant!
			var norm = BufferUtil.Dot(B, B);
			var root1 = Math.Sqrt(norm);

			// The first iteration. 
			var rho0 = BufferUtil.Dot(_r, _r);

			// [RMS] If we were initialized w/ constraints already satisfied, 
			//   then we are done! (happens for example in mesh deformations)
			if (rho0 < MathUtil.ZERO_TOLERANCE * root1) {
				return true;
			}

			Array.Copy(_r, _p, _r.Length);

			MultiplyF(_p, _aP);

			var alpha = rho0 / BufferUtil.Dot(_p, _aP);
			BufferUtil.MultiplyAdd(X, alpha, _p);
			BufferUtil.MultiplyAdd(_r, -alpha, _aP);
			var rho1 = BufferUtil.Dot(_r, _r);

			// The remaining iterations.
			int iter;
			for (iter = 1; iter < MaxIterations; ++iter)
			{
				var root0 = Math.Sqrt(rho1);
				if (root0 <= MathUtil.ZERO_TOLERANCE * root1)
				{
					break;
				}

				var beta = rho1 / rho0;
				UpdateP(_p, beta, _r);

				MultiplyF(_p, _aP);

				alpha = rho1 / BufferUtil.Dot(_p, _aP);

				// can compute these two steps simultaneously
				double RdotR = 0;
				GParallel.Evaluate(
					() => BufferUtil.MultiplyAdd(X, alpha, _p),
					() => RdotR = BufferUtil.MultiplyAdd_GetSqrSum(_r, -alpha, _aP));

				rho0 = rho1;
				rho1 = RdotR; // BufferUtil.Dot(R, R);
			}

			//System.Console.WriteLine("{0} iterations", iter);
			Iterations = iter;
			return iter < MaxIterations;
		}

		static void UpdateP(in double[] P,in double beta,in double[] R)
		{
			for (var i = 0; i < P.Length; ++i) {
				P[i] = R[i] + (beta * P[i]);
			}
		}


		void InitializeR(in double[] R)
		{
			// R = B - A*X
			MultiplyF(X, R);
			for (var i = 0; i < X.Length; ++i) {
				R[i] = B[i] - R[i];
			}
		}







		public bool SolvePreconditioned()
		{
			Iterations = 0;
			var n = B.Length;

			_r = new double[n];
			_p = new double[n];
			_aP = new double[n];
			_z = new double[n];

			if (X == null || UseXAsInitialGuess == false)
			{
				X ??= new double[n];

				Array.Clear(X, 0, X.Length);
				Array.Copy(B, _r, B.Length);
			}
			else
			{
				// hopefully is X is a decent initialization...
				InitializeR(_r);
			}

			// [RMS] for convergence test?
			var norm = BufferUtil.Dot(B, B);
			var root1 = Math.Sqrt(norm);

			// r_0 = b - A*x_0
			MultiplyF(X, _r);
			for (var i = 0; i < n; ++i) {
				_r[i] = B[i] - _r[i];
			}

			// z0 = M_inverse * r_0
			PreconditionMultiplyF(_r, _z);

			// p0 = z0
			Array.Copy(_z, _p, n);

			var RdotZ_k = BufferUtil.Dot(_r, _z);

			var iter = 0;
			while (iter++ < MaxIterations)
			{

				// convergence test
				var root0 = Math.Sqrt(RdotZ_k);
				if (root0 <= MathUtil.ZERO_TOLERANCE * root1)
				{
					break;
				}

				MultiplyF(_p, _aP);
				var alpha_k = RdotZ_k / BufferUtil.Dot(_p, _aP);

				GParallel.Evaluate(
					// x_k+1 = x_k + alpha_k * p_k
					() => BufferUtil.MultiplyAdd(X, alpha_k, _p),
					// r_k+1 = r_k - alpha_k * A * p_k
					() => BufferUtil.MultiplyAdd(_r, -alpha_k, _aP));

				// z_k+1 = M_inverse * r_k+1
				PreconditionMultiplyF(_r, _z);

				// beta_k = (z_k+1 * r_k+1) / (z_k * r_k)
				var beta_k = BufferUtil.Dot(_z, _r) / RdotZ_k;

				GParallel.Evaluate(
					// p_k+1 = z_k+1 + beta_k * p_k
					() =>
					{
						for (var i = 0; i < n; ++i) {
							_p[i] = _z[i] + (beta_k * _p[i]);
						}
					},
					() => RdotZ_k = BufferUtil.Dot(_r, _z));
			}


			//System.Console.WriteLine("{0} iterations", iter);
			Iterations = iter;
			return iter < MaxIterations;
		}


	}











	/// <summary>
	/// [RMS] this is a variant of SparseSymmetricCG that supports multiple right-hand-sides.
	/// Makes quite a big difference as matrix gets bigger, because MultiplyF can
	/// unroll inner loops (as long as you actually do that)
	/// 
	/// However, if this is done then it is not really possible to do different numbers
	/// of iterations for different RHS's. We will not update that RHS once it has 
	/// converged, however we still have to do the multiplies!
	/// 
	/// </summary>
	public sealed class SparseSymmetricCGMultipleRHS
	{
		// Compute B = A*X, where inputs are ordered <X,B>
		public Action<double[][], double[][]> MultiplyF;

		public Action<double[][], double[][]> PreconditionMultiplyF;

		// B is not modified!
		public double[][] B;

		public double ConvergeTolerance = MathUtil.ZERO_TOLERANCE;

		// X will be used as initial guess if non-null and UseXAsInitialGuess is true
		// After Solve(), solution will be available in X
		public double[][] X;
		public bool UseXAsInitialGuess = true;

		public int MaxIterations = 1024;
		public int Iterations;

		// internal
		double[][] _r, _p, _w, _aP, _z;


		/// <summary>
		/// standard CG solve
		/// </summary>
		public bool Solve()
		{
			Iterations = 0;
			if (B == null || MultiplyF == null) {
				throw new Exception("SparseSymmetricCGMultipleRHS.Solve(): Must set B and MultiplyF!");
			}

			var NRHS = B.Length;
			if (NRHS == 0) {
				throw new Exception("SparseSymmetricCGMultipleRHS.Solve(): Need at least one RHS vector in B");
			}

			var size = B[0].Length;

			// Based on the algorithm in "Matrix Computations" by Golum and Van Loan.
			_r = BufferUtil.AllocNxM(NRHS, size);
			_p = BufferUtil.AllocNxM(NRHS, size);
			_w = BufferUtil.AllocNxM(NRHS, size);

			if (X == null || UseXAsInitialGuess == false)
			{
				X ??= BufferUtil.AllocNxM(NRHS, size);

				for (var j = 0; j < NRHS; ++j)
				{
					Array.Clear(X[j], 0, size);
					Array.Copy(B[j], _r[j], size);
				}
			}
			else
			{
				// hopefully is X is a decent initialization...
				InitializeR(_r);
			}

			// [RMS] these were inside loop but they are constant!
			var norm = new double[NRHS];
			for (var j = 0; j < NRHS; ++j) {
				norm[j] = BufferUtil.Dot(B[j], B[j]);
			}

			var root1 = new double[NRHS];
			for (var j = 0; j < NRHS; ++j) {
				root1[j] = Math.Sqrt(norm[j]);
			}

			// The first iteration. 
			var rho0 = new double[NRHS];
			for (var j = 0; j < NRHS; ++j) {
				rho0[j] = BufferUtil.Dot(_r[j], _r[j]);
			}

			// [RMS] If we were initialized w/ constraints already satisfied, 
			//   then we are done! (happens for example in mesh deformations)
			var converged = new bool[NRHS];
			var nconverged = 0;
			for (var j = 0; j < NRHS; ++j)
			{
				converged[j] = rho0[j] < (ConvergeTolerance * root1[j]);
				if (converged[j]) {
					nconverged++;
				}
			}
			if (nconverged == NRHS) {
				return true;
			}

			for (var j = 0; j < NRHS; ++j) {
				Array.Copy(_r[j], _p[j], size);
			}

			MultiplyF(_p, _w);

			var alpha = new double[NRHS];
			for (var j = 0; j < NRHS; ++j) {
				alpha[j] = rho0[j] / BufferUtil.Dot(_p[j], _w[j]);
			}

			for (var j = 0; j < NRHS; ++j) {
				BufferUtil.MultiplyAdd(X[j], alpha[j], _p[j]);
			}

			for (var j = 0; j < NRHS; ++j) {
				BufferUtil.MultiplyAdd(_r[j], -alpha[j], _w[j]);
			}

			var rho1 = new double[NRHS];
			for (var j = 0; j < NRHS; ++j) {
				rho1[j] = BufferUtil.Dot(_r[j], _r[j]);
			}

			var beta = new double[NRHS];

			var rhs = Interval1i.Range(NRHS);

			// The remaining iterations.
			int iter;
			for (iter = 1; iter < MaxIterations; ++iter)
			{

				var done = true;
				for (var j = 0; j < NRHS; ++j)
				{
					if (converged[j] == false)
					{
						var root0 = Math.Sqrt(rho1[j]);
						if (root0 <= ConvergeTolerance * root1[j]) {
							converged[j] = true;
						}
					}
					if (converged[j] == false) {
						done = false;
					}
				}
				if (done) {
					break;
				}

				for (var j = 0; j < NRHS; ++j) {
					beta[j] = rho1[j] / rho0[j];
				}

				UpdateP(_p, beta, _r, converged);

				MultiplyF(_p, _w);

				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false) {
						alpha[j] = rho1[j] / BufferUtil.Dot(_p[j], _w[j]);
					}
				});

				// can do all these in parallel, but improvement is minimal
				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false) {
						BufferUtil.MultiplyAdd(X[j], alpha[j], _p[j]);
					}
				});
				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false)
					{
						rho0[j] = rho1[j];
						rho1[j] = BufferUtil.MultiplyAdd_GetSqrSum(_r[j], -alpha[j], _w[j]);
					}
				});
			}

			//System.Console.WriteLine("{0} iterations", iter);
			Iterations = iter;
			return iter < MaxIterations;
		}





		/// <summary>
		/// Preconditioned variant
		/// Similar to non-preconditioned version, this can suffer if one solution converges
		/// much slower than others, as we can't skip matrix multiplies in that case.
		/// </summary>
		public bool SolvePreconditioned()
		{
			Iterations = 0;
			if (B == null || MultiplyF == null || PreconditionMultiplyF == null) {
				throw new Exception("SparseSymmetricCGMultipleRHS.SolvePreconditioned(): Must set B and MultiplyF and PreconditionMultiplyF!");
			}

			var NRHS = B.Length;
			if (NRHS == 0) {
				throw new Exception("SparseSymmetricCGMultipleRHS.SolvePreconditioned(): Need at least one RHS vector in B");
			}

			var n = B[0].Length;

			_r = BufferUtil.AllocNxM(NRHS, n);
			_p = BufferUtil.AllocNxM(NRHS, n);
			_aP = BufferUtil.AllocNxM(NRHS, n);
			_z = BufferUtil.AllocNxM(NRHS, n);

			if (X == null || UseXAsInitialGuess == false)
			{
				X ??= BufferUtil.AllocNxM(NRHS, n);

				for (var j = 0; j < NRHS; ++j)
				{
					Array.Clear(X[j], 0, n);
					Array.Copy(B[j], _r[j], n);
				}
			}
			else
			{
				// hopefully is X is a decent initialization...
				InitializeR(_r);
			}

			// [RMS] for convergence test?
			var norm = new double[NRHS];
			for (var j = 0; j < NRHS; ++j) {
				norm[j] = BufferUtil.Dot(B[j], B[j]);
			}

			var root1 = new double[NRHS];
			for (var j = 0; j < NRHS; ++j) {
				root1[j] = Math.Sqrt(norm[j]);
			}


			// r_0 = b - A*x_0
			MultiplyF(X, _r);
			for (var j = 0; j < NRHS; ++j)
			{
				for (var i = 0; i < n; ++i) {
					_r[j][i] = B[j][i] - _r[j][i];
				}
			}

			// z0 = M_inverse * r_0
			PreconditionMultiplyF(_r, _z);

			// p0 = z0
			for (var j = 0; j < NRHS; ++j) {
				Array.Copy(_z[j], _p[j], n);
			}

			// compute initial R*Z
			var RdotZ_k = new double[NRHS];
			for (var j = 0; j < NRHS; ++j) {
				RdotZ_k[j] = BufferUtil.Dot(_r[j], _z[j]);
			}

			var alpha_k = new double[NRHS];
			var beta_k = new double[NRHS];
			var converged = new bool[NRHS];
			var rhs = Interval1i.Range(NRHS);

			var iter = 0;
			while (iter++ < MaxIterations)
			{

				// convergence test
				var done = true;
				for (var j = 0; j < NRHS; ++j)
				{
					if (converged[j] == false)
					{
						var root0 = Math.Sqrt(RdotZ_k[j]);
						if (root0 <= ConvergeTolerance * root1[j]) {
							converged[j] = true;
						}
					}
					if (converged[j] == false) {
						done = false;
					}
				}
				if (done) {
					break;
				}

				MultiplyF(_p, _aP);

				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false) {
						alpha_k[j] = RdotZ_k[j] / BufferUtil.Dot(_p[j], _aP[j]);
					}
				});

				// x_k+1 = x_k + alpha_k * p_k
				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false)
					{
						BufferUtil.MultiplyAdd(X[j], alpha_k[j], _p[j]);
					}
				});

				// r_k+1 = r_k - alpha_k * A * p_k
				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false)
					{
						BufferUtil.MultiplyAdd(_r[j], -alpha_k[j], _aP[j]);
					}
				});

				// z_k+1 = M_inverse * r_k+1
				PreconditionMultiplyF(_r, _z);

				// beta_k = (z_k+1 * r_k+1) / (z_k * r_k)
				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false) {
						beta_k[j] = BufferUtil.Dot(_z[j], _r[j]) / RdotZ_k[j];
					}
				});

				// can do these in parallel but improvement is minimal

				// p_k+1 = z_k+1 + beta_k * p_k
				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false)
					{
						for (var i = 0; i < n; ++i) {
							_p[j][i] = _z[j][i] + (beta_k[j] * _p[j][i]);
						}
					}
				});

				GParallel.ForEach(rhs, (j) =>
				{
					if (converged[j] == false)
					{
						RdotZ_k[j] = BufferUtil.Dot(_r[j], _z[j]);
					}
				});
			}


			//System.Console.WriteLine("{0} iterations", iter);
			Iterations = iter;
			return iter < MaxIterations;
		}

		static void UpdateP(double[][] P, double[] beta, double[][] R, bool[] converged)
		{
			var rhs = Interval1i.Range(P.Length);
			GParallel.ForEach(rhs, (j) =>
			{
				if (converged[j] == false)
				{
					var n = P[j].Length;
					for (var i = 0; i < n; ++i) {
						P[j][i] = R[j][i] + (beta[j] * P[j][i]);
					}
				}
			});
		}


		void InitializeR(in double[][] R)
		{
			// R = B - A*X
			MultiplyF(X, R);
			for (var j = 0; j < X.Length; ++j)
			{
				var n = R[j].Length;
				for (var i = 0; i < n; ++i) {
					R[j][i] = B[j][i] - R[j][i];
				}
			}
		}




	}


}
