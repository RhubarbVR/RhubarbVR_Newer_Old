// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;

namespace RNumerics.IK
{
	public struct CachedDegreesToCosSin
	{
		public float _degrees;
		public float cos;
		public float sin;

		public static readonly CachedDegreesToCosSin zero = new(0.0f, 1.0f, 0.0f);

		public CachedDegreesToCosSin(float degrees) {
			_degrees = degrees;
			cos = (float)System.Math.Cos(degrees * MathUtil.DEG_2_RADF);
			sin = (float)System.Math.Sin(degrees * MathUtil.DEG_2_RADF);
		}

		public CachedDegreesToCosSin(float degrees, float cos_, float sin_) {
			_degrees = degrees;
			cos = cos_;
			sin = sin_;
		}

		public void Reset(float degrees) {
			_degrees = degrees;
			cos = (float)System.Math.Cos(degrees * MathUtil.DEG_2_RADF);
			sin = (float)System.Math.Sin(degrees * MathUtil.DEG_2_RADF);
		}
	}

}
