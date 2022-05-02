using System;
using System.Collections.Generic;
using System.Linq;

namespace RNumerics
{
	public struct CurveSample
	{
		public Vector3d position;
		public Vector3d tangent;
		public CurveSample(Vector3d p, Vector3d t) {
			position = p;
			tangent = t;
		}
	}

	public interface IArcLengthParam
	{
		double ArcLength { get; }
		CurveSample Sample(double fArcLen);
	}



	public class SampledArcLengthParam : IArcLengthParam
	{
		readonly double[] _arc_len;
		readonly Vector3d[] _positions;

		public SampledArcLengthParam(IEnumerable<Vector3d> samples, int nCountHint = -1) {
			var N = (nCountHint == -1) ? samples.Count() : nCountHint;
			_arc_len = new double[N];
			_arc_len[0] = 0;
			_positions = new Vector3d[N];

			var i = 0;
			Vector3d prev = Vector3f.Zero;
			foreach (var v in samples) {
				_positions[i] = v;
				if (i > 0) {
					var d = (v - prev).Length;
					_arc_len[i] = _arc_len[i - 1] + d;
				}
				i++;
				prev = v;
			}
		}



		public double ArcLength => _arc_len[_arc_len.Length - 1];


		public CurveSample Sample(double f) {
			if (f <= 0) {
				return new CurveSample(new Vector3d(_positions[0]), Tangent(0));
			}

			var N = _arc_len.Length;
			if (f >= _arc_len[N - 1]) {
				return new CurveSample(new Vector3d(_positions[N - 1]), Tangent(N - 1));
			}

			for (var k = 0; k < N; ++k) {
				if (f < _arc_len[k]) {
					var a = k - 1;
					var b = k;
					if (_arc_len[a] == _arc_len[b]) {
						return new CurveSample(new Vector3d(_positions[a]), Tangent(a));
					}

					var t = (f - _arc_len[a]) / (_arc_len[b] - _arc_len[a]);
					return new CurveSample(
						Vector3d.Lerp(_positions[a], _positions[b], t),
						Vector3d.Lerp(Tangent(a), Tangent(b), t));
				}
			}

			throw new ArgumentException("SampledArcLengthParam.Sample: somehow arc len is outside any possible range");
		}


		protected Vector3d Tangent(int i) {
			var N = _arc_len.Length;
			return i == 0
				? (_positions[1] - _positions[0]).Normalized
				: i == N - 1 ? (_positions[N - 1] - _positions[N - 2]).Normalized : (_positions[i + 1] - _positions[i - 1]).Normalized;
		}
	}






	public struct CurveSample2d
	{
		public Vector2d position;
		public Vector2d tangent;
		public CurveSample2d(Vector2d p, Vector2d t) {
			position = p;
			tangent = t;
		}
	}


	public interface IArcLengthParam2d
	{
		double ArcLength { get; }
		CurveSample2d Sample(double fArcLen);
	}


	public class SampledArcLengthParam2d : IArcLengthParam2d
	{
		readonly double[] _arc_len;
		readonly Vector2d[] _positions;

		public SampledArcLengthParam2d(IEnumerable<Vector2d> samples, int nCountHint = -1) {
			var N = (nCountHint == -1) ? samples.Count() : nCountHint;
			_arc_len = new double[N];
			_arc_len[0] = 0;
			_positions = new Vector2d[N];

			var i = 0;
			var prev = Vector2d.Zero;
			foreach (var v in samples) {
				_positions[i] = v;
				if (i > 0) {
					var d = (v - prev).Length;
					_arc_len[i] = _arc_len[i - 1] + d;
				}
				i++;
				prev = v;
			}
		}


		public double ArcLength => _arc_len[_arc_len.Length - 1];

		public CurveSample2d Sample(double f) {
			if (f <= 0) {
				return new CurveSample2d(new Vector2d(_positions[0]), Tangent(0));
			}

			var N = _arc_len.Length;
			if (f >= _arc_len[N - 1]) {
				return new CurveSample2d(new Vector2d(_positions[N - 1]), Tangent(N - 1));
			}

			for (var k = 0; k < N; ++k) {
				if (f < _arc_len[k]) {
					var a = k - 1;
					var b = k;
					if (_arc_len[a] == _arc_len[b]) {
						return new CurveSample2d(new Vector2d(_positions[a]), Tangent(a));
					}

					var t = (f - _arc_len[a]) / (_arc_len[b] - _arc_len[a]);
					return new CurveSample2d(
						Vector2d.Lerp(_positions[a], _positions[b], t),
						Vector2d.Lerp(Tangent(a), Tangent(b), t));
				}
			}

			throw new ArgumentException("SampledArcLengthParam2d.Sample: somehow arc len is outside any possible range");
		}


		protected Vector2d Tangent(int i) {
			var N = _arc_len.Length;
			return i == 0
				? (_positions[1] - _positions[0]).Normalized
				: i == N - 1 ? (_positions[N - 1] - _positions[N - 2]).Normalized : (_positions[i + 1] - _positions[i - 1]).Normalized;
		}
	}



}
