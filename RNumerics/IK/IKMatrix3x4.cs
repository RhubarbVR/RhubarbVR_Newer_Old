// Copyright (c) 2016 Nora
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

namespace RNumerics.IK
{
	[System.Serializable]
	public struct IKMatrix3x4
	{
		public IKMatrix3x3 basis;
		public Vector3f origin;

		public static readonly IKMatrix3x4 identity = new(IKMatrix3x3.identity, Vector3f.Zero);

		public IKMatrix3x4(IKMatrix3x3 _basis, Vector3f _origin) {
			basis = _basis;
			origin = _origin;
		}

		public IKMatrix3x4(ref IKMatrix3x3 _basis, ref Vector3f _origin) {
			basis = _basis;
			origin = _origin;
		}

		public IKMatrix3x4(ref Matrix m) {
			basis = new IKMatrix3x3(ref m);
			origin = new Vector3f(m.m.M14, m.m.M24, m.m.M34);
		}

		public static implicit operator Matrix(IKMatrix3x4 t) {
			var m = Matrix.Identity;
			m.m.M11 = t.basis.column0.x;
			m.m.M12 = t.basis.column1.x;
			m.m.M13 = t.basis.column2.x;

			m.m.M21 = t.basis.column0.y;
			m.m.M22 = t.basis.column1.y;
			m.m.M23 = t.basis.column2.y;

			m.m.M31 = t.basis.column0.z;
			m.m.M32 = t.basis.column1.z;
			m.m.M33 = t.basis.column2.z;

			m.m.M14 = t.origin.x;
			m.m.M24 = t.origin.y;
			m.m.M34 = t.origin.z;
			return m;
		}

		public static implicit operator IKMatrix3x4(Matrix m) => new(ref m);

		public static Vector3f operator *(IKMatrix3x4 t, Vector3f v) {
			IKMath.MatMultVecAdd(out var tmp, t.basis, v, t.origin);
			return tmp;
		}
	}


}
