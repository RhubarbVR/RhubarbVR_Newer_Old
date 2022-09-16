using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RNumerics
{
	public sealed class ParametricCurveSequence2 : IParametricCurve2d, IMultiCurve2d
	{

		List<IParametricCurve2d> _curves;

		public ParametricCurveSequence2() {
			_curves = new List<IParametricCurve2d>();
		}

		public ParametricCurveSequence2(in IEnumerable<IParametricCurve2d> curvesIn, in bool isClosed = true) {
			_curves = new List<IParametricCurve2d>(curvesIn);
			IsClosed = isClosed;
		}

		public int Count => _curves.Count;

		public ReadOnlyCollection<IParametricCurve2d> Curves => _curves.AsReadOnly();

		public bool IsClosed { get; set; }


		public void Append(in IParametricCurve2d c) {
			// sanity checking??
			_curves.Add(c);
		}

		public void Prepend(in IParametricCurve2d c) {
			_curves.Insert(0, c);
		}


		public double ParamLength
		{
			get {
				double sum = 0;
				foreach (var c in Curves) {
					sum += c.ParamLength;
				}

				return sum;
			}
		}

		public Vector2d SampleT(in double t) {
			double sum = 0;
			for (var i = 0; i < Curves.Count; ++i) {
				var l = _curves[i].ParamLength;
				if (t <= sum + l) {
					var ct = t - sum;
					return _curves[i].SampleT(ct);
				}
				sum += l;
			}
			throw new ArgumentException("ParametricCurveSequence2.SampleT: argument out of range");
		}

		public Vector2d TangentT(in double t) {
			double sum = 0;
			for (var i = 0; i < Curves.Count; ++i) {
				var l = _curves[i].ParamLength;
				if (t <= sum + l) {
					var ct = t - sum;
					return _curves[i].TangentT(ct);
				}
				sum += l;
			}
			throw new ArgumentException("ParametricCurveSequence2.SampleT: argument out of range");
		}



		public bool HasArcLength
		{
			get {
				foreach (var c in Curves) {
					if (c.HasArcLength == false) {
						return false;
					}
				}

				return true;
			}
		}

		public double ArcLength
		{
			get {
				double sum = 0;
				foreach (var c in Curves) {
					sum += c.ArcLength;
				}

				return sum;
			}
		}

		public Vector2d SampleArcLength(in double a) {
			double sum = 0;
			for (var i = 0; i < Curves.Count; ++i) {
				var l = _curves[i].ArcLength;
				if (a <= sum + l) {
					var ca = a - sum;
					return _curves[i].SampleArcLength(ca);
				}
				sum += l;
			}
			throw new ArgumentException("ParametricCurveSequence2.SampleArcLength: argument out of range");
		}

		public void Reverse() {
			foreach (var c in _curves) {
				c.Reverse();
			}

			_curves.Reverse();
		}

		public IParametricCurve2d Clone() {
			var s2 = new ParametricCurveSequence2 {
				IsClosed = IsClosed,
				_curves = new List<IParametricCurve2d>()
			};
			foreach (var c in _curves) {
				s2._curves.Add(c.Clone());
			}

			return s2;
		}


		public bool IsTransformable => true;
		public void Transform(in ITransform2 xform) {
			foreach (var c in _curves) {
				c.Transform(xform);
			}
		}

	}
}
