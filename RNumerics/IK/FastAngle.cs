// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics.IK
{
	[System.Serializable]
	public struct FastAngle
	{
		public float angle; // Radian
		public float cos;
		public float sin;

		public static readonly FastAngle zero = new(0.0f, 1.0f, 0.0f);

		public FastAngle(float angle_) {
			angle = angle_;
			cos = (float)System.Math.Cos(angle_);
			sin = (float)System.Math.Sin(angle_);
		}

		public FastAngle(float angle_, float cos_, float sin_) {
			angle = angle_;
			cos = cos_;
			sin = sin_;
		}
	}

}
