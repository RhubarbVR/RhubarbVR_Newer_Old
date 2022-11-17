using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct Triangle2d
	{
		[Key(0)]
		public Vector2d v0;
		[Key(1)]
		public Vector2d v1;
		[Key(2)]
		public Vector2d v2;

		[Exposed, IgnoreMember]
		public Vector2d V0
		{
			get => v0;
			set => v0 = value;
		}
		[Exposed, IgnoreMember]
		public Vector2d V1
		{
			get => v1;
			set => v1 = value;
		}
		[Exposed, IgnoreMember]
		public Vector2d V2
		{
			get => v2;
			set => v2 = value;
		}

		public Triangle2d() {
			v0 = new Vector2d(0, 0);
			v1 = new Vector2d(0, 0);
			v2 = new Vector2d(0, 0);
		}

		public Triangle2d(in Vector2d v0, in Vector2d v1, in Vector2d v2) {
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
		}

		public Vector2d this[in int key]
		{
			get => (key == 0) ? v0 : (key == 1) ? v1 : v2;
			set {
				if (key == 0) { v0 = value; }
				else if (key == 1) { v1 = value; }
				else {
					v2 = value;
				}
			}
		}

		public Vector2d PointAt(in double bary0, in double bary1, in double bary2) {
			return (bary0 * v0) + (bary1 * v1) + (bary2 * v2);
		}
		public Vector2d PointAt(in Vector3d bary) {
			return (bary.x * v0) + (bary.y * v1) + (bary.z * v2);
		}

		// conversion operators
		public static implicit operator Triangle2d(in Triangle2f v) => new(v.V0, v.V1, v.V2);
		public static explicit operator Triangle2f(in Triangle2d v) => new((Vector2f)v.v0, (Vector2f)v.v1, (Vector2f)v.v2);
	}


	[MessagePackObject]
	public struct Triangle2f
	{
		[Key(0)]
		public Vector2f V0;
		[Key(1)]
		public Vector2f V1;
		[Key(2)]
		public Vector2f V2;

		public Triangle2f(in Vector2f v0, in Vector2f v1, in Vector2f v2) {
			V0 = v0;
			V1 = v1;
			V2 = v2;
		}

		public Vector2f this[in int key]
		{
			get => (key == 0) ? V0 : (key == 1) ? V1 : V2;
			set {
				if (key == 0) {
					V0 = value;
				}
				else if (key == 1) { V1 = value; }
				else {
					V2 = value;
				}
			}
		}


		public Vector2f PointAt(in float bary0, in float bary1, in float bary2) {
			return (bary0 * V0) + (bary1 * V1) + (bary2 * V2);
		}
		public Vector2f PointAt(in Vector3f bary) {
			return (bary.x * V0) + (bary.y * V1) + (bary.z * V2);
		}
	}

}
