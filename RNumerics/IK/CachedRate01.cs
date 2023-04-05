// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics.IK
{

	public struct CachedRate01
	{
		public float _value;
		public float value;
		public bool isGreater0;
		public bool isLess1;

		public static readonly CachedRate01 zero = new(0.0f);

		public CachedRate01(float v) {
			_value = v;
			value = MathUtil.Clamp(v, 0, 1);
			isGreater0 = value > IKMath.IK_EPSILON;
			isLess1 = value < 1.0f - IKMath.IK_EPSILON;
		}

		public void Reset(float v) {
			_value = v;
			value = MathUtil.Clamp(v, 0, 1);
			isGreater0 = value > IKMath.IK_EPSILON;
			isLess1 = value < 1.0f - IKMath.IK_EPSILON;
		}
	}

}
