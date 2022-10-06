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
		public static void SetPos(this Node3D node3D,Matrix matrix) {
			matrix.Decompose(out var pos, out var rot, out var scale);
			if (pos.IsAnyNan || rot.IsAnyNan || scale.IsAnyNan){
				node3D.Transform = new Transform3D();
				return;
			}
			var trans = node3D.Transform;
			trans.basis = new Basis(new Quaternion(rot.x, rot.y, rot.z, rot.w)) {
				Scale = new Vector3(scale.x, scale.y, scale.z)
			};
			trans.origin = new Vector3(pos.x,pos.y,pos.z);
			node3D.Transform = trans;
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
	}
}
