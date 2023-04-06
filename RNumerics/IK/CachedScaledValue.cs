// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics.IK
{
	public struct CachedScaledValue
	{
		public float _a;
		public float _b;
		public float value;

		public static readonly CachedScaledValue zero = new(0.0f, 0.0f, 0.0f);

		public CachedScaledValue(float a, float b, float value_) {
			_a = a;
			_b = b;
			value = value_;
		}

		public void Reset(float a, float b) {
			_a = a;
			_b = b;
			value = a * b;
		}
	}
}
