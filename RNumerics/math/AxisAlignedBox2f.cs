using System;
using System.IO;

namespace RNumerics
{
	public struct AxisAlignedBox2f : ISerlize<AxisAlignedBox2f>
	{
		public Vector2f min = new(float.MaxValue, float.MaxValue);
		public Vector2f max = new(float.MinValue, float.MinValue);


		public void Serlize(BinaryWriter binaryWriter) {
			min.Serlize(binaryWriter);
			max.Serlize(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			min.DeSerlize(binaryReader);
			max.DeSerlize(binaryReader);
		}

		[Exposed]
		public Vector2f Min
		{
			get => min;
			set => min = value;
		}
		[Exposed]
		public Vector2f Max
		{
			get => max;
			set => max = value;
		}

		[Exposed]
		public static readonly AxisAlignedBox2f Empty = new();
		[Exposed]
		public static readonly AxisAlignedBox2f Zero = new(0);
		[Exposed]
		public static readonly AxisAlignedBox2f UnitPositive = new(1);
		[Exposed]
		public static readonly AxisAlignedBox2f Infinite = new(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
		public AxisAlignedBox2f() {

		}
		public AxisAlignedBox2f(in float xmin, in float ymin, in float xmax, in float ymax) {
			min = new Vector2f(xmin, ymin);
			max = new Vector2f(xmax, ymax);
		}

		public AxisAlignedBox2f(in float fSquareSize) {
			min = new Vector2f(0, 0);
			max = new Vector2f(fSquareSize, fSquareSize);
		}

		public AxisAlignedBox2f(in float fWidth, in float fHeight) {
			min = new Vector2f(0, 0);
			max = new Vector2f(fWidth, fHeight);
		}

		public AxisAlignedBox2f(in Vector2f vMin, in Vector2f vMax) {
			min = new Vector2f(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
			max = new Vector2f(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
		}

		public AxisAlignedBox2f(in Vector2f vCenter, in float fHalfWidth, in float fHalfHeight) {
			min = new Vector2f(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
			max = new Vector2f(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
		}
		public AxisAlignedBox2f(in Vector2f vCenter, in float fHalfWidth) {
			min = new Vector2f(vCenter.x - fHalfWidth, vCenter.y - fHalfWidth);
			max = new Vector2f(vCenter.x + fHalfWidth, vCenter.y + fHalfWidth);
		}

		public AxisAlignedBox2f(in Vector2f vCenter) {
			min = max = vCenter;
		}

		public AxisAlignedBox2f(in AxisAlignedBox2f o) {
			min = new Vector2f(o.min);
			max = new Vector2f(o.max);
		}
		
		public float Width => Math.Max(max.x - min.x, 0);
		
		public float Height => Math.Max(max.y - min.y, 0);
		
		public float Area => Width * Height;
		
		public float DiagonalLength => (float)Math.Sqrt(((max.x - min.x) * (max.x - min.x)) + ((max.y - min.y) * (max.y - min.y)));
		
		public float MaxDim => Math.Max(Width, Height);
		
		public Vector2f Diagonal => new(max.x - min.x, max.y - min.y);
		
		public Vector2f Center => new(0.5f * (min.x + max.x), 0.5f * (min.y + max.y));
		
		public Vector2f BottomLeft => min;
		
		public Vector2f BottomRight => new(max.x, min.y);
		
		public Vector2f TopLeft => new(min.x, max.y);
		
		public Vector2f TopRight => max;

		
		public Vector2f CenterLeft => new(min.x, (min.y + max.y) * 0.5f);
		
		public Vector2f CenterRight => new(max.x, (min.y + max.y) * 0.5f);
		
		public Vector2f CenterTop => new((min.x + max.x) * 0.5f, max.y);
		
		public Vector2f CenterBottom => new((min.x + max.x) * 0.5f, min.y);


		//! 0 == bottom-left, 1 = bottom-right, 2 == top-right, 3 == top-left
		public Vector2f GetCorner(in int i) {
			return new((i % 3 == 0) ? min.x : max.x, (i < 2) ? min.y : max.y);
		}

		//! value is subtracted from min and added to max
		public void Expand(in float fRadius) {
			min.x -= fRadius;
			min.y -= fRadius;
			max.x += fRadius;
			max.y += fRadius;
		}
		//! value is added to min and subtracted from max
		public void Contract(in float fRadius) {
			min.x += fRadius;
			min.y += fRadius;
			max.x -= fRadius;
			max.y -= fRadius;
		}

		public void Add(in float left, in float right, in float bottom, in float top) {
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
		public void SetWidth(in float fNewWidth, in ScaleMode eScaleMode) {
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
		public void SetHeight(in float fNewHeight, in ScaleMode eScaleMode) {
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

		public void Contain(in Vector2f v) {
			min.x = Math.Min(min.x, v.x);
			min.y = Math.Min(min.y, v.y);
			max.x = Math.Max(max.x, v.x);
			max.y = Math.Max(max.y, v.y);
		}

		public void Contain(in AxisAlignedBox2f box) {
			min.x = Math.Min(min.x, box.min.x);
			min.y = Math.Min(min.y, box.min.y);
			max.x = Math.Max(max.x, box.max.x);
			max.y = Math.Max(max.y, box.max.y);
		}

		public AxisAlignedBox2f Intersect(in AxisAlignedBox2f box) {
			var intersect = new AxisAlignedBox2f(
				Math.Max(min.x, box.min.x), Math.Max(min.y, box.min.y),
				Math.Min(max.x, box.max.x), Math.Min(max.y, box.max.y));
			return intersect.Height <= 0 || intersect.Width <= 0 ? AxisAlignedBox2f.Empty : intersect;
		}



		public bool Contains(in Vector2f v) {
			return (min.x < v.x) && (min.y < v.y) && (max.x > v.x) && (max.y > v.y);
		}
		public bool Intersects(in AxisAlignedBox2f box) {
			return !((box.max.x < min.x) || (box.min.x > max.x) || (box.max.y < min.y) || (box.min.y > max.y));
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
			min.Add(vTranslate);
			max.Add(vTranslate);
		}

		public void MoveMin(in Vector2f vNewMin) {
			max.x = vNewMin.x + (max.x - min.x);
			max.y = vNewMin.y + (max.y - min.y);
			min.Set(vNewMin);
		}
		public void MoveMin(in float fNewX, in float fNewY) {
			max.x = fNewX + (max.x - min.x);
			max.y = fNewY + (max.y - min.y);
			min.Set(fNewX, fNewY);
		}



		public override string ToString() {
			return string.Format("[{0:F8},{1:F8}] [{2:F8},{3:F8}]", min.x, max.x, min.y, max.y);
		}
	}
}
