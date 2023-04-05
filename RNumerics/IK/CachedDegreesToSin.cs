// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics.IK
{
	public struct CachedDegreesToSin
	{
		public float _degrees;
		public float sin;

		public static readonly CachedDegreesToSin zero = new(0.0f, 0.0f);

		public CachedDegreesToSin(float degrees, float sin_) {
			_degrees = degrees;
			sin = sin_;
		}

		public void Reset(float degrees) {
			_degrees = degrees;
			sin = (float)System.Math.Sin(degrees * MathUtil.DEG_2_RADF);
		}
	}

}
