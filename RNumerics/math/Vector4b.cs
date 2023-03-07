using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RNumerics
{
	public struct Vector4b : IComparable<Vector4b>, IEquatable<Vector4b>, ISerlize<Vector4b>
	{
		public bool x;
		public bool y;
		public bool z;
		public bool w;


		public void Serlize(BinaryWriter binaryWriter) {
			byte data = 0;
			if (x) {
				data |= 1;
			}
			if (y) {
				data |= 2;
			}
			if (z) {
				data |= 4;
			}
			if (w) {
				data |= 8;
			}
			binaryWriter.Write(data);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			var data = binaryReader.ReadByte();
			if ((data & 1) != 0) {
				x = true;
			}
			if ((data & 2) != 0) {
				y = true;
			}
			if ((data & 4) != 0) {
				z = true;
			}
			if ((data & 8) != 0) {
				w = true;
			}
		}

		[Exposed]
		public bool X
		{
			get => x;
			set => x = value;
		}
		[Exposed]
		public bool Y
		{
			get => y;
			set => y = value;
		}
		[Exposed]
		public bool Z
		{
			get => z;
			set => z = value;
		}
		[Exposed]
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

		[Exposed]
		static public readonly Vector4b True = new(true, true, true, true);
		[Exposed]
		static public readonly Vector4b False = new(false, false, false, false);

		
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