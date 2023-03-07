using System;
using System.IO;

namespace RNumerics
{
	// These are convenience classes used in place of local stack arrays
	// (which C# does not support, but is common in C++ code)

	public struct Vector3dTuple2 : ISerlize<Vector3dTuple2>
	{
		public Vector3d v0;
		public Vector3d v1;

		public void Serlize(BinaryWriter binaryWriter) {
			v0.Serlize(binaryWriter);
			v1.Serlize(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			v0.DeSerlize(binaryReader);
			v1.DeSerlize(binaryReader);
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

		public Vector3dTuple2() {
			v0 = new Vector3d(0, 0, 0);
			v1 = new Vector3d(0, 0, 0);
		}

		public Vector3dTuple2(in Vector3d v0, in Vector3d v1) {
			this.v0 = v0;
			this.v1 = v1;
		}

		public Vector3d this[in int key]
		{
			get => (key == 0) ? v0 : v1;
			set {
				if (key == 0) { v0 = value; }
				else {
					v1 = value;
				}
			}
		}
	}

	public struct Vector3dTuple3 : ISerlize<Vector3dTuple3>
	{
		public Vector3d V0;
		public Vector3d V1;
		public Vector3d V2;


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

		public Vector3dTuple3() {
			V0 = new Vector3d(0, 0, 0);
			V1 = new Vector3d(0, 0, 0);
			V2 = new Vector3d(0, 0, 0);
		}

		public Vector3dTuple3(in Vector3d v0, in Vector3d v1, in Vector3d v2) {
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
	}


	public struct Vector3fTuple3 : ISerlize<Vector3fTuple3>
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


		public Vector3fTuple3() {
			V0 = new Vector3f(0, 0, 0);
			V1 = new Vector3f(0, 0, 0);
			V2 = new Vector3f(0, 0, 0);
		}
		public Vector3fTuple3(in Vector3f v0, in Vector3f v1, in Vector3f v2) {
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
	}



	public struct Vector2dTuple2 : ISerlize<Vector2dTuple2>
	{
		public Vector2d V0;
		public Vector2d V1;

		public void Serlize(BinaryWriter binaryWriter) {
			V0.Serlize(binaryWriter);
			V1.Serlize(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			V0.DeSerlize(binaryReader);
			V1.DeSerlize(binaryReader);
		}


		public Vector2dTuple2() {
			V0 = new Vector2d(0, 0);
			V1 = new Vector2d(0, 0);
		}
		public Vector2dTuple2(in Vector2d v0, in Vector2d v1) {
			V0 = v0;
			V1 = v1;
		}

		public Vector2d this[in int key]
		{
			get => (key == 0) ? V0 : V1;
			set {
				if (key == 0) { V0 = value; }
				else {
					V1 = value;
				}
			}
		}
	}

	public struct Vector2dTuple3 : ISerlize<Vector2dTuple3>
	{
		public Vector2d V0;
		public Vector2d V1;
		public Vector2d V2;

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


		public Vector2dTuple3() {
			V0 = new Vector2d(0, 0);
			V1 = new Vector2d(0, 0);
			V2 = new Vector2d(0, 0);
		}
		public Vector2dTuple3(in Vector2d v0, in Vector2d v1, in Vector2d v2) {
			V0 = v0;
			V1 = v1;
			V2 = v2;
		}

		public Vector2d this[in int key]
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
	}

	public struct Vector2dTuple4 : ISerlize<Vector2dTuple4>
	{
		public Vector2d V0;
		public Vector2d V1;
		public Vector2d V2;
		public Vector2d V3;

		public void Serlize(BinaryWriter binaryWriter) {
			V0.Serlize(binaryWriter);
			V1.Serlize(binaryWriter);
			V2.Serlize(binaryWriter);
			V3.Serlize(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			V0.DeSerlize(binaryReader);
			V1.DeSerlize(binaryReader);
			V2.DeSerlize(binaryReader);
			V3.DeSerlize(binaryReader);
		}

		public Vector2dTuple4() {
			V0 = new Vector2d(0, 0);
			V1 = new Vector2d(0, 0);
			V2 = new Vector2d(0, 0);
			V3 = new Vector2d(0, 0);
		}
		public Vector2dTuple4(in Vector2d v0, in Vector2d v1, in Vector2d v2, in Vector2d v3) {
			V0 = v0;
			V1 = v1;
			V2 = v2;
			V3 = v3;
		}

		public Vector2d this[in int key]
		{
			get {
				return (key > 1) ?
				  ((key == 2) ? V2 : V3) :
				  ((key == 1) ? V1 : V0);
			}
			set {
				if (key > 1) {
					if (key == 2) {
						V2 = value;
					}
					else {
						V3 = value;
					}
				}
				else {
					if (key == 1) {
						V0 = value;
					}
					else {
						V1 = value;
					}
				}
			}
		}
	}


}
