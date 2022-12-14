using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.Normal)]
	[OverlayOnly]
	public sealed class GizmoPlane : Component
	{
		public readonly SyncRef<Gizmo3D> Gizmo3DTarget;

		public readonly Sync<GizmoDir> Direction;
		[OnChanged(nameof(UpdateMeshes))]
		[Default(GizmoMode.All)]
		public readonly Sync<GizmoMode> Mode;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Position;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> PositionCollider;

		public readonly Linker<Colorf> ColorOfPositionGizmo;

		public readonly SyncRef<PhysicsObject> PositionColliderTarget;

		public bool isInPos;
		private Handed _handed;

		protected override void Step() {
			base.Step();
			if (Gizmo3DTarget.Target?.GetIfOtherIsActive(this) ?? false) {
				return;
			}
			if (PositionColliderTarget.Target is not null & ColorOfPositionGizmo.Linked) {
				ColorOfPositionGizmo.LinkedValue = PositionColliderTarget.Target.LazeredThisFrame | isInPos ? GetColor(0.85f) : GetColor();
				if (!isInPos) {
					_handed = PositionColliderTarget.Target.LazerHand;
					if (InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed) && PositionColliderTarget.Target.LazeredThisFrame) {
						isInPos = true;
						PrivateSpaceManager.GetLazer(_handed).Locked.Value = true;
					}
				}
				else {
					if (!InputManager.GetInputAction(InputTypes.Primary).HandedActivated(_handed)) {
						isInPos = false;
						PrivateSpaceManager.GetLazer(_handed).Locked.Value = false;
					}
				}
			}
		}

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
			plane.position.Value = new Vector3f(0.6f, 0, 0.6f);
			var positionMeshRender = plane.AttachComponent<MeshRender>();
			Position.Target = positionMeshRender.Enabled;
			var planeMesh = plane.AttachComponent<RectangleMesh>();
			planeMesh.Dimensions.Value = new Vector2f(0.5f);

			var posColider = plane.AttachComponent<BoxShape>();
			posColider.Size.Value = new Vector3f(0.5f, 0.02f, 0.5f);
			PositionCollider.Target = posColider.Enabled;
			positionMeshRender.mesh.Target = planeMesh;
			var posmit = Entity.AttachComponent<UnlitMaterial>();
			positionMeshRender.materials.Add().Target = posmit;
			posmit.Transparency.Value = Transparency.Blend;
			RenderThread.ExecuteOnEndOfFrame(() => posmit._material.NoDepthTest = true);
			ColorOfPositionGizmo.Target = posmit.Tint;
			PositionColliderTarget.Target = posColider;
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			UpdateMeshes();
		}

	}
}