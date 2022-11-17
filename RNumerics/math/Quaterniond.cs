using MessagePack;

using System;



namespace RNumerics
{
	// mostly ported from WildMagic5 Wm5Quaternion, from geometrictools.com
	[MessagePackObject]
	public struct Quaterniond
	{
		// note: in Wm5 version, this is a 4-element array stored in order (w,x,y,z).

		[Key(0)]
		public double x;
		[Key(1)]
		public double y;
		[Key(2)]
		public double z;
		[Key(3)]
		public double w;

		[Exposed, IgnoreMember]
		public double X
		{
			get => x;
			set => x = value;
		}
		[Exposed, IgnoreMember]
		public double Y
		{
			get => y;
			set => y = value;
		}
		[Exposed, IgnoreMember]
		public double Z
		{
			get => z;
			set => z = value;
		}
		[Exposed, IgnoreMember]
		public double W
		{
			get => w;
			set => w = value;
		}
		public Quaterniond() {
			x = 0;
			y = 0;
			z = 0;
			w = 1;
		}

		public Quaterniond(in double x, in double y, in double z, in double w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Quaterniond(in double[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }
		public Quaterniond(in Quaterniond q2) { x = q2.x; y = q2.y; z = q2.z; w = q2.w; }

		public Quaterniond(in Vector3d axis, in double AngleDeg) {
			x = y = z = 0;
			w = 1;
			SetAxisAngleD(axis, AngleDeg);
		}
		public Quaterniond(in Vector3d vFrom, in Vector3d vTo) {
			x = y = z = 0;
			w = 1;
			SetFromTo(vFrom, vTo);
		}
		public Quaterniond(in Quaterniond p, in Quaterniond q, in double t) {
			x = y = z = 0;
			w = 1;
			SetToSlerp(p, q, t);
		}
		public Quaterniond(in Matrix3d mat) {
			x = y = z = 0;
			w = 1;
			SetFromRotationMatrix(mat);
		}
		[Exposed,IgnoreMember]
		static public readonly Quaterniond Zero = new(0.0, 0.0, 0.0, 0.0);
		[Exposed,IgnoreMember]
		static public readonly Quaterniond Identity = new(0.0, 0.0, 0.0, 1.0);

		public double this[in int key]
		{
			get => key == 0 ? x : key == 1 ? y : key == 2 ? z : w;
			set {
				if (key == 0) { x = value; }
				else if (key == 1) { y = value; }
				else if (key == 2) { z = value; }
				else {
					w = value;
				}
			}

		}

		[IgnoreMember]
		public double LengthSquared => (x * x) + (y * y) + (z * z) + (w * w);
		[IgnoreMember]
		public double Length => (double)Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w));

		public double Normalize(in double epsilon = 0) {
			var length = Length;
			if (length > epsilon) {
				var invLength = 1.0 / length;
				x *= invLength;
				y *= invLength;
				z *= invLength;
				w *= invLength;
			}
			else {
				length = 0;
				x = y = z = w = 0;
			}
			return length;
		}
		[IgnoreMember]
		public Quaterniond Normalized
		{
			get { var q = new Quaterniond(this); q.Normalize(); return q; }
		}

		public double Dot(in Quaterniond q2) {
			var v = x * q2.x;
			return v + (y * q2.y) + (z * q2.z) + (w * q2.w);
		}


		public static Quaterniond operator -(in Quaterniond q2) => new(-q2.x, -q2.y, -q2.z, -q2.w);

		public static Quaterniond operator *(in Quaterniond a, in Quaterniond b) {
			var w = (a.w * b.w) - (a.x * b.x) - (a.y * b.y) - (a.z * b.z);
			var x = (a.w * b.x) + (a.x * b.w) + (a.y * b.z) - (a.z * b.y);
			var y = (a.w * b.y) + (a.y * b.w) + (a.z * b.x) - (a.x * b.z);
			var z = (a.w * b.z) + (a.z * b.w) + (a.x * b.y) - (a.y * b.x);
			return new Quaterniond(x, y, z, w);
		}
		public static Quaterniond operator *(in Quaterniond q1, in double d) => new(d * q1.x, d * q1.y, d * q1.z, d * q1.w);
		public static Quaterniond operator *(in double d, in Quaterniond q1) => new(d * q1.x, d * q1.y, d * q1.z, d * q1.w);

		public static Quaterniond operator -(in Quaterniond q1, in Quaterniond q2) => new(q1.x - q2.x, q1.y - q2.y, q1.z - q2.z, q1.w - q2.w);
		public static Quaterniond operator +(in Quaterniond q1, in Quaterniond q2) => new(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);

		public static Vector3d operator *(in Quaterniond q, in Vector3d v) {
			//return q.ToRotationMatrix() * v;
			// inline-expansion of above:
			var twoX = 2 * q.x;
			var twoY = 2 * q.y;
			var twoZ = 2 * q.z;
			var twoWX = twoX * q.w;
			var twoWY = twoY * q.w;
			var twoWZ = twoZ * q.w;
			var twoXX = twoX * q.x;
			var twoXY = twoY * q.x;
			var twoXZ = twoZ * q.x;
			var twoYY = twoY * q.y;
			var twoYZ = twoZ * q.y;
			var twoZZ = twoZ * q.z;
			return new Vector3d(
				(v.x * (1 - (twoYY + twoZZ))) + (v.y * (twoXY - twoWZ)) + (v.z * (twoXZ + twoWY)),
				(v.x * (twoXY + twoWZ)) + (v.y * (1 - (twoXX + twoZZ))) + (v.z * (twoYZ - twoWX)),
				(v.x * (twoXZ - twoWY)) + (v.y * (twoYZ + twoWX)) + (v.z * (1 - (twoXX + twoYY))));
			;
		}


		// these multiply quaternion by (1,0,0), (0,1,0), (0,0,1), respectively.
		// faster than full multiply, because of all the zeros
		[IgnoreMember]
		public Vector3d AxisX
		{
			get {
				var twoY = 2 * y;
				var twoZ = 2 * z;
				var twoWY = twoY * w;
				var twoWZ = twoZ * w;
				var twoXY = twoY * x;
				var twoXZ = twoZ * x;
				var twoYY = twoY * y;
				var twoZZ = twoZ * z;
				return new Vector3d(1 - (twoYY + twoZZ), twoXY + twoWZ, twoXZ - twoWY);
			}
		}
		[IgnoreMember]
		public Vector3d AxisY
		{
			get {
				var twoX = 2 * x;
				var twoY = 2 * y;
				var twoZ = 2 * z;
				var twoWX = twoX * w;
				var twoWZ = twoZ * w;
				var twoXX = twoX * x;
				var twoXY = twoY * x;
				var twoYZ = twoZ * y;
				var twoZZ = twoZ * z;
				return new Vector3d(twoXY - twoWZ, 1 - (twoXX + twoZZ), twoYZ + twoWX);
			}
		}
		[IgnoreMember]
		public Vector3d AxisZ
		{
			get {
				var twoX = 2 * x;
				var twoY = 2 * y;
				var twoZ = 2 * z;
				var twoWX = twoX * w;
				var twoWY = twoY * w;
				var twoXX = twoX * x;
				var twoXZ = twoZ * x;
				var twoYY = twoY * y;
				var twoYZ = twoZ * y;
				return new Vector3d(twoXZ + twoWY, twoYZ - twoWX, 1 - (twoXX + twoYY));
			}
		}



		public Quaterniond Inverse() {
			var norm = LengthSquared;
			if (norm > 0) {
				var invNorm = 1.0 / norm;
				return new Quaterniond(
					-x * invNorm, -y * invNorm, -z * invNorm, w * invNorm);
			}
			else {
				return Quaterniond.Zero;
			}
		}
		public static Quaterniond Inverse(Quaterniond q) {
			return q.Inverse();
		}


		/// <summary>
		/// Equivalent to transpose of matrix. similar to inverse, but w/o normalization...
		/// </summary>
		public Quaterniond Conjugate() {
			return new Quaterniond(-x, -y, -z, w);
		}


		public Matrix3d ToRotationMatrix() {
			var twoX = 2 * x;
			var twoY = 2 * y;
			var twoZ = 2 * z;
			var twoWX = twoX * w;
			var twoWY = twoY * w;
			var twoWZ = twoZ * w;
			var twoXX = twoX * x;
			var twoXY = twoY * x;
			var twoXZ = twoZ * x;
			var twoYY = twoY * y;
			var twoYZ = twoZ * y;
			var twoZZ = twoZ * z;
			return new Matrix3d(
				1 - (twoYY + twoZZ), twoXY - twoWZ, twoXZ + twoWY,
				twoXY + twoWZ, 1 - (twoXX + twoZZ), twoYZ - twoWX,
				twoXZ - twoWY, twoYZ + twoWX, 1 - (twoXX + twoYY));
		}



		public void SetAxisAngleD(in Vector3d axis, in double AngleDeg) {
			var angle_rad = MathUtil.DEG_2_RAD * AngleDeg;
			var halfAngle = 0.5 * angle_rad;
			var sn = Math.Sin(halfAngle);
			w = (double)Math.Cos(halfAngle);
			x = (double)(sn * axis.x);
			y = (double)(sn * axis.y);
			z = (double)(sn * axis.z);
		}
		public static Quaterniond AxisAngleD(in Vector3d axis, in double angleDeg) {
			return new Quaterniond(axis, angleDeg);
		}
		public static Quaterniond AxisAngleR(in Vector3d axis, in double angleRad) {
			return new Quaterniond(axis, angleRad * MathUtil.RAD_2_DEGF);
		}

		// this function can take non-normalized vectors vFrom and vTo (normalizes internally)
		public void SetFromTo(in Vector3d vFrom, in Vector3d vTo) {
			// [TODO] this page seems to have optimized version:
			//    http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors

			// [RMS] not ideal to explicitly normalize here, but if we don't,
			//   output quaternion is not normalized and this causes problems,
			//   eg like drift if we do repeated SetFromTo()
			Vector3d from = vFrom.Normalized, to = vTo.Normalized;
			var bisector = (from + to).Normalized;
			w = from.Dot(bisector);
			if (w != 0) {
				var cross = from.Cross(bisector);
				x = cross.x;
				y = cross.y;
				z = cross.z;
			}
			else {
				double invLength;
				if (Math.Abs(from.x) >= Math.Abs(from.y)) {
					// V1.x or V1.z is the largest magnitude component.
					invLength = (double)(1.0 / Math.Sqrt((from.x * from.x) + (from.z * from.z)));
					x = -from.z * invLength;
					y = 0;
					z = +from.x * invLength;
				}
				else {
					// V1.y or V1.z is the largest magnitude component.
					invLength = (double)(1.0 / Math.Sqrt((from.y * from.y) + (from.z * from.z)));
					x = 0;
					y = +from.z * invLength;
					z = -from.y * invLength;
				}
			}
			Normalize();   // aaahhh just to be safe...
		}
		public static Quaterniond FromTo(in Vector3d vFrom, in Vector3d vTo) {
			return new Quaterniond(vFrom, vTo);
		}
		public static Quaterniond FromToConstrained(in Vector3d vFrom, in Vector3d vTo, in Vector3d vAround) {
			var fAngle = MathUtil.PlaneAngleSignedD(vFrom, vTo, vAround);
			return Quaterniond.AxisAngleD(vAround, fAngle);
		}


		public void SetToSlerp(in Quaterniond p, in Quaterniond q, in double t) {
			var cs = p.Dot(q);
			var angle = (double)Math.Acos(cs);
			if (Math.Abs(angle) >= MathUtil.ZERO_TOLERANCE) {
				var sn = (double)Math.Sin(angle);
				var invSn = 1 / sn;
				var tAngle = t * angle;
				var coeff0 = (double)Math.Sin(angle - tAngle) * invSn;
				var coeff1 = (double)Math.Sin(tAngle) * invSn;
				x = (coeff0 * p.x) + (coeff1 * q.x);
				y = (coeff0 * p.y) + (coeff1 * q.y);
				z = (coeff0 * p.z) + (coeff1 * q.z);
				w = (coeff0 * p.w) + (coeff1 * q.w);
			}
			else {
				x = p.x;
				y = p.y;
				z = p.z;
				w = p.w;
			}
		}
		public static Quaterniond Slerp(in Quaterniond p, in Quaterniond q, in double t) {
			return new Quaterniond(p, q, t);
		}


		public void SetFromRotationMatrix(in Matrix3d rot) {
			// Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
			// article "Quaternion Calculus and Fast Animation".
			var next = new Index3i(1, 2, 0);

			var trace = rot[0, 0] + rot[1, 1] + rot[2, 2];
			double root;

			if (trace > 0) {
				// |w| > 1/2, may as well choose w > 1/2
				root = Math.Sqrt(trace + 1.0);  // 2w
				w = 0.5 * root;
				root = 0.5 / root;  // 1/(4w)
				x = (rot[2, 1] - rot[1, 2]) * root;
				y = (rot[0, 2] - rot[2, 0]) * root;
				z = (rot[1, 0] - rot[0, 1]) * root;
			}
			else {
				// |w| <= 1/2
				var i = 0;
				if (rot[1, 1] > rot[0, 0]) {
					i = 1;
				}
				if (rot[2, 2] > rot[i, i]) {
					i = 2;
				}
				var j = next[i];
				var k = next[j];

				root = Math.Sqrt(rot[i, i] - rot[j, j] - rot[k, k] + 1.0);

				var quat = new Vector3d(x, y, z);
				quat[i] = 0.5 * root;
				root = 0.5 / root;
				w = (rot[k, j] - rot[j, k]) * root;
				quat[j] = (rot[j, i] + rot[i, j]) * root;
				quat[k] = (rot[k, i] + rot[i, k]) * root;
				x = quat.x;
				y = quat.y;
				z = quat.z;
			}

			Normalize();   // we prefer normalized quaternions...
		}





		public bool EpsilonEqual(in Quaterniond q2, in double epsilon) {
			return Math.Abs(x - q2.x) <= epsilon &&
				   Math.Abs(y - q2.y) <= epsilon &&
				   Math.Abs(z - q2.z) <= epsilon &&
				   Math.Abs(w - q2.w) <= epsilon;
		}


		// [TODO] should we be normalizing in these casts??
		public static implicit operator Quaterniond(in Quaternionf q) => new(q.x, q.y, q.z, q.w);
		public static explicit operator Quaternionf(in Quaterniond q) => new((float)q.x, (float)q.y, (float)q.z, (float)q.w);


		public override string ToString() {
			return string.Format("{0:F8} {1:F8} {2:F8} {3:F8}", x, y, z, w);
		}
		public string ToString(in string fmt) {
			return string.Format("{0} {1} {2} {3}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt), w.ToString(fmt));
		}
	}
}
