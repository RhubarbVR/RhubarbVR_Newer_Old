using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{

	[Category(new string[] { "Developer" })]
	public sealed class GizmoPlane : Component
	{

		public readonly Sync<GizmoDir> Direction;
		[OnChanged(nameof(UpdateMeshes))]
		[Default(GizmoMode.All)]
		public readonly Sync<GizmoMode> Mode;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Position;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> PositionCollider;

		public readonly Linker<Colorf> ColorOfPositionGizmo;

		private Colorf GetColor(float addedValue = 0) {
			return Direction.Value switch {
				GizmoDir.Y => new Colorf(addedValue, 1, addedValue, Gizmo3D.ALPHA),
				GizmoDir.X => new Colorf(1, addedValue, addedValue, Gizmo3D.ALPHA),
				_ => new Colorf(addedValue, addedValue, 1, Gizmo3D.ALPHA),
			};
		}

		private void UpdateMeshes() {
			if (PositionCollider.Linked) {
				PositionCollider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Position);
			}
			if (Position.Linked) {
				Position.LinkedValue = Mode.Value.HasFlag(GizmoMode.Position);
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			var plane = Entity.AddChild("Plane");
			plane.position.Value = new Vector3f(0.6f / 5, 0, 0.6f / 5);
			var positionMeshRender = plane.AttachComponent<MeshRender>();
			Position.Target = positionMeshRender.Enabled;
			var planeMesh = plane.AttachComponent<RectangleMesh>();
			planeMesh.Dimensions.Value = new Vector2f(0.5f / 5);

			var posColider = plane.AttachComponent<BoxShape>();
			posColider.Size.Value = new Vector3f(0.5f, 0.02f, 0.5f) / 5;
			PositionCollider.Target = posColider.Enabled;
			positionMeshRender.mesh.Target = planeMesh;
			var posmit = Entity.AttachComponent<UnlitMaterial>();
			positionMeshRender.materials.Add().Target = posmit;
			posmit.Transparency.Value = Transparency.Blend;

			ColorOfPositionGizmo.Target = posmit.Tint;
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			UpdateMeshes();
		}

	}
}