using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Triangle3d
	{
		[Key(0)]
		public Vector3d V0;
		[Key(1)]
		public Vector3d V1;
		[Key(2)]
		public Vector3d V2;

		public Triangle3d() {
			V0 = new Vector3d(0, 0, 0);
			V1 = new Vector3d(0, 0, 0);
			V2 = new Vector3d(0, 0, 0);
		}

		public Triangle3d(in Vector3d v0, in Vector3d v1, in Vector3d v2) {
			V0 = v0;
			V1 = v1;
			V2 = v2;
		}

		public Vector3d this[in int key]
		{
			get => (key == 0) ? V0 : (key == 1) ? V1 : V2;
			set {
				if (key == 0) { V0 = value; }
				else if (key == 1) { V1 = value; }
				else {
					V2 = value;
				}
			}
		}
		[IgnoreMember]
		public Vector3d Normal => MathUtil.Normal(V0, V1, V2);
		[IgnoreMember]
		public double Area => MathUtil.Area(V0, V1, V2);
		[IgnoreMember]
		public double AspectRatio => MathUtil.AspectRatio( V0,  V1,  V2);

		public Vector3d PointAt(in double bary0, in double bary1, in double bary2) {
			return (bary0 * V0) + (bary1 * V1) + (bary2 * V2);
		}
		public Vector3d PointAt(in Vector3d bary) {
			return (bary.x * V0) + (bary.y * V1) + (bary.z * V2);
		}

		public Vector3d BarycentricCoords(in Vector3d point) {
			return MathUtil.BarycentricCoords(point, V0, V1, V2);
		}

		// conversion operators
		public static implicit operator Triangle3d(in Triangle3f v) => new(v.V0, v.V1, v.V2);
		public static explicit operator Triangle3f(in Triangle3d v) => new((Vector3f)v.V0, (Vector3f)v.V1, (Vector3f)v.V2);
	}


	[MessagePackObject]
	public struct Triangle3f
	{
		[Key(0)]
		public Vector3f V0;
		[Key(1)]
		public Vector3f V1;
		[Key(2)]
		public Vector3f V2;

		public Triangle3f(in Vector3f v0, in Vector3f v1, in Vector3f v2) {
			V0 = v0;
			V1 = v1;
			V2 = v2;
		}

		public Vector3f this[in int key]
		{
			get => (key == 0) ? V0 : (key == 1) ? V1 : V2;
			set {
				if (key == 0) { V0 = value; }
				else if (key == 1) { V1 = value; }
				else {
					V2 = value;
				}
			}
		}


		public Vector3f PointAt(in float bary0, in float bary1, in float bary2) {
			return (bary0 * V0) + (bary1 * V1) + (bary2 * V2);
		}
		public Vector3f PointAt(in Vector3f bary) {
			return (bary.x * V0) + (bary.y * V1) + (bary.z * V2);
		}

		public Vector3f BarycentricCoords(in Vector3f point) {
			return (Vector3f)MathUtil.BarycentricCoords(point, V0, V1, V2);
		}
	}

}
