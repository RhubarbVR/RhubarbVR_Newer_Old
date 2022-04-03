using System;
using System.Collections.Generic;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox2d
	{
		[Key(0)]
		public Vector2d Min;
		[Key(1)]
		public Vector2d Max;
		[IgnoreMember]
		public static readonly AxisAlignedBox2d Empty = new();
		[IgnoreMember]
		public static readonly AxisAlignedBox2d Zero = new(0);
		[IgnoreMember]
		public static readonly AxisAlignedBox2d UnitPositive = new(1);
		[IgnoreMember]
		public static readonly AxisAlignedBox2d Infinite = new(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue);


		public AxisAlignedBox2d() {
			Min = new Vector2d(double.MaxValue, double.MaxValue);
			Max = new Vector2d(double.MinValue, double.MinValue);
		}

		public AxisAlignedBox2d(double xmin, double ymin, double xmax, double ymax) {
			Min = new Vector2d(xmin, ymin);
			Max = new Vector2d(xmax, ymax);
		}

		public AxisAlignedBox2d(double fSquareSize) {
			Min = new Vector2d(0, 0);
			Max = new Vector2d(fSquareSize, fSquareSize);
		}

		public AxisAlignedBox2d(double fWidth, double fHeight) {
			Min = new Vector2d(0, 0);
			Max = new Vector2d(fWidth, fHeight);
		}

		public AxisAlignedBox2d(Vector2d vMin, Vector2d vMax) {
			Min = new Vector2d(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
			Max = new Vector2d(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
		}

		public AxisAlignedBox2d(Vector2d vCenter, double fHalfWidth, double fHalfHeight) {
			Min = new Vector2d(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
			Max = new Vector2d(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
		}
		public AxisAlignedBox2d(Vector2d vCenter, double fHalfWidth) {
			Min = new Vector2d(vCenter.x - fHalfWidth, vCenter.y - fHalfWidth);
			Max = new Vector2d(vCenter.x + fHalfWidth, vCenter.y + fHalfWidth);
		}

		public AxisAlignedBox2d(Vector2d vCenter) {
			Min = Max = vCenter;
		}


		public AxisAlignedBox2d(AxisAlignedBox2d o) {
			Min = new Vector2d(o.Min);
			Max = new Vector2d(o.Max);
		}
		[IgnoreMember]
		public double Width => Math.Max(Max.x - Min.x, 0);
		[IgnoreMember]
		public double Height => Math.Max(Max.y - Min.y, 0);
		[IgnoreMember]
		public double Area => Width * Height;
		[IgnoreMember]
		public double DiagonalLength => (double)Math.Sqrt(((Max.x - Min.x) * (Max.x - Min.x)) + ((Max.y - Min.y) * (Max.y - Min.y)));
		[IgnoreMember]
		public double MaxDim => Math.Max(Width, Height);
		[IgnoreMember]
		public double MinDim => Math.Min(Width, Height);

		/// <summary>
		/// returns absolute value of largest min/max x/y coordinate (ie max axis distance to origin)
		/// </summary>
		[IgnoreMember]
		public double MaxUnsignedCoordinate => Math.Max(Math.Max(Math.Abs(Min.x), Math.Abs(Max.x)), Math.Max(Math.Abs(Min.y), Math.Abs(Max.y)));
		[IgnoreMember]
		public Vector2d Diagonal => new(Max.x - Min.x, Max.y - Min.y);
		[IgnoreMember]
		public Vector2d Center => new(0.5f * (Min.x + Max.x), 0.5f * (Min.y + Max.y));

		//! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		public Vector2d GetCorner(int i) {
			return new Vector2d((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
		}

		/// <summary>
		/// Point inside box where t,s are in range [0,1]
		/// </summary>
		public Vector2d SampleT(double tx, double sy) {
			return new Vector2d(((1.0 - tx) * Min.x) + (tx * Max.x), ((1.0 - sy) * Min.y) + (sy * Max.y));
		}

		//! value is subtracted from min and added to max
		public void Expand(double fRadius) {
			Min.x -= fRadius;
			Min.y -= fRadius;
			Max.x += fRadius;
			Max.y += fRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(double fRadius) {
			Min.x += fRadius;
			Min.y += fRadius;
			Max.x -= fRadius;
			Max.y -= fRadius;
		}

		public void Add(double left, double right, double bottom, double top) {
			Min.x += left;
			Min.y += bottom;
			Max.x += right;
			Max.y += top;
		}

		public enum ScaleMode
		{
			ScaleRight,
			ScaleLeft,
			ScaleUp,
			ScaleDown,
			ScaleCenter
		}
		public void SetWidth(double fNewWidth, ScaleMode eScaleMode) {
			switch (eScaleMode) {
				case ScaleMode.ScaleLeft:
					Min.x = Max.x - fNewWidth;
					break;
				case ScaleMode.ScaleRight:
					Max.x = Min.x + fNewWidth;
					break;
				case ScaleMode.ScaleCenter:
					var vCenter = Center;
					Min.x = vCenter.x - (0.5f * fNewWidth);
					Max.x = vCenter.x + (0.5f * fNewWidth);
					break;
				default:
					throw new Exception("Invalid scale mode...");
			}
		}
		public void SetHeight(double fNewHeight, ScaleMode eScaleMode) {
			switch (eScaleMode) {
				case ScaleMode.ScaleDown:
					Min.y = Max.y - fNewHeight;
					break;
				case ScaleMode.ScaleUp:
					Max.y = Min.y + fNewHeight;
					break;
				case ScaleMode.ScaleCenter:
					var vCenter = Center;
					Min.y = vCenter.y - (0.5f * fNewHeight);
					Max.y = vCenter.y + (0.5f * fNewHeight);
					break;
				default:
					throw new Exception("Invalid scale mode...");
			}
		}

		public void Contain(Vector2d v) {
			if (v.x < Min.x) {
				Min.x = v.x;
			}

			if (v.x > Max.x) {
				Max.x = v.x;
			}

			if (v.y < Min.y) {
				Min.y = v.y;
			}

			if (v.y > Max.y) {
				Max.y = v.y;
			}
		}
		public void Contain(ref Vector2d v) {
			if (v.x < Min.x) {
				Min.x = v.x;
			}

			if (v.x > Max.x) {
				Max.x = v.x;
			}

			if (v.y < Min.y) {
				Min.y = v.y;
			}

			if (v.y > Max.y) {
				Max.y = v.y;
			}
		}


		public void Contain(AxisAlignedBox2d box) {
			if (box.Min.x < Min.x) {
				Min.x = box.Min.x;
			}

			if (box.Max.x > Max.x) {
				Max.x = box.Max.x;
			}

			if (box.Min.y < Min.y) {
				Min.y = box.Min.y;
			}

			if (box.Max.y > Max.y) {
				Max.y = box.Max.y;
			}
		}
		public void Contain(ref AxisAlignedBox2d box) {
			if (box.Min.x < Min.x) {
				Min.x = box.Min.x;
			}

			if (box.Max.x > Max.x) {
				Max.x = box.Max.x;
			}

			if (box.Min.y < Min.y) {
				Min.y = box.Min.y;
			}

			if (box.Max.y > Max.y) {
				Max.y = box.Max.y;
			}
		}


		public void Contain(IList<Vector2d> points) {
			var N = points.Count;
			if (N > 0) {
				var v = points[0];
				Contain(ref v);
				// once we are sure we have initialized min/max, we can use if/else
				for (var i = 1; i < N; ++i) {
					v = points[i];
					if (v.x < Min.x) {
						Min.x = v.x;
					}
					else if (v.x > Max.x) {
						Max.x = v.x;
					}


					if (v.y < Min.y) {
						Min.y = v.y;
					}
					else if (v.y > Max.y) {
						Max.y = v.y;
					}
				}
			}
		}



		public AxisAlignedBox2d Intersect(AxisAlignedBox2d box) {
			var intersect = new AxisAlignedBox2d(
				Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y),
				Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y));
			return intersect.Height <= 0 || intersect.Width <= 0 ? AxisAlignedBox2d.Empty : intersect;
		}



		public bool Contains(Vector2d v) {
			return (Min.x < v.x) && (Min.y < v.y) && (Max.x > v.x) && (Max.y > v.y);
		}
		public bool Contains(ref Vector2d v) {
			return (Min.x < v.x) && (Min.y < v.y) && (Max.x > v.x) && (Max.y > v.y);
		}

		public bool Contains(AxisAlignedBox2d box2) {
			return Contains(ref box2.Min) && Contains(ref box2.Max);
		}
		public bool Contains(ref AxisAlignedBox2d box2) {
			return Contains(ref box2.Min) && Contains(ref box2.Max);
		}

		public bool Intersects(AxisAlignedBox2d box) {
			return !((box.Max.x < Min.x) || (box.Min.x > Max.x) || (box.Max.y < Min.y) || (box.Min.y > Max.y));
		}
		public bool Intersects(ref AxisAlignedBox2d box) {
			return !((box.Max.x < Min.x) || (box.Min.x > Max.x) || (box.Max.y < Min.y) || (box.Min.y > Max.y));
		}


		public double Distance(Vector2d v) {
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
		public void Translate(Vector2d vTranslate) {
			Min.Add(vTranslate);
			Max.Add(vTranslate);
		}

		public void Scale(double scale) {
			Min *= scale;
			Max *= scale;
		}
		public void Scale(double scale, Vector2d origin) {
			Min = ((Min - origin) * scale) + origin;
			Max = ((Max - origin) * scale) + origin;
		}

		public void MoveMin(Vector2d vNewMin) {
			Max.x = vNewMin.x + (Max.x - Min.x);
			Max.y = vNewMin.y + (Max.y - Min.y);
			Min.Set(vNewMin);
		}
		public void MoveMin(double fNewX, double fNewY) {
			Max.x = fNewX + (Max.x - Min.x);
			Max.y = fNewY + (Max.y - Min.y);
			Min.Set(fNewX, fNewY);
		}



		public override string ToString() {
			return string.Format("[{0:F8},{1:F8}] [{2:F8},{3:F8}]", Min.x, Max.x, Min.y, Max.y);
		}
	}
}
