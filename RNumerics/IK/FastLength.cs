// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php


namespace RNumerics.IK
{

	[System.Serializable]
	public struct FastLength
	{
		public float length;
		public float lengthSq;

		FastLength(float length_) {
			length = length_;
			lengthSq = length_ * length_;
		}

		FastLength(float length_, float lengthSq_) {
			length = length_;
			lengthSq = lengthSq_;
		}

		public static FastLength FromLength(float length) {
			return new FastLength(length);
		}

		public static FastLength FromVector3(ref Vector3f v) {
			var length = IKMath.VecLengthAndLengthSq(out var lengthSq, v);
			return new FastLength(length, lengthSq);
		}
	}

}
