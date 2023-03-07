using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace RNumerics
{
	public struct Triangle3d : ISerlize<Triangle3d>
	{
		public Vector3d v0;
		public Vector3d v1;
		public Vector3d v2;


		public void Serlize(BinaryWriter binaryWriter) {
			v0.Serlize(binaryWriter);
			v1.Serlize(binaryWriter);
			v2.Serlize(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			v0.DeSerlize(binaryReader);
			v1.DeSerlize(binaryReader);
			v2.DeSerlize(binaryReader);
		}

		[Exposed]
		public Vector3d V0
		{
			get => v0;
			set => v0 = value;
		}
		[Exposed]
		public Vector3d V1
		{
			get => v1;
			set => v1 = value;
		}
		[Exposed]
		public Vector3d V2
		{
			get => v2;
			set => v2 = value;
		}

		public Triangle3d() {
			v0 = new Vector3d(0, 0, 0);
			v1 = new Vector3d(0, 0, 0);
			v2 = new Vector3d(0, 0, 0);
		}

		public Triangle3d(in Vector3d v0, in Vector3d v1, in Vector3d v2) {
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
		}

		public Vector3d this[in int key]
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
		
		public Vector3d Normal => MathUtil.Normal(v0, v1, v2);
		
		public double Area => MathUtil.Area(v0, v1, v2);
		
		public double AspectRatio => MathUtil.AspectRatio( v0,  v1,  v2);

		public Vector3d PointAt(in double bary0, in double bary1, in double bary2) {
			return (bary0 * v0) + (bary1 * v1) + (bary2 * v2);
		}
		public Vector3d PointAt(in Vector3d bary) {
			return (bary.x * v0) + (bary.y * v1) + (bary.z * v2);
		}

		public Vector3d BarycentricCoords(in Vector3d point) {
			return MathUtil.BarycentricCoords(point, v0, v1, v2);
		}

		// conversion operators
		public static implicit operator Triangle3d(in Triangle3f v) => new(v.V0, v.V1, v.V2);
		public static explicit operator Triangle3f(in Triangle3d v) => new((Vector3f)v.v0, (Vector3f)v.v1, (Vector3f)v.v2);
	}


	public struct Triangle3f : ISerlize<Triangle3f>
	{
		public Vector3f V0;
		public Vector3f V1;
		public Vector3f V2;

		public void Serlize(BinaryWriter binaryWriter) {
			V0.Serlize(binaryWriter);
			V1.Serlize(binaryWriter);
			V2.Serlize(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			V0.DeSerlize(binaryReader);
			V1.DeSerlize(binaryReader);
			V2.DeSerlize(binaryReader);
		}


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
