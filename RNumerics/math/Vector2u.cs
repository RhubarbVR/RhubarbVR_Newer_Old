using System;
using System.IO;

namespace RNumerics
{
	public struct Vector2u : IComparable<Vector2u>, IEquatable<Vector2u>, ISerlize<Vector2u>
	{
		public uint x;
		public uint y;

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(x);
			binaryWriter.Write(y);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			x = binaryReader.ReadUInt32();
			y = binaryReader.ReadUInt32();
		}

		[Exposed]
		public uint X
		{
			get => x;
			set => x = value;
		}
		[Exposed]
		public uint Y
		{
			get => y;
			set => y = value;
		}
		public Vector2u() {
			x = 0;
			y = 0;
		}

		public Vector2u(in uint f) { x = y = f; }
		public Vector2u(in uint x, in uint y) { this.x = x; this.y = y; }
		public Vector2u(in uint[] v2) { x = v2[0]; y = v2[1]; }
		[Exposed]
		static public readonly Vector2u Zero = new(0, 0);
		[Exposed]
		static public readonly Vector2u One = new(1, 1);
		[Exposed]
		static public readonly Vector2u AxisX = new(1, 0);
		[Exposed]
		static public readonly Vector2u AxisY = new(0, 1);
		
		public uint this[in uint key]
		{
			get => (key == 0) ? x : y;
			set {
				if (key == 0) { x = value; }
				else {
					y = value;
				}
			}
		}
		
		public uint[] Array => new uint[] { x, y };

		public void Add(in uint s) { x += s; y += s; }

		
		public uint LengthSquared => (x * x) + (y * y);


		public static Vector2u operator -(in Vector2u v) => new((uint)-(int)v.x, (uint)-(int)v.y);

		public static Vector2u operator *(in uint f, in Vector2u v) => new(f * v.x, f * v.y);
		public static Vector2u operator *(in Vector2u v, in uint f) => new(f * v.x, f * v.y);
		public static Vector2u operator /(in Vector2u v, in uint f) => new(v.x / f, v.y / f);
		public static Vector2u operator /(in uint f, in Vector2u v) => new(f / v.x, f / v.y);

		public static Vector2u operator *(in Vector2u a, in Vector2u b) => new(a.x * b.x, a.y * b.y);
		public static Vector2u operator /(in Vector2u a, in Vector2u b) => new(a.x / b.x, a.y / b.y);


		public static Vector2u operator +(in Vector2u v0, in Vector2u v1) => new(v0.x + v1.x, v0.y + v1.y);
		public static Vector2u operator +(in Vector2u v0, in uint f) => new(v0.x + f, v0.y + f);

		public static Vector2u operator -(in Vector2u v0, in Vector2u v1) => new(v0.x - v1.x, v0.y - v1.y);
		public static Vector2u operator -(in Vector2u v0, in uint f) => new(v0.x - f, v0.y - f);



		public static bool operator ==(in Vector2u a, in Vector2u b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(in Vector2u a, in Vector2u b) => a.x != b.x || a.y != b.y;
		public override bool Equals(object obj) {
			return this == (Vector2u)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y);
		}

		public int CompareTo(Vector2u other) {
			if (x != other.x) {
				return x < other.x ? -1 : 1;
			}
			else if (y != other.y) {
				return y < other.y ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Vector2u other) {
			return x == other.x && y == other.y;
		}



		public override string ToString() {
			return string.Format("{0} {1}", x, y);
		}
	}
}
