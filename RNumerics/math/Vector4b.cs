using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Vector4b : IComparable<Vector4b>, IEquatable<Vector4b>
	{
		[Key(0)]
		public bool x;
		[Key(1)]
		public bool y;
		[Key(2)]
		public bool z;
		[Key(3)]
		public bool w;

		[Exposed, IgnoreMember]
		public bool X
		{
			get => x;
			set => x = value;
		}
		[Exposed, IgnoreMember]
		public bool Y
		{
			get => y;
			set => y = value;
		}
		[Exposed, IgnoreMember]
		public bool Z
		{
			get => z;
			set => z = value;
		}
		[Exposed, IgnoreMember]
		public bool W
		{
			get => w;
			set => w = value;
		}

		public Vector4b() {
			x = false;
			y = false;
			z = false;
			w = false;
		}

		public Vector4b(in bool f) { x = y = z = w = f; }
		public Vector4b(in bool x, in bool y, in bool z, in bool w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Vector4b(in bool[] v4) { x = v4[0]; y = v4[1]; z = v4[2]; w = v4[3]; }

		[Exposed,IgnoreMember]
		static public readonly Vector4b True = new(true, true, true, true);
		[Exposed,IgnoreMember]
		static public readonly Vector4b False = new(false, false, false, false);

		[IgnoreMember]
		public bool this[in int key]
		{
			get => (key == 0) ? x : y;
			set {
				if (key == 0) {
					x = value;
				}
				else {
					y = value;
				}
			}
		}





		public void Set(in Vector2b o) {
			x = o.x;
			y = o.y;
		}
		public void Set(in bool fX, in bool fY) {
			x = fX;
			y = fY;
		}



		public static bool operator ==(in Vector4b a, in Vector4b b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator !=(in Vector4b a, in Vector4b b) => a.x != b.x || (a.y != b.y && a.z != b.z && a.w != b.w);
		public override bool Equals(object obj) {
			return this == (Vector4b)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y, z, w);
		}

		public int CompareTo(Vector4b other) {
			return 0;
		}
		public bool Equals(Vector4b other) {
			return x == other.x && y == other.y && z == other.z && w == other.w;
		}


		public override string ToString() {
			return string.Format("{0} {1} {2} {3}", x, y, z, w);
		}
	}

}