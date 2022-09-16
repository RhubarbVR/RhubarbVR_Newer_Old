using System;

using MessagePack;


namespace RNumerics
{
	[MessagePackObject]
	public struct AxisAlignedBox2f
	{
		[Key(0)]
		public Vector2f Min = new(float.MaxValue, float.MaxValue);
		[Key(1)]
		public Vector2f Max = new(float.MinValue, float.MinValue);
		[IgnoreMember]
		public static readonly AxisAlignedBox2f Empty = new();
		[IgnoreMember]
		public static readonly AxisAlignedBox2f Zero = new(0);
		[IgnoreMember]
		public static readonly AxisAlignedBox2f UnitPositive = new(1);
		[IgnoreMember]
		public static readonly AxisAlignedBox2f Infinite = new(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
		public AxisAlignedBox2f() {

		}
		public AxisAlignedBox2f(in float xmin, in float ymin, in float xmax, in float ymax) {
			Min = new Vector2f(xmin, ymin);
			Max = new Vector2f(xmax, ymax);
		}

		public AxisAlignedBox2f(in float fSquareSize) {
			Min = new Vector2f(0, 0);
			Max = new Vector2f(fSquareSize, fSquareSize);
		}

		public AxisAlignedBox2f(in float fWidth, in float fHeight) {
			Min = new Vector2f(0, 0);
			Max = new Vector2f(fWidth, fHeight);
		}

		public AxisAlignedBox2f(in Vector2f vMin, in Vector2f vMax) {
			Min = new Vector2f(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
			Max = new Vector2f(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
		}

		public AxisAlignedBox2f(in Vector2f vCenter, in float fHalfWidth, in float fHalfHeight) {
			Min = new Vector2f(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
			Max = new Vector2f(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
		}
		public AxisAlignedBox2f(in Vector2f vCenter, in float fHalfWidth) {
			Min = new Vector2f(vCenter.x - fHalfWidth, vCenter.y - fHalfWidth);
			Max = new Vector2f(vCenter.x + fHalfWidth, vCenter.y + fHalfWidth);
		}

		public AxisAlignedBox2f(in Vector2f vCenter) {
			Min = Max = vCenter;
		}

		public AxisAlignedBox2f(in AxisAlignedBox2f o) {
			Min = new Vector2f(o.Min);
			Max = new Vector2f(o.Max);
		}
		[IgnoreMember]
		public float Width => Math.Max(Max.x - Min.x, 0);
		[IgnoreMember]
		public float Height => Math.Max(Max.y - Min.y, 0);
		[IgnoreMember]
		public float Area => Width * Height;
		[IgnoreMember]
		public float DiagonalLength => (float)Math.Sqrt(((Max.x - Min.x) * (Max.x - Min.x)) + ((Max.y - Min.y) * (Max.y - Min.y)));
		[IgnoreMember]
		public float MaxDim => Math.Max(Width, Height);
		[IgnoreMember]
		public Vector2f Diagonal => new(Max.x - Min.x, Max.y - Min.y);
		[IgnoreMember]
		public Vector2f Center => new(0.5f * (Min.x + Max.x), 0.5f * (Min.y + Max.y));
		[IgnoreMember]
		public Vector2f BottomLeft => Min;
		[IgnoreMember]
		public Vector2f BottomRight => new(Max.x, Min.y);
		[IgnoreMember]
		public Vector2f TopLeft => new(Min.x, Max.y);
		[IgnoreMember]
		public Vector2f TopRight => Max;

		[IgnoreMember]
		public Vector2f CenterLeft => new(Min.x, (Min.y + Max.y) * 0.5f);
		[IgnoreMember]
		public Vector2f CenterRight => new(Max.x, (Min.y + Max.y) * 0.5f);
		[IgnoreMember]
		public Vector2f CenterTop => new((Min.x + Max.x) * 0.5f, Max.y);
		[IgnoreMember]
		public Vector2f CenterBottom => new((Min.x + Max.x) * 0.5f, Min.y);


		//! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		public Vector2f GetCorner(in int i) {
			return new((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
		}

		//! value is subtracted from min and added to max
		public void Expand(in float fRadius) {
			Min.x -= fRadius;
			Min.y -= fRadius;
			Max.x += fRadius;
			Max.y += fRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(in float fRadius) {
			Min.x += fRadius;
			Min.y += fRadius;
			Max.x -= fRadius;
			Max.y -= fRadius;
		}

		public void Add(in float left, in float right, in float bottom, in float top) {
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
		public void SetWidth(in float fNewWidth, in ScaleMode eScaleMode) {
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
		public void SetHeight(in float fNewHeight, in ScaleMode eScaleMode) {
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

		public void Contain(in Vector2f v) {
			Min.x = Math.Min(Min.x, v.x);
			Min.y = Math.Min(Min.y, v.y);
			Max.x = Math.Max(Max.x, v.x);
			Max.y = Math.Max(Max.y, v.y);
		}

		public void Contain(in AxisAlignedBox2f box) {
			Min.x = Math.Min(Min.x, box.Min.x);
			Min.y = Math.Min(Min.y, box.Min.y);
			Max.x = Math.Max(Max.x, box.Max.x);
			Max.y = Math.Max(Max.y, box.Max.y);
		}

		public AxisAlignedBox2f Intersect(in AxisAlignedBox2f box) {
			var intersect = new AxisAlignedBox2f(
				Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y),
				Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y));
			return intersect.Height <= 0 || intersect.Width <= 0 ? AxisAlignedBox2f.Empty : intersect;
		}



		public bool Contains(in Vector2f v) {
			return (Min.x < v.x) && (Min.y < v.y) && (Max.x > v.x) && (Max.y > v.y);
		}
		public bool Intersects(in AxisAlignedBox2f box) {
			return !((box.Max.x < Min.x) || (box.Min.x > Max.x) || (box.Max.y < Min.y) || (box.Min.y > Max.y));
		}



		public float Distance(in Vector2f v) {
			var dx = (float)Math.Abs(v.x - Center.x);
			var dy = (float)Math.Abs(v.y - Center.y);
			var fWidth = Width * 0.5f;
			var fHeight = Height * 0.5f;
			return dx < fWidth && dy < fHeight
				? 0.0f
				: dx > fWidth && dy > fHeight
					? (float)Math.Sqrt(((dx - fWidth) * (dx - fWidth)) + ((dy - fHeight) * (dy - fHeight)))
					: dx > fWidth ? dx - fWidth : dy > fHeight ? dy - fHeight : 0.0f;
		}


		//! relative translation
		public void Translate(in Vector2f vTranslate) {
			Min.Add(vTranslate);
			Max.Add(vTranslate);
		}

		public void MoveMin(in Vector2f vNewMin) {
			Max.x = vNewMin.x + (Max.x - Min.x);
			Max.y = vNewMin.y + (Max.y - Min.y);
			Min.Set(vNewMin);
		}
		public void MoveMin(in float fNewX, in float fNewY) {
			Max.x = fNewX + (Max.x - Min.x);
			Max.y = fNewY + (Max.y - Min.y);
			Min.Set(fNewX, fNewY);
		}



		public override string ToString() {
			return string.Format("[{0:F8},{1:F8}] [{2:F8},{3:F8}]", Min.x, Max.x, Min.y, Max.y);
		}
	}
}
