using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Vector3b : IComparable<Vector3b>, IEquatable<Vector3b>
	{
		[Key(0)]
		public bool x;
		[Key(1)]
		public bool y;
		[Key(2)]
		public bool z;

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
		public Vector3b() {
			x = false;
			y = false;
			z = false;
		}

		public Vector3b(in bool f) { x = y = z = f; }
		public Vector3b(in bool x, in bool y, in bool z) { this.x = x; this.y = y; this.z = z; }
		public Vector3b(in bool[] v3) { x = v3[0]; y = v3[1]; z = v3[2]; }

		[Exposed,IgnoreMember]
		static public readonly Vector3b True = new(true, true, true);
		[Exposed,IgnoreMember]
		static public readonly Vector3b False = new(false, false, false);
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



		public static bool operator ==(in Vector3b a, in Vector3b b) => a.x == b.x && a.y == b.y && a.z == b.z;
		public static bool operator !=(in Vector3b a, in Vector3b b) => a.x != b.x || (a.y != b.y && a.z != b.z);
		public override bool Equals(object obj) {
			return this == (Vector3b)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y, z);
		}

		public int CompareTo(Vector3b other) {
			return 0;
		}
		public bool Equals(Vector3b other) {
			return x == other.x && y == other.y && z == other.z;
		}


		public override string ToString() {
			return string.Format("{0} {1} {2}", x, y, z);
		}
	}

}