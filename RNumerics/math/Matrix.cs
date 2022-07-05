using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace RNumerics
{
	/// <summary>A Matrix in StereoKit is a 4x4 grid of numbers that is used 
	/// to represent a transformation for any sort of position or vector! 
	/// This is an oversimplification of what a matrix actually is, but it's
	/// accurate in this case.
	/// 
	/// Matrices are really useful for transforms because you can chain 
	/// together all sorts of transforms into a single Matrix! A Matrix
	/// transform really shines when applied to many positions, as the more
	/// expensive operations get cached within the matrix values.
	/// 
	/// Matrices are prominently used within shaders for mesh transforms!
	/// </summary>
	public struct Matrix {
		/// <summary>The internal, wrapped System.Numerics type. This can be
		/// nice to have around so you can pass its fields as 'ref', which you
		/// can't do with properties. You won't often need this, as implicit
		/// conversions to System.Numerics types are also provided.</summary>
		public Matrix4x4 m;

		public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44) {
			m = new Matrix4x4(
						   m11, m12, m13, m14,
						   m21, m22, m23, m24,
						   m31, m32, m33, m34,
						   m41, m42, m43, m44);
		}

		public static Matrix CreateFromAssimp(Assimp.Matrix4x4 a) {
			return new Matrix(a.A1, a.A2, a.A3, a.A4,
							  a.B1, a.B2, a.B3, a.B4,
							  a.C1, a.C2, a.C3, a.C4,
							  a.D1, a.D2, a.D3, a.D4);
		}

		public Matrix(Matrix4x4 matrix) {
			m = matrix;
		}

		public static implicit operator Matrix(Matrix4x4 m) => new(m);
		public static implicit operator Matrix4x4(Matrix m) => m.m;

		public static Matrix operator *(Matrix a, Matrix b) => a.m * b.m;
		public static Vector3f operator *(Matrix a, Vector3f b) => Vector3.Transform(b, a.m);
		public static bool operator ==(Matrix a, Matrix b) => a.m == b.m;
		public static bool operator !=(Matrix a, Matrix b) => a.m != b.m;

		/// <summary>An identity Matrix is the matrix equivalent of '1'! 
		/// Transforming anything by this will leave it at the exact same
		/// place.</summary>
		public static Matrix Identity => Matrix4x4.Identity;

		/// <summary>A fast Property that will return or set the translation
		/// component embedded in this transform matrix.</summary>
		public Vector3f Translation { get => m.Translation; set => m.Translation = value; }
		/// <summary>Returns the scale embedded in this transform matrix. Not
		/// exactly cheap, requires 3 sqrt calls, but is cheaper than calling
		/// Decompose.</summary>
		public Vector3f Scale {
			get {
				Matrix4x4.Decompose(m, out var scale,out _,out _);
				return scale;
			}
			}
		/// <summary>A slow function that returns the rotation quaternion 
		/// embedded in this transform matrix. This is backed by Decompose,
		/// so if you need any additional info, it's better to just call
		/// Decompose instead.</summary>
		public Quaternionf Rotation
		{
			get {
				Matrix4x4.Decompose(m, out _, out var rot, out _);
				return (Quaternionf)rot;
			}
		}


		/// <summary>Creates an inverse matrix! If the matrix takes a point 
		/// from a -> b, then its inverse takes the point from b -> a.
		/// </summary>
		/// <returns>An inverse matrix of the current one.</returns>
		public Matrix Inverse { get { Matrix4x4.Invert(m, out var result); return result; } }

		public Matrix InvScale
		{
			get {
				Decompose(out var pos,out var rot , out var scale);
				scale = 1 / scale;
				return Matrix.TRS(pos,rot,scale);
			}
		}

		/// <summary>Inverts this Matrix! If the matrix takes a point from a
		/// -> b, then its inverse takes the point from b -> a.</summary>
		public void Invert() {
			Matrix4x4.Invert(m, out m);
		}

		/// <summary>Transforms a point through the Matrix! This is basically 
		/// just multiplying a vector (x,y,z,1) with the Matrix.</summary>
		/// <param name="point">The point to transform.</param>
		/// <returns>The point transformed by the Matrix.</returns>
		public Vector3f Transform(Vector3f point) {
			return Vector3.Transform(point, m);
		}

		/// <summary> Transforms a point through the Matrix, but excluding 
		/// translation! This is great for transforming vectors that are 
		/// -directions- rather than points in space. Use this to transform 
		/// normals and directions. The same as multiplying (x,y,z,0) with 
		/// the Matrix.</summary>
		/// <param name="normal">The direction to transform.</param>
		/// <returns>The direction transformed by the Matrix.</returns>
		public Vector3f TransformNormal(Vector3f normal) {
			return Vector3.TransformNormal(normal, m);
		}


		/// <summary>Returns this transformation matrix to its original 
		/// translation, rotation and scale components. Not exactly a cheap
		/// function. If this is not a transform matrix, there's a chance
		/// this call will fail, and return false.</summary>
		/// <param name="translation">XYZ translation of the matrix.</param>
		/// <param name="rotation">The rotation quaternion, some lossiness
		/// may be encountered when composing/decomposing.</param>
		/// <param name="scale">XYZ scale components.</param>
		/// <returns>If this is not a transform matrix, there's a chance this
		/// call will fail, and return false.</returns>
		public bool Decompose(out Vector3f translation, out Quaternionf rotation, out Vector3f scale) {
			var ret = Matrix4x4.Decompose(m, out var escale, out var erotation, out var etranslation);
			translation = etranslation;
			rotation = (Quaternionf)erotation;
			scale = escale;
			return ret;
		}

		/// <summary>Translate. Creates a translation Matrix!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <returns>A Matrix containing a simple translation!</returns>
		public static Matrix T(Vector3f translation) {
			return Matrix4x4.CreateTranslation(translation.x, translation.y, translation.z);
		}

		/// <summary>Translate. Creates a translation Matrix!</summary>
		/// <param name="x">Move an object on the x axis by this amount.</param>
		/// <param name="y">Move an object on the y axis by this amount.</param>
		/// <param name="z">Move an object on the z axis by this amount.</param>
		/// <returns>A Matrix containing a simple translation!</returns>
		public static Matrix T(float x, float y, float z) {
			return Matrix4x4.CreateTranslation(x, y, z);
		}

		/// <summary>Create a rotation matrix from a Quaternion.</summary>
		/// <param name="rotation">A Quaternion describing the rotation for 
		/// this transform.</param>
		/// <returns>A Matrix that will rotate by the provided Quaternion 
		/// orientation.</returns>
		public static Matrix R(Quaternionf rotation) {
			return Matrix4x4.CreateFromQuaternion((Quaternion)rotation);
		}

		/// <summary>Create a rotation matrix from pitch, yaw, and roll 
		/// information. Units are in degrees.</summary>
		/// <param name="pitchXDeg">Pitch, or rotation around the X axis, in
		/// degrees.</param>
		/// <param name="yawYDeg">Yaw, or rotation around the Y axis, in 
		/// degrees.</param>
		/// <param name="rollZDeg">Roll, or rotation around the Z axis, in
		/// degrees.</param>
		/// <returns>A Matrix that will rotate by the provided pitch, yaw and 
		/// roll.</returns>
		public static Matrix R(float pitchXDeg, float yawYDeg, float rollZDeg) {
			return Matrix4x4.CreateFromYawPitchRoll((float)(yawYDeg * MathUtil.DEG_2_RAD), (float)(pitchXDeg * MathUtil.DEG_2_RAD), (float)(rollZDeg * MathUtil.DEG_2_RAD));
		}

		/// <summary>Create a rotation matrix from pitch, yaw, and roll 
		/// information. Units are in degrees.</summary>
		/// <param name="pitchYawRollDeg">Pitch (x-axis), yaw (y-axis), and 
		/// roll (z-axis) stored as x, y and z respectively in this Vec3.
		/// Units are in degrees.</param>
		/// <returns>A Matrix that will rotate by the provided pitch, yaw and 
		/// roll.</returns>
		public static Matrix R(Vector3f pitchYawRollDeg) {
			return R(pitchYawRollDeg.x, pitchYawRollDeg.y, pitchYawRollDeg.z);
		}

		/// <summary>Creates a scaling Matrix, where scale can be different
		/// on each axis (non-uniform).</summary>
		/// <param name="scale">How much larger or smaller this transform
		/// makes things. Vec3.One is a good default, as Vec3.Zero will
		/// shrink it to nothing!</param>
		/// <returns>A non-uniform scaling matrix.</returns>
		public static Matrix S(Vector3f scale) {
			return Matrix4x4.CreateScale(scale.x, scale.y, scale.z);
		}

		/// <summary>Creates a scaling Matrix, where the scale is the same on
		/// each axis (uniform).</summary>
		/// <param name="scale">How much larger or smaller this transform
		/// makes things. 1 is a good default, as 0 will shrink it to nothing!
		/// This will expand to a scale vector of (size, size, size)</param>
		/// <returns>A uniform scaling matrix.</returns>
		public static Matrix S(float scale) {
			return Matrix4x4.CreateScale(scale, scale, scale);
		}

		/// <summary>Translate, Scale. Creates a transform Matrix using both
		/// these components!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <param name="scale">How much larger or smaller this transform
		/// makes things. 1 is a good default, as 0 will shrink it to nothing!
		/// This will expand to a scale vector of (size, size, size)</param>
		/// <returns>A Matrix that combines translation and scale information
		/// into a single Matrix!</returns>
		public static Matrix TS(Vector3f translation, float scale) {
			return TRS(translation, Quaternionf.Identity, new Vector3f(scale, scale, scale));
		}

		/// <summary>Translate, Scale. Creates a transform Matrix using both
		/// these components!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <param name="scale">How much larger or smaller this transform 
		/// makes things. Vec3.One is a good default, as Vec3.Zero will 
		/// shrink it to nothing!</param>
		/// <returns>A Matrix that combines translation and scale information
		/// into a single Matrix!</returns>
		public static Matrix TS(Vector3f translation, Vector3f scale) {
			return TRS(translation, Quaternionf.Identity, scale);
		}

		/// <summary>Translate, Scale. Creates a transform Matrix using both
		/// these components!</summary>
		/// <param name="x">Move an object on the x axis by this amount.</param>
		/// <param name="y">Move an object on the y axis by this amount.</param>
		/// <param name="z">Move an object on the z axis by this amount.</param>
		/// <param name="scale">How much larger or smaller this transform 
		/// makes things. Vec3.One is a good default, as Vec3.Zero will 
		/// shrink it to nothing!</param>
		/// <returns>A Matrix that combines translation and scale information
		/// into a single Matrix!</returns>
		public static Matrix TS(float x, float y, float z, float scale) {
			return TRS(new Vector3f(x, y, z), Quaternionf.Identity, new Vector3f(scale, scale, scale));
		}

		/// <summary>Translate, Scale. Creates a transform Matrix using both
		/// these components!</summary>
		/// <param name="x">Move an object on the x axis by this amount.</param>
		/// <param name="y">Move an object on the y axis by this amount.</param>
		/// <param name="z">Move an object on the z axis by this amount.</param>
		/// <param name="scale">How much larger or smaller this transform 
		/// makes things. Vec3.One is a good default, as Vec3.Zero will 
		/// shrink it to nothing!</param>
		/// <returns>A Matrix that combines translation and scale information
		/// into a single Matrix!</returns>
		public static Matrix TS(float x, float y, float z, Vector3f scale) {
			return TRS(new Vector3f(x, y, z), Quaternionf.Identity, scale);
		}


		/// <summary>Translate, Rotate. Creates a transform Matrix using 
		/// these components!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <param name="rotation">A Quaternion describing the rotation for 
		/// this transform.</param>
		/// <returns>A Matrix that combines translation and rotation
		/// information into a single Matrix!</returns>
		public static Matrix TR(Vector3f translation, Quaternionf rotation) {
			return TRS(translation, rotation, Vector3f.One);
		}

		/// <summary>Translate, Rotate. Creates a transform Matrix using 
		/// these components!</summary>
		/// <param name="x">Move an object on the x axis by this amount.</param>
		/// <param name="y">Move an object on the y axis by this amount.</param>
		/// <param name="z">Move an object on the z axis by this amount.</param>
		/// <param name="rotation">A Quaternion describing the rotation for 
		/// this transform.</param>
		/// <returns>A Matrix that combines translation and rotation
		/// information into a single Matrix!</returns>
		public static Matrix TR(float x, float y, float z, Quaternionf rotation) {
			return TRS(new Vector3f(x, y, z), rotation, Vector3f.One);
		}

		/// <summary>Translate, Rotate. Creates a transform Matrix using 
		/// these components!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <param name="pitchYawRollDeg">Pitch (x-axis), yaw (y-axis), and 
		/// roll (z-axis) stored as x, y and z respectively in this Vec3.
		/// Units are in degrees.</param>
		/// <returns>A Matrix that combines translation and rotation
		/// information into a single Matrix!</returns>
		public static Matrix TR(Vector3f translation, Vector3f pitchYawRollDeg) {
			return TRS(translation, Quaternionf.CreateFromEuler(pitchYawRollDeg), Vector3f.One);
		}

		/// <summary>Translate, Rotate. Creates a transform Matrix using 
		/// these components!</summary>
		/// <param name="x">Move an object on the x axis by this amount.</param>
		/// <param name="y">Move an object on the y axis by this amount.</param>
		/// <param name="z">Move an object on the z axis by this amount.</param>
		/// <param name="pitchYawRollDeg">Pitch (x-axis), yaw (y-axis), and 
		/// roll (z-axis) stored as x, y and z respectively in this Vec3.
		/// Units are in degrees.</param>
		/// <returns>A Matrix that combines translation and rotation
		/// information into a single Matrix!</returns>
		public static Matrix TR(float x, float y, float z, Vector3f pitchYawRollDeg) {
			return TRS(new Vector3f(x, y, z), Quaternionf.CreateFromEuler(pitchYawRollDeg), Vector3f.One);
		}

		/// <summary>Translate, Rotate, Scale. Creates a transform Matrix 
		/// using all these components!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <param name="rotation">A Quaternion describing the rotation for 
		/// this transform.</param>
		/// <param name="scale">How much larger or smaller this transform 
		/// makes things. 1 is a good default, as 0 will shrink it to nothing! 
		/// This will expand to a scale vector of (size, size, size)</param>
		/// <returns>A Matrix that combines translation, rotation, and scale
		/// information into a single Matrix!</returns>
		public static Matrix TRS(Vector3f translation, Quaternionf rotation, float scale) {
			return TRS(translation, rotation, new Vector3f(scale, scale, scale));
		}

		/// <summary>Translate, Rotate, Scale. Creates a transform Matrix 
		/// using all these components!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <param name="rotation">A Quaternion describing the rotation for 
		/// this transform.</param>
		/// <param name="scale">How much larger or smaller this transform 
		/// makes things. Vec3.One is a good default, as Vec3.Zero will 
		/// shrink it to nothing!</param>
		/// <returns>A Matrix that combines translation, rotation, and scale
		/// information into a single Matrix!</returns>
		public static Matrix TRS(Vector3f translation, Quaternionf rotation, Vector3f scale) {
			return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion((Quaternion)rotation) * Matrix4x4.CreateTranslation(translation);
		}
		/// <summary>Translate, Rotate, Scale. Creates a transform Matrix 
		/// using all these components!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <param name="pitchYawRollDeg">Pitch (x-axis), yaw (y-axis), and 
		/// roll (z-axis) stored as x, y and z respectively in this Vec3.
		/// Units are in degrees.</param>
		/// <param name="scale">How much larger or smaller this transform 
		/// makes things. Vec3.One is a good default, as Vec3.Zero will 
		/// shrink it to nothing!</param>
		/// <returns>A Matrix that combines translation, rotation, and scale
		/// information into a single Matrix!</returns>
		public static Matrix TRS(Vector3f translation, Vector3f pitchYawRollDeg, float scale) {
			return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(pitchYawRollDeg.x, pitchYawRollDeg.y, pitchYawRollDeg.z)) * Matrix4x4.CreateTranslation(translation);
		}

		/// <summary>Translate, Rotate, Scale. Creates a transform Matrix 
		/// using all these components!</summary>
		/// <param name="translation">Move an object by this amount.</param>
		/// <param name="pitchYawRollDeg">Pitch (x-axis), yaw (y-axis), and 
		/// roll (z-axis) stored as x, y and z respectively in this Vec3.
		/// Units are in degrees.</param>
		/// <param name="scale">How much larger or smaller this transform 
		/// makes things. Vec3.One is a good default, as Vec3.Zero will 
		/// shrink it to nothing!</param>
		/// <returns>A Matrix that combines translation, rotation, and scale
		/// information into a single Matrix!</returns>
		public static Matrix TRS(Vector3f translation, Vector3f pitchYawRollDeg, Vector3f scale) {
			return TRS(translation, Quaternionf.CreateFromEuler(pitchYawRollDeg), scale);
		}

		/// <summary>This creates a matrix used for projecting 3D geometry
		/// onto a 2D surface for rasterization. Perspective projection 
		/// matrices will cause parallel lines to converge at the horizon. 
		/// This is great for normal looking content.</summary>
		/// <param name="fovDegrees">This is the vertical field of view of
		/// the perspective matrix, units are in degrees.</param>
		/// <param name="aspectRatio">The projection surface's width/height.
		/// </param>
		/// <param name="nearClip">Anything closer than this distance (in
		/// meters) will be discarded. Must not be zero, and if you make this
		/// too small, you may experience glitching in your depth buffer.</param>
		/// <param name="farClip">Anything further than this distance (in
		/// meters) will be discarded. For low resolution depth buffers, this
		/// should not be too far away, or you'll see bad z-fighting 
		/// artifacts.</param>
		/// <returns>The final perspective matrix.</returns>
		public static Matrix Perspective(float fovDegrees, float aspectRatio, float nearClip, float farClip) {
			return Matrix4x4.CreatePerspectiveFieldOfView((float)(fovDegrees * MathUtil.RAD_2_DEG), aspectRatio, nearClip, farClip);
		}

		/// <summary>This creates a matrix used for projecting 3D geometry
		/// onto a 2D surface for rasterization. Orthographic projection 
		/// matrices will preserve parallel lines. This is great for 2D 
		/// scenes or content.</summary>
		/// <param name="width">The width, in meters, of the area that will 
		/// be projected.</param>
		/// <param name="height">The height, in meters, of the area that will
		/// be projected.</param>
		/// <param name="nearClip">Anything closer than this distance (in
		/// meters) will be discarded. Must not be zero, and if you make this
		/// too small, you may experience glitching in your depth buffer.</param>
		/// <param name="farClip">Anything further than this distance (in
		/// meters) will be discarded. For low resolution depth buffers, this
		/// should not be too far away, or you'll see bad z-fighting 
		/// artifacts.</param>
		/// <returns>The final orhtographic matrix.</returns>
		public static Matrix Orthographic(float width, float height, float nearClip, float farClip) {
			return Matrix4x4.CreateOrthographic(width, height, nearClip, farClip);
		}

		public override bool Equals(object obj) {
			return obj is Matrix matrix && matrix == this;
		}

		public override int GetHashCode() {
			return m.GetHashCode();
		}
	}
}
