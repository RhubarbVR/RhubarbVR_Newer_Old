using System;
using System.Collections.Generic;

namespace RNumerics
{
	/// <summary>
	/// Summary description for ImplicitField2D.
	/// </summary>
	public interface IImplicitField2d
	{
		float Value(in float fX, in float fY);

		void Gradient(in float fX, in float fY, ref float fGX, ref float fGY);

		AxisAlignedBox2f Bounds { get; }
	}

	public interface IImplicitOperator2d : IImplicitField2d
	{
		void AddChild(IImplicitField2d field);
	}




	public sealed class ImplicitPoint2d : IImplicitField2d
	{
		Vector2f _vCenter;

		public ImplicitPoint2d(in float x, in float y)
		{
			_vCenter = new Vector2f(x, y);
			Radius = 1;
		}
		public ImplicitPoint2d(in float x, in float y, in float radius)
		{
			_vCenter = new Vector2f(x, y);
			Radius = radius;
		}

		public float Value(in float fX, in float fY)
		{

			var tx = (fX - _vCenter.x);
			var ty = (fY - _vCenter.y);
			var fDist2 = (tx * tx) + (ty * ty);
			fDist2 /= (Radius * Radius);
			fDist2 = 1.0f - fDist2;
			return fDist2 < 0.0f ? 0.0f : fDist2 * fDist2 * fDist2;
		}

		public AxisAlignedBox2f Bounds => new AxisAlignedBox2f(LowX, LowY, HighX, HighY);

		public void Gradient(in float fX, in float fY, ref float fGX, ref float fGY)
		{
			var tx = (fX - _vCenter.x);
			var ty = (fY - _vCenter.y);
			var fDist2 = (tx * tx) + (ty * ty);
			var fTmp = 1.0f - fDist2;
			if (fTmp < 0.0f)
			{
				fGX = fGY = 0;
			}
			else
			{
				var fSqrt = (float)Math.Sqrt(fDist2);
				var fGradMag = -6.0f * fSqrt * fTmp * fTmp;
				fGradMag /= fSqrt;
				fGX = tx * fGradMag;
				fGY = ty * fGradMag;
			}
		}

		public bool InBounds(in float x, in float y)
		{
			return x >= LowX && x <= HighX && x >= LowY && x <= HighY;
		}

		public float LowX => _vCenter.x - Radius;

		public float LowY => _vCenter.y - Radius;

		public float HighX => _vCenter.x + Radius;

		public float HighY => _vCenter.y + Radius;

		public float Radius { get; set; }

		public float X
		{
			get => _vCenter.x;
			set => _vCenter.x = value;
		}

		public float Y
		{
			get => _vCenter.y;
			set => _vCenter.y = value;
		}

		public Vector2f Center
		{
			get => _vCenter;
			set => _vCenter = value;
		}
	}



}
