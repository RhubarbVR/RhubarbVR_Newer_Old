using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Vector2b : IComparable<Vector2b>, IEquatable<Vector2b>
	{
		[Key(0)]
		public bool x;
		[Key(1)]
		public bool y;

		public Vector2b() {
			x = false;
			y = false;
		}

		public Vector2b(in bool f) { x = y = f; }
		public Vector2b(in bool x, in bool y) { this.x = x; this.y = y; }
		public Vector2b(in bool[] v2) { x = v2[0]; y = v2[1]; }

		[IgnoreMember]
		static public readonly Vector2b True = new (true, true);
		[IgnoreMember]
		static public readonly Vector2b False = new (false, false);

		public bool this[in int key]
		{
			get => (key == 0) ? x : y;
			set { if (key == 0) { x = value; } else {
					y = value;
				}
			}
		}





		public void Set(in Vector2b o)
		{
			x = o.x;
			y = o.y;
		}
		public void Set(in bool fX, in bool fY)
		{
			x = fX;
			y = fY;
		}



		public static bool operator ==(in Vector2b a, in Vector2b b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(in Vector2b a, in Vector2b b) => a.x != b.x || a.y != b.y;
		public override bool Equals(object obj)
		{
			return this == (Vector2b)obj;
		}
		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				var hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				return hash;
			}
		}
		public int CompareTo(Vector2b other)
		{
			return 0;
		}
		public bool Equals(Vector2b other)
		{
			return x == other.x && y == other.y;
		}


		public override string ToString()
		{
			return string.Format("{0} {1}", x, y);
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}
	}

}