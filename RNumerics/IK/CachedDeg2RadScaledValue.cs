// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics.IK
{
	public struct CachedDeg2RadScaledValue
	{
		public float _a;
		public float _b;
		public float value;

		public static readonly CachedDeg2RadScaledValue zero = new(0.0f, 0.0f, 0.0f);

		public CachedDeg2RadScaledValue(float a, float b, float value_) {
			_a = a;
			_b = b;
			value = value_;
		}
	}

}
