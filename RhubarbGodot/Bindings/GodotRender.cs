using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Linker;

using RNumerics;

namespace RhubarbVR.Bindings
{
	public static class GodotMatrixHelper
	{
		public static Matrix GetPos(this Node3D node3D) {
			var pos = node3D.Position;
			var scale = node3D.Scale;
			var rot = node3D.Transform.basis.GetRotationQuaternion();
			return Matrix.TRS(new Vector3f(pos.x, pos.y, pos.z), new Quaternionf(rot.x, rot.y, rot.z, rot.w), new Vector3f(scale.x, scale.y, scale.z));
		}

		public static Transform3D CastMatrix(this Matrix matrix) {
			return new Transform3D(
				new Vector3(matrix.M.M11, matrix.M.M21, matrix.M.M31),
				new Vector3(matrix.M.M12, matrix.M.M22, matrix.M.M32),
				new Vector3(matrix.M.M13, matrix.M.M23, matrix.M.M33),
				new Vector3(matrix.M.M14, matrix.M.M24, matrix.M.M34)
				);
		}

		public static Transform3D CastPosMatrix(this Matrix matrix) {
			return new Transform3D(
				new Vector3(matrix.M.M11, matrix.M.M12, matrix.M.M13),
				new Vector3(matrix.M.M21, matrix.M.M22, matrix.M.M23),
				new Vector3(matrix.M.M31, matrix.M.M32, matrix.M.M33),
				new Vector3(matrix.M.M41, matrix.M.M42, matrix.M.M43)
				);
		}

		public static Transform3D GetTrans(this Matrix matrix) {
			matrix.Decompose(out var pos, out var rot, out var scale);
			return pos.IsAnyNan || rot.IsAnyNan || scale.IsAnyNan ? new Transform3D() : CastPosMatrix(matrix);
		}

		public static void SetPos(this Node3D node3D, Matrix matrix) {
			node3D.Transform = GetTrans(matrix);
		}
	}
	public sealed class GodotRender : IRRenderer
	{

		public GodotRender(EngineRunner engineRunner) {
			EngineRunner = engineRunner;
		}

		public float MinClip { get => EngineRunner.Camera.Near; set => EngineRunner.Camera.Near = value; }
		public float FarClip { get => EngineRunner.Camera.Far; set => EngineRunner.Camera.Far = value; }
		public EngineRunner EngineRunner { get; }
		public float Fov { get => EngineRunner.Camera.Fov; set => EngineRunner.Camera.Fov = value; }

		public bool PassthroughSupport => XRServer.PrimaryInterface?.IsPassthroughSupported() ?? false;

		public bool PassthroughMode
		{
			get => XRServer.PrimaryInterface?.IsPassthroughEnabled() ?? false; set {
				if (value) {
					XRServer.PrimaryInterface?.StartPassthrough();
				}
				else {
					XRServer.PrimaryInterface?.StopPassthrough();
				}
			}
		}

		public Matrix GetCameraRootMatrix() {
			return EngineRunner.Rigin.GetPos();
		}
		public void SetCameraRootMatrix(Matrix m) {
			EngineRunner.Rigin.SetPos(m);
		}

		public bool GetEnableSky() {
			return false;
		}

		public void SetEnableSky(bool e) {

		}

		public Matrix GetCameraLocalMatrix() {
			return EngineRunner.Camera.GetPos();
		}

		public void SetCameraLocalMatrix(Matrix m) {
			EngineRunner.Camera.SetPos(m);
		}
	}
}
