using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using RNumerics;
using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct Frame3f
	{
		[Key(0)]
		Quaternionf _rotation;
		[Key(1)]
		Vector3f _origin;
		[IgnoreMember]
		static readonly public Frame3f Identity = new(Vector3f.Zero, Quaternionf.Identity);

		public Frame3f() {
			_origin = new Vector3f();
			_rotation = new Quaternionf();
		}

		public Frame3f(in Frame3f copy) {
			_rotation = copy._rotation;
			_origin = copy._origin;
		}

		public Frame3f(in Vector3f origin) {
			_rotation = Quaternionf.Identity;
			_origin = origin;
		}
		public Frame3f(in Vector3d origin) {
			_rotation = Quaternionf.Identity;
			_origin = (Vector3f)origin;
		}

		public Frame3f(in Vector3f origin, in Vector3f setZ) {
			_rotation = Quaternionf.FromTo(Vector3f.AxisZ, setZ);
			_origin = origin;
		}

		public Frame3f(in Vector3d origin, in Vector3d setZ) {
			_rotation = Quaternionf.FromTo(Vector3f.AxisZ, (Vector3f)setZ);
			_origin = (Vector3f)origin;
		}


		public Frame3f(in Vector3f origin, in Vector3f setAxis, in int nAxis) {
			_rotation = nAxis == 0
				? Quaternionf.FromTo(Vector3f.AxisX, setAxis)
				: nAxis == 1 ? Quaternionf.FromTo(Vector3f.AxisY, setAxis) : Quaternionf.FromTo(Vector3f.AxisZ, setAxis);

			_origin = origin;
		}

		public Frame3f(in Vector3f origin, in Quaternionf orientation) {
			_rotation = orientation;
			_origin = origin;
		}

		public Frame3f(in Vector3f origin, in Vector3f x, in Vector3f y, in Vector3f z) {
			_origin = origin;
			var m = new Matrix3f(x, y, z, false);
			_rotation = m.ToQuaternion();
		}


		[IgnoreMember]
		public Quaternionf Rotation
		{
			get => _rotation;
			set => _rotation = value;
		}

		[IgnoreMember]
		public Vector3f Origin
		{
			get => _origin;
			set => _origin = value;
		}

		[IgnoreMember]
		public Vector3f X => _rotation.AxisX;
		[IgnoreMember]
		public Vector3f Y => _rotation.AxisY;
		[IgnoreMember]
		public Vector3f Z => _rotation.AxisZ;

		public Vector3f GetAxis(in int nAxis) {
			return nAxis == 0
				? _rotation * Vector3f.AxisX
				: nAxis == 1
					? _rotation * Vector3f.AxisY
					: nAxis == 2 ? _rotation * Vector3f.AxisZ : throw new ArgumentOutOfRangeException("nAxis");
		}


		public void Translate(in Vector3f v) {
			_origin += v;
		}
		public Frame3f Translated(in Vector3f v) {
			return new Frame3f(_origin + v, _rotation);
		}
		public Frame3f Translated(in float fDistance, in int nAxis) {
			return new Frame3f(_origin + (fDistance * GetAxis(nAxis)), _rotation);
		}

		public void Scale(in float f) {
			_origin *= f;
		}
		public void Scale(in Vector3f scale) {
			_origin *= scale;
		}
		public Frame3f Scaled(in float f) {
			return new Frame3f(f * _origin, _rotation);
		}
		public Frame3f Scaled(in Vector3f scale) {
			return new Frame3f(scale * _origin, _rotation);
		}

		public void Rotate(in Quaternionf q) {
			_rotation = q * _rotation;
		}
		public Frame3f Rotated(in Quaternionf q) {
			return new Frame3f(_origin, q * _rotation);
		}
		public Frame3f Rotated(in float fAngle, in int nAxis) {
			return Rotated(new Quaternionf(GetAxis(nAxis), fAngle));
		}

		/// <summary>
		/// this rotates the frame around its own axes, rather than around the world axes,
		/// which is what Rotate() does. So, RotateAroundAxis(AxisAngleD(Z,180)) is equivalent
		/// to Rotate(AxisAngleD(My_AxisZ,180)). 
		/// </summary>
		public void RotateAroundAxes(in Quaternionf q) {
			_rotation *= q;
		}

		public void RotateAround(in Vector3f point, in Quaternionf q) {
			var dv = q * (_origin - point);
			_rotation = q * _rotation;
			_origin = point + dv;
		}
		public Frame3f RotatedAround(in Vector3f point, in Quaternionf q) {
			var dv = q * (_origin - point);
			return new Frame3f(point + dv, q * _rotation);
		}

		public void AlignAxis(in int nAxis, in Vector3f vTo) {
			var rot = Quaternionf.FromTo(GetAxis(nAxis), vTo);
			Rotate(rot);
		}
		public void ConstrainedAlignAxis(in int nAxis, in Vector3f vTo, in Vector3f vAround) {
			var axis = GetAxis(nAxis);
			var fAngle = MathUtil.PlaneAngleSignedD(axis, vTo, vAround);
			var rot = Quaternionf.AxisAngleD(vAround, fAngle);
			Rotate(rot);
		}

		/// <summary>
		/// 3D projection of point p onto frame-axis plane orthogonal to normal axis
		/// </summary>
		public Vector3f ProjectToPlane(in Vector3f p, in int nNormal) {
			var d = p - _origin;
			var n = GetAxis(nNormal);
			return _origin + (d - (d.Dot(n) * n));
		}

		/// <summary>
		/// map from 2D coordinates in frame-axes plane perpendicular to normal axis, to 3D
		/// [TODO] check that mapping preserves orientation?
		/// </summary>
		public Vector3f FromPlaneUV(in Vector2f v, in int nPlaneNormalAxis) {
			var dv = new Vector3f(v[0], v[1], 0);
			if (nPlaneNormalAxis == 0) {
				dv[0] = 0;
				dv[2] = v[0];
			}
			else if (nPlaneNormalAxis == 1) {
				dv[1] = 0;
				dv[2] = v[1];
			}
			return (_rotation * dv) + _origin;
		}


		/// <summary>
		/// Project p onto plane axes
		/// [TODO] check that mapping preserves orientation?
		/// </summary>
		public Vector2f ToPlaneUV(in Vector3f p, in int nNormal) {
			int nAxis0 = 0, nAxis1 = 1;
			if (nNormal == 0) {
				nAxis0 = 2;
			}
			else if (nNormal == 1) {
				nAxis1 = 2;
			}

			var d = p - _origin;
			var fu = d.Dot(GetAxis(nAxis0));
			var fv = d.Dot(GetAxis(nAxis1));
			return new Vector2f(fu, fv);
		}


		///<summary> distance from p to frame-axes-plane perpendicular to normal axis </summary>
		public float DistanceToPlane(in Vector3f p, in int nNormal) {
			return Math.Abs((p - _origin).Dot(GetAxis(nNormal)));
		}
		///<summary> signed distance from p to frame-axes-plane perpendicular to normal axis </summary>
		public float DistanceToPlaneSigned(in Vector3f p, in int nNormal) {
			return (p - _origin).Dot(GetAxis(nNormal));
		}


		///<summary> Map point *into* local coordinates of Frame </summary>
		public Vector3f ToFrameP(in Vector3f v) {
			var x = new Vector3f(v.x - _origin.x, v.y - _origin.y, v.z - _origin.z);
			return _rotation.InverseMultiply(ref x);
		}
		///<summary> Map point *into* local coordinates of Frame </summary>
		public Vector3d ToFrameP(in Vector3d v) {
			var x = new Vector3d(v.x - _origin.x, v.y - _origin.y, v.z - _origin.z);
			return _rotation.InverseMultiply(ref x);
		}

		/// <summary> Map point *from* local frame coordinates into "world" coordinates </summary>
		public Vector3f FromFrameP(in Vector3f v) {
			return (_rotation * v) + _origin;
		}

		/// <summary> Map point *from* local frame coordinates into "world" coordinates </summary>
		public Vector3d FromFrameP(in Vector3d v) {
			return (_rotation * v) + _origin;
		}


		///<summary> Map vector *into* local coordinates of Frame </summary>
		public Vector3f ToFrameV(Vector3f v) {
			return _rotation.InverseMultiply(ref v);
		}
		///<summary> Map vector *into* local coordinates of Frame </summary>
		public Vector3f ToFrameV(ref Vector3f v) {
			return _rotation.InverseMultiply(ref v);
		}
		///<summary> Map vector *into* local coordinates of Frame </summary>
		public Vector3d ToFrameV(Vector3d v) {
			return _rotation.InverseMultiply(ref v);
		}
		///<summary> Map vector *into* local coordinates of Frame </summary>
		public Vector3d ToFrameV(ref Vector3d v) {
			return _rotation.InverseMultiply(ref v);
		}
		/// <summary> Map vector *from* local frame coordinates into "world" coordinates </summary>
		public Vector3f FromFrameV(Vector3f v) {
			return _rotation * v;
		}
		/// <summary> Map vector *from* local frame coordinates into "world" coordinates </summary>
		public Vector3f FromFrameV(ref Vector3f v) {
			return _rotation * v;
		}
		/// <summary> Map vector *from* local frame coordinates into "world" coordinates </summary>
		public Vector3d FromFrameV(ref Vector3d v) {
			return _rotation * v;
		}
		/// <summary> Map vector *from* local frame coordinates into "world" coordinates </summary>
		public Vector3d FromFrameV(Vector3d v) {
			return _rotation * v;
		}



		///<summary> Map quaternion *into* local coordinates of Frame </summary>
		public Quaternionf ToFrame(Quaternionf q) {
			return _rotation.Inverse * q;
		}
		///<summary> Map quaternion *into* local coordinates of Frame </summary>
		public Quaternionf ToFrame(ref Quaternionf q) {
			return _rotation.Inverse * q;
		}
		/// <summary> Map quaternion *from* local frame coordinates into "world" coordinates </summary>
		public Quaternionf FromFrame(Quaternionf q) {
			return _rotation * q;
		}
		/// <summary> Map quaternion *from* local frame coordinates into "world" coordinates </summary>
		public Quaternionf FromFrame(ref Quaternionf q) {
			return _rotation * q;
		}


		///<summary> Map ray *into* local coordinates of Frame </summary>
		public Ray3f ToFrame(Ray3f r) {
			return new Ray3f(ToFrameP(r.Origin), ToFrameV(ref r.Direction));
		}
		///<summary> Map ray *into* local coordinates of Frame </summary>
		public Ray3f ToFrame(ref Ray3f r) {
			return new Ray3f(ToFrameP(r.Origin), ToFrameV(ref r.Direction));
		}
		/// <summary> Map ray *from* local frame coordinates into "world" coordinates </summary>
		public Ray3f FromFrame(Ray3f r) {
			return new Ray3f(FromFrameP(r.Origin), FromFrameV(ref r.Direction));
		}
		/// <summary> Map ray *from* local frame coordinates into "world" coordinates </summary>
		public Ray3f FromFrame(ref Ray3f r) {
			return new Ray3f(FromFrameP(r.Origin), FromFrameV(ref r.Direction));
		}


		///<summary> Map frame *into* local coordinates of Frame </summary>
		public Frame3f ToFrame(Frame3f f) {
			return new Frame3f(ToFrameP(f._origin), ToFrame(ref f._rotation));
		}
		///<summary> Map frame *into* local coordinates of Frame </summary>
		public Frame3f ToFrame(ref Frame3f f) {
			return new Frame3f(ToFrameP(f._origin), ToFrame(ref f._rotation));
		}
		/// <summary> Map frame *from* local frame coordinates into "world" coordinates </summary>
		public Frame3f FromFrame(Frame3f f) {
			return new Frame3f(FromFrameP(f._origin), FromFrame(ref f._rotation));
		}
		/// <summary> Map frame *from* local frame coordinates into "world" coordinates </summary>
		public Frame3f FromFrame(ref Frame3f f) {
			return new Frame3f(FromFrameP(f._origin), FromFrame(ref f._rotation));
		}


		///<summary> Map box *into* local coordinates of Frame </summary>
		public Box3f ToFrame(ref Box3f box) {
			box.Center = ToFrameP(box.Center);
			box.AxisX = ToFrameV(ref box.AxisX);
			box.AxisY = ToFrameV(ref box.AxisY);
			box.AxisZ = ToFrameV(ref box.AxisZ);
			return box;
		}
		/// <summary> Map box *from* local frame coordinates into "world" coordinates </summary>
		public Box3f FromFrame(ref Box3f box) {
			box.Center = FromFrameP(box.Center);
			box.AxisX = FromFrameV(ref box.AxisX);
			box.AxisY = FromFrameV(ref box.AxisY);
			box.AxisZ = FromFrameV(ref box.AxisZ);
			return box;
		}
		///<summary> Map box *into* local coordinates of Frame </summary>
		public Box3d ToFrame(ref Box3d box) {
			box.Center = ToFrameP(box.Center);
			box.AxisX = ToFrameV(ref box.AxisX);
			box.AxisY = ToFrameV(ref box.AxisY);
			box.AxisZ = ToFrameV(ref box.AxisZ);
			return box;
		}
		/// <summary> Map box *from* local frame coordinates into "world" coordinates </summary>
		public Box3d FromFrame(ref Box3d box) {
			box.Center = FromFrameP(box.Center);
			box.AxisX = FromFrameV(ref box.AxisX);
			box.AxisY = FromFrameV(ref box.AxisY);
			box.AxisZ = FromFrameV(ref box.AxisZ);
			return box;
		}


		/// <summary>
		/// Compute intersection of ray with plane passing through frame origin, normal
		/// to the specified axis. 
		/// If the ray is parallel to the plane, no intersection can be found, and
		/// we return Vector3f.Invalid
		/// </summary>
		public Vector3f RayPlaneIntersection(in Vector3f ray_origin, in Vector3f ray_direction, in int nAxisAsNormal) {
			var N = GetAxis(nAxisAsNormal);
			var d = -Vector3f.Dot(Origin, N);
			var div = Vector3f.Dot(ray_direction, N);
			if (MathUtil.EpsilonEqual(div, 0, MathUtil.ZERO_TOLERANCEF)) {
				return Vector3f.Invalid;
			}

			var t = -(Vector3f.Dot(ray_origin, N) + d) / div;
			return ray_origin + (t * ray_direction);
		}


		/// <summary>
		/// Interpolate between two frames - Lerp for origin, Slerp for rotation
		/// </summary>
		public static Frame3f Interpolate(in Frame3f f1, in Frame3f f2, in float t) {
			return new Frame3f(
				Vector3f.Lerp(f1._origin, f2._origin, t),
				Quaternionf.Slerp(f1._rotation, f2._rotation, t));
		}



		public bool EpsilonEqual(in Frame3f f2, in float epsilon) {
			return _origin.EpsilonEqual(f2._origin, epsilon) &&
				_rotation.EpsilonEqual(f2._rotation, epsilon);
		}


		public override string ToString() {
			return ToString("F4");
		}
		public string ToString(in string fmt) {
			return string.Format("[Frame3f: Origin={0}, X={1}, Y={2}, Z={3}]", Origin.ToString(fmt), X.ToString(fmt), Y.ToString(fmt), Z.ToString(fmt));
		}



		// finds minimal rotation that aligns source frame with axes of target frame.
		// considers all signs
		//   1) find smallest angle(axis_source, axis_target), considering all sign permutations
		//   2) rotate source to align axis_source with sign*axis_target
		//   3) now rotate around alined_axis_source to align second-best pair of axes
		public static Frame3f SolveMinRotation(in Frame3f source, in Frame3f target) {
			int best_i = -1, best_j = -1;
			double fMaxAbsDot = 0, fMaxSign = 0;
			for (var i = 0; i < 3; ++i) {
				for (var j = 0; j < 3; ++j) {
					double d = source.GetAxis(i).Dot(target.GetAxis(j));
					var a = Math.Abs(d);
					if (a > fMaxAbsDot) {
						fMaxAbsDot = a;
						fMaxSign = Math.Sign(d);
						best_i = i;
						best_j = j;
					}
				}
			}

			var R1 = source.Rotated(
				Quaternionf.FromTo(source.GetAxis(best_i), (float)fMaxSign * target.GetAxis(best_j)));
			var vAround = R1.GetAxis(best_i);

			int second_i = -1, second_j = -1;
			double fSecondDot = 0, fSecondSign = 0;
			for (var i = 0; i < 3; ++i) {
				if (i == best_i) {
					continue;
				}

				for (var j = 0; j < 3; ++j) {
					if (j == best_j) {
						continue;
					}

					double d = R1.GetAxis(i).Dot(target.GetAxis(j));
					var a = Math.Abs(d);
					if (a > fSecondDot) {
						fSecondDot = a;
						fSecondSign = Math.Sign(d);
						second_i = i;
						second_j = j;
					}
				}
			}

			R1.ConstrainedAlignAxis(second_i, (float)fSecondSign * target.GetAxis(second_j), vAround);

			return R1;
		}


	}
}
