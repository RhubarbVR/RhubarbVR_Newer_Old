using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public sealed class CurveResampler
	{

		double[] _lengths;

		// will return null if no edges need to be split!
		public List<Vector3d> SplitResample(in ISampledCurve3d curve, in double fMaxEdgeLen) {
			var fMaxSqr = fMaxEdgeLen * fMaxEdgeLen;

			var N = curve.VertexCount;
			var Nstop = curve.Closed ? N + 1 : N;
			if (_lengths == null || _lengths.Length < Nstop) {
				_lengths = new double[Nstop];
			}

			var bFoundSplit = false;
			for (var i = 0; i < Nstop; ++i) {
				_lengths[i] = curve.GetVertex(i).DistanceSquared(curve.GetVertex((i + 1) % N));
				if (_lengths[i] > fMaxSqr) {
					bFoundSplit = true;
				}
			}
			if (!bFoundSplit) {
				return null;
			}

			var vNew = new List<Vector3d>();
			var prev = curve.GetVertex(0);
			vNew.Add(prev);
			for (var i = 0; i < Nstop - 1; ++i) {
				var next = curve.GetVertex((i + 1) % N);

				if (_lengths[i] > fMaxSqr) {
					var fLen = Math.Sqrt(_lengths[i]);
					var nSteps = (int)(fLen / fMaxEdgeLen) + 1;
					for (var k = 1; k < nSteps; ++k) {
						var t = (double)k / (double)nSteps;
						var mid = Vector3d.Lerp(prev, next, t);
						vNew.Add(mid);
					}
				}
				vNew.Add(next);
				prev = next;
			}

			return vNew;
		}




		// will return null if no edges need to be split!
		public List<Vector3d> SplitCollapseResample(in ISampledCurve3d curve, in double fMaxEdgeLen, in double fMinEdgeLen) {
			var fMaxSqr = fMaxEdgeLen * fMaxEdgeLen;
			var fMinSqr = fMinEdgeLen * fMinEdgeLen;

			var N = curve.VertexCount;
			var Nstop = curve.Closed ? N + 1 : N;
			if (_lengths == null || _lengths.Length < Nstop) {
				_lengths = new double[Nstop];
			}

			var bFoundSplit = false;
			var bFoundCollapse = false;
			for (var i = 0; i < Nstop - 1; ++i) {
				_lengths[i] = curve.GetVertex(i).DistanceSquared(curve.GetVertex((i + 1) % N));
				if (_lengths[i] > fMaxSqr) {
					bFoundSplit = true;
				}
				else if (_lengths[i] < fMinSqr) {
					bFoundCollapse = true;
				}
			}
			if (bFoundSplit == false && bFoundCollapse == false) {
				return null;
			}

			var vNew = new List<Vector3d>();
			var prev = curve.GetVertex(0);
			vNew.Add(prev);
			double collapse_accum = 0;
			for (var i = 0; i < Nstop - 1; ++i) {
				var next = curve.GetVertex((i + 1) % N);

				// accumulate collapsed edges. if we accumulate past min-edge length,
				// then need to drop a vertex
				if (_lengths[i] < fMinSqr) {
					collapse_accum += Math.Sqrt(_lengths[i]);
					if (collapse_accum > fMinEdgeLen) {
						collapse_accum = 0;
						vNew.Add(next);
					}
					prev = next;
					continue;
				}

				// if we have been accumulating collapses, then we need to
				// drop a new vertex  (todo: is this right? shouldn't we just
				//   continue from previous?)
				if (collapse_accum > 0) {
					vNew.Add(prev);
					collapse_accum = 0;
				}

				// split edge if it is too long
				if (_lengths[i] > fMaxSqr) {
					var fLen = Math.Sqrt(_lengths[i]);
					var nSteps = (int)(fLen / fMaxEdgeLen) + 1;
					for (var k = 1; k < nSteps; ++k) {
						var t = (double)k / (double)nSteps;
						var mid = Vector3d.Lerp(prev, next, t);
						vNew.Add(mid);
					}
				}
				vNew.Add(next);
				prev = next;
			}

			return vNew;
		}



	}
}
