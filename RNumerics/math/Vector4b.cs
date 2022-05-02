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


		public Vector4b(bool f) { x = y = z = w = f; }
		public Vector4b(bool x, bool y, bool z, bool w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Vector4b(bool[] v4) { x = v4[0]; y = v4[1]; z = v4[2]; w = v4[3]; }

		[IgnoreMember]
		static public readonly Vector4b True = new(true, true, true, true);
		[IgnoreMember]
		static public readonly Vector4b False = new(false, false, false, false);

		[IgnoreMember]
		public bool this[int key]
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





		public void Set(Vector2b o) {
			x = o.x;
			y = o.y;
		}
		public void Set(bool fX, bool fY) {
			x = fX;
			y = fY;
		}



		public static bool operator ==(Vector4b a, Vector4b b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator !=(Vector4b a, Vector4b b) => a.x != b.x || (a.y != b.y && a.z != b.z && a.w != b.w);
		public override bool Equals(object obj) {
			return this == (Vector4b)obj;
		}
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