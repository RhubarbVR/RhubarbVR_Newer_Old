using System;
using System.Collections.Generic;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public sealed class ColorMap
	{
		[MessagePackObject]
		public struct ColorPoint
		{
			[Key(0)]
			public float t;
			[Key(1)]
			public Colorf c;
		}
		[Key(0)]
		public List<ColorPoint> points = new();
		[Key(1)]
		Interval1d _validRange;

		public ColorMap() {
			_validRange = Interval1d.Empty;
		}

		public ColorMap(in float[] t, in Colorf[] c) {
			_validRange = Interval1d.Empty;
			for (var i = 0; i < t.Length; ++i) {
				AddPoint(t[i], c[i]);
			}
		}

		public void AddPoint(in float t, in Colorf c) {
			var cp = new ColorPoint() { t = t, c = c };
			if (points.Count == 0) {
				points.Add(cp);
				_validRange.Contain(t);
			}
			else if (t < points[0].t) {
				points.Insert(0, cp);
				_validRange.Contain(t);
			}
			else {
				for (var k = 0; k < points.Count; ++k) {
					if (points[k].t == t) {
						points[k] = cp;
						return;
					}
					else if (points[k].t > t) {
						points.Insert(k, cp);
						return;
					}
				}
				points.Add(cp);
				_validRange.Contain(t);
			}
		}




		public Colorf Linear(in float t) {
			if (t <= points[0].t) {
				return points[0].c;
			}

			var N = points.Count;
			if (t >= points[N - 1].t) {
				return points[N - 1].c;
			}

			for (var k = 1; k < points.Count; ++k) {
				if (points[k].t > t) {
					ColorPoint prev = points[k - 1], next = points[k];
					var a = (t - prev.t) / (next.t - prev.t);
					return ((1.0f - a) * prev.c) + (a * next.c);
				}
			}
			return points[N - 1].c;  // should never get here...
		}


	}
}
