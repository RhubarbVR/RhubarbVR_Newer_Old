using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{

	public sealed class InPlaceIterativeCurveSmooth
	{
		DCurve3 _curve;
		public DCurve3 Curve
		{
			get => _curve;
			set { if (_curve != value) { _curve = value; } }
		}

		public int Start { get; set; }
		public int End { get; set; }

		float _alpha;
		public float Alpha
		{
			get => _alpha;
			set => _alpha = MathUtil.Clamp(value, 0.0f, 1.0f);
		}

		public InPlaceIterativeCurveSmooth() {
			Start = End = -1;
			Alpha = 0.25f;
		}
		public InPlaceIterativeCurveSmooth(in DCurve3 curve, in float alpha = 0.25f) {
			Curve = curve;
			Start = 0;
			End = Curve.VertexCount;
			Alpha = alpha;
		}


		public void UpdateDeformation(in int nIterations = 1) {
			if (Curve.Closed) {
				UpdateDeformation_Closed(nIterations);
			}
			else {
				UpdateDeformation_Open(nIterations);
			}
		}


		public void UpdateDeformation_Closed(in int nIterations = 1) {
			if (Start < 0 || Start > Curve.VertexCount || End > Curve.VertexCount) {
				throw new ArgumentOutOfRangeException(nameof(nIterations));
			}

			var N = Curve.VertexCount;
			for (var iter = 0; iter < nIterations; ++iter) {
				for (var ii = Start; ii < End; ++ii) {
					var i = ii % N;
					var iPrev = (ii == 0) ? N - 1 : ii - 1;
					var iNext = (ii + 1) % N;
					Vector3d prev = Curve[iPrev], next = Curve[iNext];
					var c = (prev + next) * 0.5f;
					Curve[i] = ((1 - Alpha) * Curve[i]) + (Alpha * c);
				}
			}
		}


		public void UpdateDeformation_Open(in int nIterations = 1) {
			if (Start < 0 || Start > Curve.VertexCount || End > Curve.VertexCount) {
				throw new ArgumentOutOfRangeException(nameof(nIterations));
			}

			for (var iter = 0; iter < nIterations; ++iter) {
				for (var i = Start; i <= End; ++i) {
					if (i == 0 || i >= Curve.VertexCount - 1) {
						continue;
					}

					Vector3d prev = Curve[i - 1], next = Curve[i + 1];
					var c = (prev + next) * 0.5f;
					Curve[i] = ((1 - Alpha) * Curve[i]) + (Alpha * c);
				}
			}
		}

	}









	public sealed class ArcLengthSoftTranslation
	{
		DCurve3 _curve;
		public DCurve3 Curve
		{
			get => _curve;
			set { if (_curve != value) { _curve = value; Invalidate_roi(); } }
		}

		// handle is position of deformation handle
		//  (but currently we are finding nearest vertex to this anyway...)
		Vector3d _handle;
		public Vector3d Handle
		{
			get => _handle;
			set { if (_handle != value) { _handle = value; Invalidate_roi(); } }
		}

		// arclength in either direction along curve, from handle, over which deformation falls off
		double _arcradius;
		public double ArcRadius
		{
			get => _arcradius;
			set { if (_arcradius != value) { _arcradius = value; Invalidate_roi(); } }
		}

		// weight function applied over falloff region. currently linear!
		Func<double, double, double> _weightfunc;
		public Func<double, double, double> WeightFunc
		{
			get => _weightfunc;
			set { if (_weightfunc != value) { _weightfunc = value; Invalidate_roi(); } }
		}

		public int[] roi_index;
		public double[] roi_weights;
		public Vector3d[] start_positions;
		bool _roi_valid;
		int _curve_timestamp;

		public ArcLengthSoftTranslation() {
			Handle = Vector3d.Zero;
			ArcRadius = 1.0f;
			WeightFunc = (d, r) => MathUtil.WyvillFalloff01(MathUtil.Clamp(d / r, 0.0, 1.0));
			_roi_valid = false;
		}



		Vector3d _start_handle;

		public Vector3d[] GetStart_positions() {
			return start_positions;
		}

		public void BeginDeformation(Vector3d[] start_positions) {
			UpdateROI(-1);      // will be ignored if you called this yourself first
			_start_handle = Handle;

			if (start_positions == null || start_positions.Length != roi_index.Length) {
				start_positions = new Vector3d[roi_index.Length];
			}

			for (var i = 0; i < roi_index.Length; ++i) {
				start_positions[i] = Curve.GetVertex(roi_index[i]);
			}
		}

		public void UpdateDeformation(in Vector3d newHandlePos) {
			var dv = newHandlePos - _start_handle;
			for (var i = 0; i < roi_index.Length; ++i) {
				var vNew = start_positions[i] + (roi_weights[i] * dv);
				Curve.SetVertex(roi_index[i], vNew);
			}
		}

		void Invalidate_roi() {
			_roi_valid = false;
		}

		bool Check_roi_valid() {
			return _roi_valid != false && Curve.Timestamp == _curve_timestamp;
		}

		public void UpdateROI(in int nNearVertexHint = -1) {
			if (Check_roi_valid()) {
				return;
			}

			var iStart = nNearVertexHint;
			if (nNearVertexHint < 0) {
				iStart = CurveUtils.FindNearestIndex(Curve, Handle);
			}
			var N = Curve.VertexCount;

			// walk forward and backward to figure out how many verts we have in ROI
			var nTotal = 1;

			double cumSumFW = 0;
			var nForward = -1;
			for (var i = iStart + 1; i < N && cumSumFW < ArcRadius; ++i) {
				var d = (Curve.GetVertex(i) - Curve.GetVertex(i - 1)).Length;
				cumSumFW += d;
				if (cumSumFW < ArcRadius) {
					nTotal++;
					nForward = i;
				}
			}
			double cumSumBW = 0;
			var nBack = -1;
			for (var i = iStart - 1; i >= 0 && cumSumBW < ArcRadius; --i) {
				var d = (Curve.GetVertex(i) - Curve.GetVertex(i + 1)).Length;
				cumSumBW += d;
				if (cumSumBW < ArcRadius) {
					nTotal++;
					nBack = i;
				}
			}

			if (roi_index == null || roi_index.Length != nTotal) {
				roi_index = new int[nTotal];
				roi_weights = new double[nTotal];
			}
			var roiI = 0;

			roi_index[roiI] = iStart;
			roi_weights[roiI++] = WeightFunc(0, ArcRadius);

			// now fill roi arrays
			if (nForward >= 0) {
				cumSumFW = 0;
				for (var i = iStart + 1; i <= nForward; ++i) {
					cumSumFW += (Curve.GetVertex(i) - Curve.GetVertex(i - 1)).Length;
					roi_index[roiI] = i;
					roi_weights[roiI++] = WeightFunc(cumSumFW, ArcRadius);
				}
			}
			if (nBack >= 0) {
				cumSumBW = 0;
				for (var i = iStart - 1; i >= nBack; --i) {
					cumSumBW += (Curve.GetVertex(i) - Curve.GetVertex(i + 1)).Length;
					roi_index[roiI] = i;
					roi_weights[roiI++] = WeightFunc(cumSumBW, ArcRadius);
				}
			}

			_roi_valid = true;
			_curve_timestamp = Curve.Timestamp;
		}

	}
}
