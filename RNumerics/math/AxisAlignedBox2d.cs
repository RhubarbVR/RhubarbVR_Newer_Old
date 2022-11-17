using System;
using System.Collections.Generic;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox2d
	{
		[Key(0)]
		public Vector2d min = new(double.MaxValue, double.MaxValue);
		[Key(1)]
		public Vector2d max = new(double.MinValue, double.MinValue);

		[Exposed, IgnoreMember]
		public Vector2d Min
		{
			get => min;
			set => min = value;
		}
		[Exposed, IgnoreMember]
		public Vector2d Max
		{
			get => max;
			set => max = value;
		}

		public AxisAlignedBox2d() {

		}

		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox2d Empty = new();
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox2d Zero = new(0);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox2d UnitPositive = new(1);
		[Exposed,IgnoreMember]
		public static readonly AxisAlignedBox2d Infinite = new(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue);

		public AxisAlignedBox2d(in double xmin, in double ymin, in double xmax, in double ymax) {
			min = new Vector2d(xmin, ymin);
			max = new Vector2d(xmax, ymax);
		}

		public AxisAlignedBox2d(in double fSquareSize) {
			min = new Vector2d(0, 0);
			max = new Vector2d(fSquareSize, fSquareSize);
		}

		public AxisAlignedBox2d(in double fWidth, in double fHeight) {
			min = new Vector2d(0, 0);
			max = new Vector2d(fWidth, fHeight);
		}

		public AxisAlignedBox2d(in Vector2d vMin, in Vector2d vMax) {
			min = new Vector2d(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
			max = new Vector2d(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
		}

		public AxisAlignedBox2d(in Vector2d vCenter, in double fHalfWidth, in double fHalfHeight) {
			min = new Vector2d(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
			max = new Vector2d(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
		}
		public AxisAlignedBox2d(in Vector2d vCenter, in double fHalfWidth) {
			min = new Vector2d(vCenter.x - fHalfWidth, vCenter.y - fHalfWidth);
			max = new Vector2d(vCenter.x + fHalfWidth, vCenter.y + fHalfWidth);
		}

		public AxisAlignedBox2d(in Vector2d vCenter) {
			min = max = vCenter;
		}


		public AxisAlignedBox2d(in AxisAlignedBox2d o) {
			min = new Vector2d(o.min);
			max = new Vector2d(o.max);
		}
		[IgnoreMember]
		public double Width => Math.Max(max.x - min.x, 0);
		[IgnoreMember]
		public double Height => Math.Max(max.y - min.y, 0);
		[IgnoreMember]
		public double Area => Width * Height;
		[IgnoreMember]
		public double DiagonalLength => (double)Math.Sqrt(((max.x - min.x) * (max.x - min.x)) + ((max.y - min.y) * (max.y - min.y)));
		[IgnoreMember]
		public double MaxDim => Math.Max(Width, Height);
		[IgnoreMember]
		public double MinDim => Math.Min(Width, Height);

		/// <summary>
		/// returns absolute value of largest min/max x/y coordinate (ie max axis distance to origin)
		/// </summary>
		[IgnoreMember]
		public double MaxUnsignedCoordinate => Math.Max(Math.Max(Math.Abs(min.x), Math.Abs(max.x)), Math.Max(Math.Abs(min.y), Math.Abs(max.y)));
		[IgnoreMember]
		public Vector2d Diagonal => new(max.x - min.x, max.y - min.y);
		[IgnoreMember]
		public Vector2d Center => new(0.5f * (min.x + max.x), 0.5f * (min.y + max.y));

		//! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		public Vector2d GetCorner(in int i) {
			return new Vector2d((i % 3 == 0) ? min.x : max.x, (i < 2) ? min.y : max.y);
		}

		/// <summary>
		/// Point inside box where t,s are in range [0,1]
		/// </summary>
		public Vector2d SampleT(in double tx, in double sy) {
			return new Vector2d(((1.0 - tx) * min.x) + (tx * max.x), ((1.0 - sy) * min.y) + (sy * max.y));
		}

		//! value is subtracted from min and added to max
		public void Expand(in double fRadius) {
			min.x -= fRadius;
			min.y -= fRadius;
			max.x += fRadius;
			max.y += fRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(in double fRadius) {
			min.x += fRadius;
			min.y += fRadius;
			max.x -= fRadius;
			max.y -= fRadius;
		}

		public void Add(in double left, in double right, in double bottom, in double top) {
			min.x += left;
			min.y += bottom;
			max.x += right;
			max.y += top;
		}

		public enum ScaleMode
		{
			ScaleRight,
			ScaleLeft,
			ScaleUp,
			ScaleDown,
			ScaleCenter
		}
		public void SetWidth(in double fNewWidth, in ScaleMode eScaleMode) {
			switch (eScaleMode) {
				case ScaleMode.ScaleLeft:
					min.x = max.x - fNewWidth;
					break;
				case ScaleMode.ScaleRight:
					max.x = min.x + fNewWidth;
					break;
				case ScaleMode.ScaleCenter:
					var vCenter = Center;
					min.x = vCenter.x - (0.5f * fNewWidth);
					max.x = vCenter.x + (0.5f * fNewWidth);
					break;
				default:
					throw new Exception("Invalid scale mode...");
			}
		}
		public void SetHeight(in double fNewHeight, in ScaleMode eScaleMode) {
			switch (eScaleMode) {
				case ScaleMode.ScaleDown:
					min.y = max.y - fNewHeight;
					break;
				case ScaleMode.ScaleUp:
					max.y = min.y + fNewHeight;
					break;
				case ScaleMode.ScaleCenter:
					var vCenter = Center;
					min.y = vCenter.y - (0.5f * fNewHeight);
					max.y = vCenter.y + (0.5f * fNewHeight);
					break;
				default:
					throw new Exception("Invalid scale mode...");
			}
		}

		public void Contain(in Vector2d v) {
			if (v.x < min.x) {
				min.x = v.x;
			}

			if (v.x > max.x) {
				max.x = v.x;
			}

			if (v.y < min.y) {
				min.y = v.y;
			}

			if (v.y > max.y) {
				max.y = v.y;
			}
		}


		public void Contain(in AxisAlignedBox2d box) {
			if (box.min.x < min.x) {
				min.x = box.min.x;
			}

			if (box.max.x > max.x) {
				max.x = box.max.x;
			}

			if (box.min.y < min.y) {
				min.y = box.min.y;
			}

			if (box.max.y > max.y) {
				max.y = box.max.y;
			}
		}

		public void Contain(in IList<Vector2d> points) {
			var N = points.Count;
			if (N > 0) {
				var v = points[0];
				Contain(v);
				// once we are sure we have initialized min/max, we can use if/else
				for (var i = 1; i < N; ++i) {
					v = points[i];
					if (v.x < min.x) {
						min.x = v.x;
					}
					else if (v.x > max.x) {
						max.x = v.x;
					}


					if (v.y < min.y) {
						min.y = v.y;
					}
					else if (v.y > max.y) {
						max.y = v.y;
					}
				}
			}
		}



		public AxisAlignedBox2d Intersect(in AxisAlignedBox2d box) {
			var intersect = new AxisAlignedBox2d(
				Math.Max(min.x, box.min.x), Math.Max(min.y, box.min.y),
				Math.Min(max.x, box.max.x), Math.Min(max.y, box.max.y));
			return intersect.Height <= 0 || intersect.Width <= 0 ? AxisAlignedBox2d.Empty : intersect;
		}



		public bool Contains(in Vector2d v) {
			return (min.x < v.x) && (min.y < v.y) && (max.x > v.x) && (max.y > v.y);
		}


		public bool Contains(in AxisAlignedBox2d box2) {
			return Contains(box2.min) && Contains(box2.max);
		}

		public bool Intersects(in AxisAlignedBox2d box) {
			return !((box.max.x < min.x) || (box.min.x > max.x) || (box.max.y < min.y) || (box.min.y > max.y));
		}


		public double Distance(in Vector2d v) {
			var dx = (double)Math.Abs(v.x - Center.x);
			var dy = (double)Math.Abs(v.y - Center.y);
			var fWidth = Width * 0.5f;
			var fHeight = Height * 0.5f;
			if (dx < fWidth && dy < fHeight) {
				return 0.0f;
			}
			else if (dx > fWidth && dy > fHeight) {
				return (double)Math.Sqrt(((dx - fWidth) * (dx - fWidth)) + ((dy - fHeight) * (dy - fHeight)));
			}
			else if (dx > fWidth) {
				return dx - fWidth;
			}
			else if (dy > fHeight) {
				return dy - fHeight;
			}

			return 0.0f;
		}


		//! relative translation
		public void Translate(in Vector2d vTranslate) {
			min.Add(vTranslate);
			max.Add(vTranslate);
		}

		public void Scale(in double scale) {
			min *= scale;
			max *= scale;
		}
		public void Scale(in double scale, in Vector2d origin) {
			min = ((min - origin) * scale) + origin;
			max = ((max - origin) * scale) + origin;
		}

		public void MoveMin(in Vector2d vNewMin) {
			max.x = vNewMin.x + (max.x - min.x);
			max.y = vNewMin.y + (max.y - min.y);
			min.Set(vNewMin);
		}
		public void MoveMin(in double fNewX, in double fNewY) {
			max.x = fNewX + (max.x - min.x);
			max.y = fNewY + (max.y - min.y);
			min.Set(fNewX, fNewY);
		}



		public override string ToString() {
			return string.Format("[{0:F8},{1:F8}] [{2:F8},{3:F8}]", min.x, max.x, min.y, max.y);
		}
	}
}
