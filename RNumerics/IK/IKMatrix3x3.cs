// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics.IK
{

	[System.Serializable]
	public struct IKMatrix3x3
	{
		public Vector3f column0, column1, column2;

		public static readonly IKMatrix3x3 identity = new(
			1.0f, 0.0f, 0.0f,
			0.0f, 1.0f, 0.0f,
			0.0f, 0.0f, 1.0f);

		public Vector3f Row0 => new(column0.x, column1.x, column2.x);
		public Vector3f Row1 => new(column0.y, column1.y, column2.y);
		public Vector3f Row2 => new(column0.z, column1.z, column2.z);

		public bool IsFuzzyIdentity
		{
			get {
				return column0.x >= 1.0f - IKMath.IK_EPSILON && column1.x >= -IKMath.IK_EPSILON && column2.x >= -IKMath.IK_EPSILON &&
					column0.y >= -IKMath.IK_EPSILON && column1.y >= 1.0f - IKMath.IK_EPSILON && column2.y >= -IKMath.IK_EPSILON &&
					column0.z >= -IKMath.IK_EPSILON && column1.z >= -IKMath.IK_EPSILON && column2.z >= 1.0f - IKMath.IK_EPSILON &&
					column0.x <= 1.0f + IKMath.IK_EPSILON && column1.x <= IKMath.IK_EPSILON && column2.x <= IKMath.IK_EPSILON &&
					column0.y <= IKMath.IK_EPSILON && column1.y <= 1.0f + IKMath.IK_EPSILON && column2.y <= IKMath.IK_EPSILON &&
					column0.z <= IKMath.IK_EPSILON && column1.z <= IKMath.IK_EPSILON && column2.z <= 1.0f + IKMath.IK_EPSILON;
			}
		}

		public IKMatrix3x3 Transpose
		{
			get {
				return new IKMatrix3x3(
					column0.x, column0.y, column0.z,
					column1.x, column1.y, column1.z,
					column2.x, column2.y, column2.z);
			}
		}

		public IKMatrix3x3(
			float _11, float _12, float _13,
			float _21, float _22, float _23,
			float _31, float _32, float _33) {
			column0 = new Vector3f(_11, _21, _31);
			column1 = new Vector3f(_12, _22, _32);
			column2 = new Vector3f(_13, _23, _33);
		}

		public IKMatrix3x3(ref Matrix m) {
			column0 = new Vector3f(m.m.M11, m.m.M21, m.m.M31);
			column1 = new Vector3f(m.m.M12, m.m.M22, m.m.M32);
			column2 = new Vector3f(m.m.M13, m.m.M23, m.m.M33);
		}

		public IKMatrix3x3(Quaternionf q) {
			IKMath.MatSetRot(out this, ref q);
		}

		public static IKMatrix3x3 FromColumn(Vector3f column0, Vector3f column1, Vector3f column2) {
			var r = new IKMatrix3x3();
			r.SetColumn(ref column0, ref column1, ref column2);
			return r;
		}

		public static IKMatrix3x3 FromColumn(ref Vector3f column0, ref Vector3f column1, ref Vector3f column2) {
			var r = new IKMatrix3x3();
			r.SetColumn(ref column0, ref column1, ref column2);
			return r;
		}


		public void SetColumn(ref Vector3f c0, ref Vector3f c1, ref Vector3f c2) {
			column0 = c0;
			column1 = c1;
			column2 = c2;
		}

		public static implicit operator Matrix(IKMatrix3x3 m) {
			var r = Matrix.Identity;
			r.m.M11 = m.column0.x;
			r.m.M12 = m.column1.x;
			r.m.M13 = m.column2.x;

			r.m.M21 = m.column0.y;
			r.m.M22 = m.column1.y;
			r.m.M23 = m.column2.y;

			r.m.M31 = m.column0.z;
			r.m.M32 = m.column1.z;
			r.m.M33 = m.column2.z;
			return r;
		}

		public static implicit operator IKMatrix3x3(Matrix m) => new(ref m);

		public override string ToString() {
			var str = new System.Text.StringBuilder();
			str.Append(Row0.ToString());
			str.Append(" : ");
			str.Append(Row1.ToString());
			str.Append(" : ");
			str.Append(Row2.ToString());
			return str.ToString();
		}

		public string ToString(string format) {
			var str = new System.Text.StringBuilder();
			str.Append(Row0.ToString(format));
			str.Append(" : ");
			str.Append(Row1.ToString(format));
			str.Append(" : ");
			str.Append(Row2.ToString(format));
			return str.ToString();
		}

	}

}
