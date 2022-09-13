using System;
using System.Collections.Generic;

namespace RNumerics
{

	public static class CurveSampler2
	{
		public static VectorArray2d AutoSample(in IParametricCurve2d curve, in double fSpacingLength, in double fSpacingT) {
			return curve is ParametricCurveSequence2
				? AutoSample(curve as ParametricCurveSequence2, fSpacingLength, fSpacingT)
				: curve.HasArcLength
				? curve is NURBSCurve2 ? SampleNURBSHybrid(curve as NURBSCurve2, fSpacingLength) : SampleArcLen(curve, fSpacingLength)
				: SampleT(curve, fSpacingT);
		}



		public static VectorArray2d SampleT(in IParametricCurve2d curve, in int N) {
			var fLenT = curve.ParamLength;
			var vec = new VectorArray2d(N);
			var divide = curve.IsClosed ? (double)N : (double)(N - 1);
			for (var i = 0; i < N; ++i) {
				var t = (double)i / divide;
				vec[i] = curve.SampleT(t * fLenT);
			}
			return vec;
		}


		public static VectorArray2d SampleTRange(in IParametricCurve2d curve, in int N, in double t0, in double t1) {
			var vec = new VectorArray2d(N);
			for (var i = 0; i < N; ++i) {
				var alpha = (double)i / (double)(N - 1);
				var t = ((1 - alpha) * t0) + (alpha * t1);
				vec[i] = curve.SampleT(t);
			}
			return vec;
		}


		public static VectorArray2d SampleT(in IParametricCurve2d curve, in double fSpacing) {
			var fLenT = curve.ParamLength;

			var nSteps = Math.Max((int)(fLenT / fSpacing) + 1, 2);

			var vec = new VectorArray2d(nSteps);

			for (var i = 0; i < nSteps; ++i) {
				var t = (double)i / (double)(nSteps - 1);
				vec[i] = curve.SampleT(t * fLenT);
			}

			return vec;
		}


		public static VectorArray2d SampleArcLen(in IParametricCurve2d curve, in double fSpacing) {
			if (curve.HasArcLength == false) {
				throw new InvalidOperationException("CurveSampler2.SampleArcLen: curve does not support arc length sampling!");
			}

			var fLen = curve.ArcLength;
			if (fLen < MathUtil.ZERO_TOLERANCE) {
				var degen = new VectorArray2d(2);
				degen[0] = curve.SampleArcLength(0);
				degen[1] = curve.SampleArcLength(1);
				return degen;
			}
			var nSteps = Math.Max((int)(fLen / fSpacing) + 1, 2);

			var vec = new VectorArray2d(nSteps);

			for (var i = 0; i < nSteps; ++i) {
				var t = (double)i / (double)(nSteps - 1);
				vec[i] = curve.SampleArcLength(t * fLen);
			}

			return vec;
		}


		// special case nurbs sampler. Computes a separate sampling of each unique knot interval
		// of the curve parameter space. Reasoning:
		//   1) computing Arc Length of an entire nurbs curve is quite slow if the curve has
		//      repeated knots. these become discontinuities which mean the numerical integrator
		//      has to do a lot of work. Instead we integrate between the discontinuities.
		//   2) by sampling per-knot-interval, we ensure we always place a sample at each knot
		//      value. If we don't do this, we can "miss" the sharp corners at duplicate knots.
		//   3) within each interval, we compute arc length and # of steps, but then sample
		//      by subdividing the T-interval. This is not precise arc-length sampling but
		//      is closer than uniform-T along the curve. And it means we don't have to
		//      do an arc-length evaluation for each point, which is very expensive!!
		public static VectorArray2d SampleNURBSHybrid(in NURBSCurve2 curve, in double fSpacing) {
			var intervals = curve.GetParamIntervals();
			var N = intervals.Count - 1;

			var spans = new VectorArray2d[N];
			var nTotal = 0;

			for (var i = 0; i < N; ++i) {
				var t0 = intervals[i];
				var t1 = intervals[i + 1];
				var fLen = curve.GetLength(t0, t1);

				var nSteps = Math.Max((int)(fLen / fSpacing) + 1, 2);
				var div = 1.0 / nSteps;
				if (curve.IsClosed == false && i == N - 1) {
					nSteps++;
					div = 1.0 / (nSteps - 1);
				}

				var vec = new VectorArray2d(nSteps);
				for (var j = 0; j < nSteps; ++j) {
					var a = (double)j * div;
					var t = ((1 - a) * t0) + (a * t1);
					vec[j] = curve.SampleT(t);
				}
				spans[i] = vec;
				nTotal += nSteps;
			}

			var final = new VectorArray2d(nTotal);
			var iStart = 0;
			for (var i = 0; i < N; ++i) {
				final.Set(iStart, spans[i].Count, spans[i]);
				iStart += spans[i].Count;
			}

			return final;
		}



		public static VectorArray2d AutoSample(in ParametricCurveSequence2 curves, in double fSpacingLength, in double fSpacingT) {
			var N = curves.Count;
			var bClosed = curves.IsClosed;

			var vecs = new VectorArray2d[N];
			var i = 0;
			var nTotal = 0;
			foreach (var c in curves.Curves) {
				vecs[i] = AutoSample(c, fSpacingLength, fSpacingT);
				nTotal += vecs[i].Count;
				i++;
			}

			var nDuplicates = bClosed ? N : N - 1;        // handle closed here...
			nTotal -= nDuplicates;

			var final = new VectorArray2d(nTotal);

			var k = 0;
			for (var vi = 0; vi < N; ++vi) {
				var vv = vecs[vi];
				// skip final vertex unless we are on last curve (because it is
				// the same as first vertex of next curve)
				var nStop = (bClosed || vi < N - 1) ? vv.Count - 1 : vv.Count;
				final.Set(k, nStop, vv);
				k += nStop;
			}

			return final;
		}
	}
}
