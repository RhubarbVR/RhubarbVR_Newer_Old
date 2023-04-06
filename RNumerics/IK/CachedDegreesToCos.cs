// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;

namespace RNumerics.IK
{

	public struct CachedDegreesToCos
	{
		public float _degrees;
		public float cos;

		public static readonly CachedDegreesToCos zero = new(0.0f, 1.0f);

		public CachedDegreesToCos(float degrees, float cos_) {
			_degrees = degrees;
			cos = cos_;
		}

		public void Reset(float degrees) {
			_degrees = degrees;
			cos = (float)System.Math.Cos(degrees * MathUtil.DEG_2_RADF);
		}
	}

}
