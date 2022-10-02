using MessagePack;

using System;
using System.Numerics;




namespace RNumerics
{
	// mostly ported from WildMagic5 Wm5Quaternion, from geometrictools.com
	[MessagePackObject]
	public struct Quaternionf : IComparable<Quaternionf>, IEquatable<Quaternionf>
	{

		[Key(1)]
		public float x;
		[Key(2)]
		public float y;
		[Key(3)]
		public float z;
		[Key(0)]
		public float w;
		public Quaternionf() {
			x = 0f;
			y = 0f;
			z = 0f;
			w = 1f;
		}

		public unsafe Quaternion ToSystemNumric() {
			fixed (Quaternionf* vector3f = &this) {
				return *(Quaternion*)vector3f;
			}
		}
		public static unsafe Quaternionf ToRhuNumrics(ref Quaternion value) {
			fixed (Quaternion* vector3f = &value) {
				return *(Quaternionf*)vector3f;
			}
		}
		public static explicit operator Quaternion(in Quaternionf b) => b.ToSystemNumric();

		public static explicit operator Quaternionf(Quaternion b) => ToRhuNumrics(ref b);

		public Quaternionf(in float x, in float y, in float z, in float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Quaternionf(in float[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }
		public Quaternionf(in Quaternionf q2) { x = q2.x; y = q2.y; z = q2.z; w = q2.w; }

		public Quaternionf(in Vector3f axis, in float AngleDeg) {
			x = y = z = 0;
			w = 1;
			SetAxisAngleD(axis, AngleDeg);
		}
		public Quaternionf(in Vector3f vFrom, in Vector3f vTo) {
			x = y = z = 0;
			w = 1;
			SetFromTo(vFrom, vTo);
		}
		public Quaternionf(in Quaternionf p, in Quaternionf q, in float t) {
			x = y = z = 0;
			w = 1;
			SetToSlerp(p, q, t);
		}
		public Quaternionf(in Matrix3f mat) {
			x = y = z = 0;
			w = 1;
			SetFromRotationMatrix(mat);
		}
		[IgnoreMember]
		static public readonly Quaternionf Zero = new(0.0f, 0.0f, 0.0f, 0.0f);
		[IgnoreMember]
		static public readonly Quaternionf Identity = new(0.0f, 0.0f, 0.0f, 1.0f);
		[IgnoreMember]
		static public readonly Quaternionf Pitched = CreateFromEuler(0,90,0);
		[IgnoreMember]
		static public readonly Quaternionf Pitched180 = CreateFromEuler(0, 180, 0);
		[IgnoreMember]
		static public readonly Quaternionf Yawed = CreateFromEuler(90, 0, 0);
		[IgnoreMember]
		static public readonly Quaternionf Yawed180 = CreateFromEuler(180, 0, 0);
		[IgnoreMember]
		static public readonly Quaternionf Rolled = CreateFromEuler(0, 0, 90);
		[IgnoreMember]
		static public readonly Quaternionf Rolled180 = CreateFromEuler(0, 0, 180);
		[IgnoreMember]
		public float this[in int key]
		{
			get => key != 0 ? key == 1 ? y : key == 2 ? z : w : x;
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
		public float LengthSquared => (x * x) + (y * y) + (z * z) + (w * w);
		[IgnoreMember]
		public float Length => (float)Math.Sqrt((x * x) + (y * y) + (z * z) + (w * w));

		public float Normalize(in float epsilon = 0) {
			var length = Length;
			if (length > epsilon) {
				var invLength = 1.0f / length;
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
		public Quaternionf Normalized
		{
			get { var q = new Quaternionf(this); q.Normalize(); return q; }
		}
		public float Dot(in Quaternionf q2) {
			return (x * q2.x) + (y * q2.y) + (z * q2.z) + (w * q2.w);
		}




		public static Quaternionf operator *(in Quaternionf a, in Quaternionf b) {
			var w = (a.w * b.w) - (a.x * b.x) - (a.y * b.y) - (a.z * b.z);
			var x = (a.w * b.x) + (a.x * b.w) + (a.y * b.z) - (a.z * b.y);
			var y = (a.w * b.y) + (a.y * b.w) + (a.z * b.x) - (a.x * b.z);
			var z = (a.w * b.z) + (a.z * b.w) + (a.x * b.y) - (a.y * b.x);
			return new Quaternionf(x, y, z, w);
		}


		public static Quaternionf operator -(in Quaternionf q1, in Quaternionf q2) => new(q1.x - q2.x, q1.y - q2.y, q1.z - q2.z, q1.w - q2.w);

		public static Vector3f operator *(in Quaternionf q, in Vector3f v) {
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
			return new Vector3f(
				(v.x * (1 - (twoYY + twoZZ))) + (v.y * (twoXY - twoWZ)) + (v.z * (twoXZ + twoWY)),
				(v.x * (twoXY + twoWZ)) + (v.y * (1 - (twoXX + twoZZ))) + (v.z * (twoYZ - twoWX)),
				(v.x * (twoXZ - twoWY)) + (v.y * (twoYZ + twoWX)) + (v.z * (1 - (twoXX + twoYY))));
			;
		}

		// so convenient
		public static Vector3d operator *(in Quaternionf q, in Vector3d v) {
			//return q.ToRotationMatrix() * v;
			// inline-expansion of above:
			double twoX = 2 * q.x;
			double twoY = 2 * q.y;
			double twoZ = 2 * q.z;
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



		/// <summary> Inverse() * v </summary>
		public Vector3f InverseMultiply(ref Vector3f v) {
			var norm = LengthSquared;
			if (norm > 0) {
				var invNorm = 1.0f / norm;
				float qx = -x * invNorm, qy = -y * invNorm, qz = -z * invNorm, qw = w * invNorm;
				var twoX = 2 * qx;
				var twoY = 2 * qy;
				var twoZ = 2 * qz;
				var twoWX = twoX * qw;
				var twoWY = twoY * qw;
				var twoWZ = twoZ * qw;
				var twoXX = twoX * qx;
				var twoXY = twoY * qx;
				var twoXZ = twoZ * qx;
				var twoYY = twoY * qy;
				var twoYZ = twoZ * qy;
				var twoZZ = twoZ * qz;
				return new Vector3f(
					(v.x * (1 - (twoYY + twoZZ))) + (v.y * (twoXY - twoWZ)) + (v.z * (twoXZ + twoWY)),
					(v.x * (twoXY + twoWZ)) + (v.y * (1 - (twoXX + twoZZ))) + (v.z * (twoYZ - twoWX)),
					(v.x * (twoXZ - twoWY)) + (v.y * (twoYZ + twoWX)) + (v.z * (1 - (twoXX + twoYY))));
			}
			else {
				return Vector3f.Zero;
			}
		}


		/// <summary> Inverse() * v </summary>
		public Vector3d InverseMultiply(ref Vector3d v) {
			var norm = LengthSquared;
			if (norm > 0) {
				var invNorm = 1.0f / norm;
				float qx = -x * invNorm, qy = -y * invNorm, qz = -z * invNorm, qw = w * invNorm;
				double twoX = 2 * qx;
				double twoY = 2 * qy;
				double twoZ = 2 * qz;
				var twoWX = twoX * qw;
				var twoWY = twoY * qw;
				var twoWZ = twoZ * qw;
				var twoXX = twoX * qx;
				var twoXY = twoY * qx;
				var twoXZ = twoZ * qx;
				var twoYY = twoY * qy;
				var twoYZ = twoZ * qy;
				var twoZZ = twoZ * qz;
				return new Vector3d(
					(v.x * (1 - (twoYY + twoZZ))) + (v.y * (twoXY - twoWZ)) + (v.z * (twoXZ + twoWY)),
					(v.x * (twoXY + twoWZ)) + (v.y * (1 - (twoXX + twoZZ))) + (v.z * (twoYZ - twoWX)),
					(v.x * (twoXZ - twoWY)) + (v.y * (twoYZ + twoWX)) + (v.z * (1 - (twoXX + twoYY))));
				;
			}
			else {
				return Vector3f.Zero;
			}
		}



		// these multiply quaternion by (1,0,0), (0,1,0), (0,0,1), respectively.
		// faster than full multiply, because of all the zeros
		[IgnoreMember]
		public Vector3f AxisX
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
				return new Vector3f(1 - (twoYY + twoZZ), twoXY + twoWZ, twoXZ - twoWY);
			}
		}
		[IgnoreMember]
		public Vector3f AxisY
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
				return new Vector3f(twoXY - twoWZ, 1 - (twoXX + twoZZ), twoYZ + twoWX);
			}
		}
		[IgnoreMember]
		public Vector3f AxisZ
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
				return new Vector3f(twoXZ + twoWY, twoYZ - twoWX, 1 - (twoXX + twoYY));
			}
		}
		[IgnoreMember]
		public Quaternionf Inverse
		{
			get {
				var norm = LengthSquared;
				if (norm > 0) {
					var invNorm = 1.0f / norm;
					return new Quaternionf(
						-x * invNorm, -y * invNorm, -z * invNorm, w * invNorm);
				}
				else {
					return Quaternionf.Zero;
				}
			}
		}
		[IgnoreMember]
		public bool IsAnyNan => float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z);

		public float Angle(in Quaternionf e) {
			return (float)Math.Acos(Math.Min(Math.Abs(Dot(e)), 1f)) * 2f * 57.29578f;
		}

		public Matrix3f ToRotationMatrix() {
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
			var m = Matrix3f.Zero;
			m[0, 0] = 1 - (twoYY + twoZZ);
			m[0, 1] = twoXY - twoWZ;
			m[0, 2] = twoXZ + twoWY;
			m[1, 0] = twoXY + twoWZ;
			m[1, 1] = 1 - (twoXX + twoZZ);
			m[1, 2] = twoYZ - twoWX;
			m[2, 0] = twoXZ - twoWY;
			m[2, 1] = twoYZ + twoWX;
			m[2, 2] = 1 - (twoXX + twoYY);
			return m;
		}



		public void SetAxisAngleD(in Vector3f axis, in float AngleDeg) {
			var angle_rad = MathUtil.DEG_2_RAD * AngleDeg;
			var halfAngle = 0.5 * angle_rad;
			var sn = Math.Sin(halfAngle);
			w = (float)Math.Cos(halfAngle);
			x = (float)(sn * axis.x);
			y = (float)(sn * axis.y);
			z = (float)(sn * axis.z);
		}
		public static Quaternionf AxisAngleD(in Vector3f axis, in float angleDeg) {
			return new Quaternionf(axis, angleDeg);
		}
		public static Quaternionf AxisAngleR(in Vector3f axis, in float angleRad) {
			return new Quaternionf(axis, angleRad * MathUtil.RAD_2_DEGF);
		}

		// this function can take non-normalized vectors vFrom and vTo (normalizes internally)
		public void SetFromTo(in Vector3f vFrom, in Vector3f vTo) {
			// [TODO] this page seems to have optimized version:
			//    http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors

			// [RMS] not ideal to explicitly normalize here, but if we don't,
			//   output quaternion is not normalized and this causes problems,
			//   eg like drift if we do repeated SetFromTo()
			Vector3f from = vFrom.Normalized, to = vTo.Normalized;
			var bisector = (from + to).Normalized;
			w = from.Dot(bisector);
			if (w != 0) {
				var cross = from.Cross(bisector);
				x = cross.x;
				y = cross.y;
				z = cross.z;
			}
			else {
				float invLength;
				if (Math.Abs(from.x) >= Math.Abs(from.y)) {
					// V1.x or V1.z is the largest magnitude component.
					invLength = (float)(1.0 / Math.Sqrt((from.x * from.x) + (from.z * from.z)));
					x = -from.z * invLength;
					y = 0;
					z = +from.x * invLength;
				}
				else {
					// V1.y or V1.z is the largest magnitude component.
					invLength = (float)(1.0 / Math.Sqrt((from.y * from.y) + (from.z * from.z)));
					x = 0;
					y = +from.z * invLength;
					z = -from.y * invLength;
				}
			}
			Normalize();   // aaahhh just to be safe...
		}
		public static Quaternionf FromTo(in Vector3f vFrom, in Vector3f vTo) {
			return new Quaternionf(vFrom, vTo);
		}
		public static Quaternionf FromTo(in Vector3f vFrom, in Vector3f vTo, in Vector3f vOffset) {
			return new Quaternionf(vFrom- vOffset, vTo- vOffset);
		}
		public static Quaternionf FromToConstrained(in Vector3f vFrom, in Vector3f vTo, in Vector3f vAround) {
			var fAngle = MathUtil.PlaneAngleSignedD(vFrom, vTo, vAround);
			return Quaternionf.AxisAngleD(vAround, fAngle);
		}

		public static Quaternionf Slerp(in Quaternionf a, Quaternionf b, in float lerp) {
			if (lerp <= 0f) {
				return a;
			}
			if (lerp >= 1f) {
				return b;
			}
			var num = (a.w * b.w) + (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
			if (num < 0f) {
				b = b.Inverse;
				num = 0f - num;
			}
			if (Math.Abs(num - 1f) < Math.Max(1E-06f * Math.Max(Math.Abs(num), Math.Abs(1f)), 5.605194E-45f)) {
				return b;
			}
			var num2 = (float)Math.Acos(num);
			var num3 = (float)Math.Sqrt(1f - (num * num));
			var num4 = (float)Math.Sin((1f - lerp) * num2) / num3;
			var num5 = (float)Math.Sin(lerp * num2) / num3;
			return new Quaternionf((a.x * num4) + (b.x * num5), (a.y * num4) + (b.y * num5), (a.z * num4) + (b.z * num5), (a.w * num4) + (b.w * num5));
		}

		public void SetToSlerp(in Quaternionf a, in Quaternionf b, in float lerp) {
			var e = Slerp(a, b, lerp);
			x = e.x;
			y = e.y;
			z = e.z;
			w = e.w;

		}


		public void SetFromRotationMatrix(in Matrix3f rot) {
			// Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
			// article "Quaternion Calculus and Fast Animation".
			var next = new Index3i(1, 2, 0);

			var trace = rot[0, 0] + rot[1, 1] + rot[2, 2];
			float root;

			if (trace > 0) {
				// |w| > 1/2, may as well choose w > 1/2
				root = (float)Math.Sqrt(trace + (float)1);  // 2w
				w = ((float)0.5) * root;
				root = ((float)0.5) / root;  // 1/(4w)
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

				root = (float)Math.Sqrt(rot[i, i] - rot[j, j] - rot[k, k] + (float)1);

				var quat = new Vector3f(x, y, z);
				quat[i] = ((float)0.5) * root;
				root = ((float)0.5) / root;
				w = (rot[k, j] - rot[j, k]) * root;
				quat[j] = (rot[j, i] + rot[i, j]) * root;
				quat[k] = (rot[k, i] + rot[i, k]) * root;
				x = quat.x;
				y = quat.y;
				z = quat.z;
			}

			Normalize();   // we prefer normalized quaternions...
		}




		public static bool operator ==(in Quaternionf a, in Quaternionf b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator !=(in Quaternionf a, in Quaternionf b) => a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;

		public override int GetHashCode() {
			unchecked // Overflow is fine, just wrap
			{
				var hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				hash = (hash * 16777619) ^ z.GetHashCode();
				hash = (hash * 16777619) ^ w.GetHashCode();
				return hash;
			}
		}
		public int CompareTo(Quaternionf other) {
			if (x != other.x) {
				return x < other.x ? -1 : 1;
			}
			else if (y != other.y) {
				return y < other.y ? -1 : 1;
			}
			else if (z != other.z) {
				return z < other.z ? -1 : 1;
			}
			else if (w != other.w) {
				return w < other.w ? -1 : 1;
			}

			return 0;
		}

		public bool Equals(Quaternionf other) {
			return this == other;
		}

		public override bool Equals(object obj) {
			return obj is Quaternionf data && data == this;
		}


		public bool EpsilonEqual(in Quaternionf q2, in float epsilon) {
			return (float)Math.Abs(x - q2.x) <= epsilon &&
				   (float)Math.Abs(y - q2.y) <= epsilon &&
				   (float)Math.Abs(z - q2.z) <= epsilon &&
				   (float)Math.Abs(w - q2.w) <= epsilon;
		}

		public static Quaternionf CreateFromYawPitchRoll(in Vector3f val) {
			return CreateFromYawPitchRoll(val.x, val.y, val.z);
		}

		/// <summary>
		/// Creates a new Quaternion from the given yaw, pitch, and roll, in radians.
		/// </summary>
		/// <param name="yaw">The yaw angle, in radians, around the Y-axis.</param>
		/// <param name="pitch">The pitch angle, in radians, around the X-axis.</param>
		/// <param name="roll">The roll angle, in radians, around the Z-axis.</param>
		/// <returns></returns>
		public static Quaternionf CreateFromYawPitchRoll(in float yaw, in float pitch, in float roll) {
			//  Roll first, about axis the object is facing, then
			//  pitch upward, then yaw to face into the new heading
			float sr, cr, sp, cp, sy, cy;

			var halfRoll = roll * 0.5f;
			sr = (float)Math.Sin(halfRoll);
			cr = (float)Math.Cos(halfRoll);

			var halfPitch = pitch * 0.5f;
			sp = (float)Math.Sin(halfPitch);
			cp = (float)Math.Cos(halfPitch);

			var halfYaw = yaw * 0.5f;
			sy = (float)Math.Sin(halfYaw);
			cy = (float)Math.Cos(halfYaw);

			Quaternionf result;

			result.x = (cy * sp * cr) + (sy * cp * sr);
			result.y = (sy * cp * cr) - (cy * sp * sr);
			result.z = (cy * cp * sr) - (sy * sp * cr);
			result.w = (cy * cp * cr) + (sy * sp * sr);

			return result;
		}
		public static Quaternionf LookAt(in Vector3f sourcePoint, in Vector3f destPoint) {
			return LookAt(sourcePoint, destPoint, Vector3f.Up);
		}
		public static Quaternionf LookAt(in Vector3f sourcePoint, in Vector3f destPoint, in Vector3f up) {
			var toVector = (destPoint - sourcePoint).Normalized;
			return LookRotation(toVector,up);
		}

		public static Quaternionf FromToRotation(in Vector3f from, in Vector3f to) {
			var num = from.Dot(to);
			if (!(from == to)) {
				var b = Vector3f.Zero;
				if (!(from == b)) {
					var b2 = Vector3f.Zero;
					if (!(to == b2)) {
						var float5 = from.Cross(to);
						if (float5.SqrMagnitude <= 1E-08f && num < 0f) {
							b = new Vector3f(1f);
							var float6 = b.Cross(from);
							if (float6.SqrMagnitude <= 1E-08f) {
								b = new Vector3f(0f, 1f);
								float6 = b.Cross(from);
							}
							return CreateFromYawPitchRoll(float6.Normalized);
						}
						return new Quaternionf(float5.x, float5.y, float5.z, (float)Math.Sqrt(from.SqrMagnitude * to.SqrMagnitude) + num).Normalized;
					}
				}
			}
			return Identity;
		}



		public static Quaternionf LookRotation(in Vector3f forward, in Vector3f up) {
			var b = Vector3f.Zero;
			if (forward == b) {
				return Identity;
			}
			var b2 = forward.Normalized;
			var a = up.Cross(b2);
			b = Vector3f.Zero;
			if (a == b) {
				var q = Yawed.Inverse;
				a = q * b2;
			}
			else {
				a = a.Normalized;
			}
			var cross = b2.Cross(a);
			var num = a.x + cross.y + b2.z;
			float neww;
			float newx;
			float newy;
			float newz;
			if (num > 0f) {
				var num2 = (float)Math.Sqrt(num + 1f);
				neww = num2 * 0.5f;
				num2 = 0.5f / num2;
				newx = (cross.z - b2.y) * num2;
				newy = (b2.x - a.z) * num2;
				newz = (a.y - cross.x) * num2;
			}
			else if (a.x >= cross.y && a.x >= b2.z) {
				var num7 = (float)Math.Sqrt(1f + a.x - cross.y - b2.z);
				var num8 = 0.5f / num7;
				newx = 0.5f * num7;
				newy = (a.y + cross.x) * num8;
				newz = (a.z + b2.x) * num8;
				neww = (cross.z - b2.y) * num8;
			}
			else if (cross.y > b2.z) {
				var num9 = (float)Math.Sqrt(1f + cross.y - a.x - b2.z);
				var num10 = 0.5f / num9;
				newx = (cross.x + a.y) * num10;
				newy = 0.5f * num9;
				newz = (b2.y + cross.z) * num10;
				neww = (b2.x - a.z) * num10;
			}
			else {
				var num11 = (float)Math.Sqrt(1f + b2.z - a.x - cross.y);
				var num12 = 0.5f / num11;
				newx = (b2.x + a.z) * num12;
				newy = (b2.y + cross.z) * num12;
				newz = 0.5f * num11;
				neww = (a.y - cross.x) * num12;
			}
			return new Quaternionf(newx, newy, newz, neww);
		}

		public static explicit operator Quaternionf(in Vector3f v) => CreateFromEuler(v.x, v.y, v.z);

		public static Quaternionf CreateFromEuler(in Vector3f v) {
			return CreateFromEuler(v.x, v.y, v.z);
		}

		public static Quaternionf CreateFromEuler(in float yaw, in float pitch, in float roll) {

			return CreateFromYawPitchRoll((float)(Math.PI / 180) * yaw, (float)(Math.PI / 180) * pitch, (float)(Math.PI / 180) * roll);
		}
		public Vector3f GetEuler() {
			var sqw = w * w;
			var sqx = x * x;
			var sqy = y * y;
			var sqz = z * z;
			var unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
			var test = (x * w) - (y * z);
			var v = Vector3f.Zero;

			if (test > 0.4995f * unit) { // singularity at north pole
				v.y = 2f * (float)Math.Atan2(y, x);
				v.x = (float)Math.PI / 2;
				v.z = 0;
				return NormalizeAngles(v * (float)(Math.PI / 180));
			}
			if (test < -0.4995f * unit) { // singularity at south pole
				v.y = -2f * (float)Math.Atan2(y, x);
				v.x = -(float)Math.PI / 2;
				v.z = 0;
				return NormalizeAngles(v * (float)(Math.PI / 180));
			}
			var q = new Quaternionf(w, z, x, y);
			v.y = (float)Math.Atan2((2f * q.x * q.w) + (2f * q.y * q.z), 1 - (2f * ((q.z * q.z) + (q.w * q.w))));     // Yaw
			v.x = (float)Math.Asin(2f * ((q.x * q.z) - (q.w * q.y)));                             // Pitch
			v.z = (float)Math.Atan2((2f * q.x * q.y) + (2f * q.z * q.w), 1 - (2f * ((q.y * q.y) + (q.z * q.z))));      // Roll
			return NormalizeAngles(v * (float)(Math.PI / 180));
		}
		static Vector3f NormalizeAngles(Vector3f angles) {
			angles.x = NormalizeAngle(angles.x);
			angles.y = NormalizeAngle(angles.y);
			angles.z = NormalizeAngle(angles.z);
			return angles;
		}
		static float NormalizeAngle(float angle) {
			while (angle > 360) {
				angle -= 360;
			}

			while (angle < 0) {
				angle += 360;
			}

			return angle;
		}
		public override string ToString() {
			return string.Format("{0:F8} {1:F8} {2:F8} {3:F8}", x, y, z, w);
		}
		public string ToString(in string fmt) {
			return string.Format("{0} {1} {2} {3}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt), w.ToString(fmt));
		}
	}
}
