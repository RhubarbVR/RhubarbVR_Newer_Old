using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{
	[Flags]
	public enum GizmoMode : byte
	{
		None = 0,
		Rotation = 1,
		Scale = 2,
		Position = 4,
		All = Rotation | Scale | Position
	}
	[Flags]
	public enum GizmoDir : byte
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4
	}

	[Category(new string[] { "Developer" })]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class GizmoAixe : Component
	{
		public readonly Sync<GizmoDir> Direction;
		[OnChanged(nameof(UpdateMeshes))]
		[Default(GizmoMode.All)]
		public readonly Sync<GizmoMode> Mode;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Rotation;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> RotationCollider;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Scale;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> ScaleCollider;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Scale2;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Scale2Collider;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> Position;
		[OnChanged(nameof(UpdateMeshes))]
		public readonly Linker<bool> PositionCollider;
		public readonly Linker<Colorf> ColorOfRotationGizmo;
		public readonly Linker<Colorf> ColorOfScaleGizmo;
		public readonly Linker<Colorf> ColorOfPositionGizmo;

		public readonly SyncRef<PhysicsObject> RotationColliderTarget;
		public readonly SyncRef<PhysicsObject> ScaleColliderTarget;
		public readonly SyncRef<PhysicsObject> Scale2ColliderTarget;
		public readonly SyncRef<PhysicsObject> PositionColliderTarget;

		protected override void Step() {
			base.Step();
			if (RotationColliderTarget.Target is not null & ColorOfRotationGizmo.Linked) {
				ColorOfRotationGizmo.LinkedValue = RotationColliderTarget.Target.LazeredThisFrame ? GetColor(0.85f) : GetColor();
			}
			if (ScaleColliderTarget.Target is not null & Scale2ColliderTarget.Target is not null & ColorOfScaleGizmo.Linked) {
				ColorOfScaleGizmo.LinkedValue = (Scale2ColliderTarget.Target.LazeredThisFrame | ScaleColliderTarget.Target.LazeredThisFrame) ? GetColor(0.85f) : GetColor();
			}
			if (PositionColliderTarget.Target is not null & ColorOfPositionGizmo.Linked) {
				ColorOfPositionGizmo.LinkedValue = PositionColliderTarget.Target.LazeredThisFrame ? GetColor(0.85f) : GetColor();
			}
		}


		private void UpdateMeshes() {
			if (RotationCollider.Linked) {
				RotationCollider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Rotation);
			}
			if (Rotation.Linked) {
				Rotation.LinkedValue = Mode.Value.HasFlag(GizmoMode.Rotation);
			}
			if (ScaleCollider.Linked) {
				ScaleCollider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Scale);
			}
			if (Scale.Linked) {
				Scale.LinkedValue = Mode.Value.HasFlag(GizmoMode.Scale);
			}
			if (Scale2Collider.Linked) {
				Scale2Collider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Scale) & !Mode.Value.HasFlag(GizmoMode.Position);
			}
			if (Scale2.Linked) {
				Scale2.LinkedValue = Mode.Value.HasFlag(GizmoMode.Scale);
			}
			if (PositionCollider.Linked) {
				PositionCollider.LinkedValue = Mode.Value.HasFlag(GizmoMode.Position);
			}
			if (Position.Linked) {
				Position.LinkedValue = Mode.Value.HasFlag(GizmoMode.Position);
			}
		}

		private Colorf GetColor(float addedValue = 0) {
			return Direction.Value switch {
				GizmoDir.Y => new Colorf(addedValue, 1, addedValue, Gizmo3D.ALPHA),
				GizmoDir.X => new Colorf(1, addedValue, addedValue, Gizmo3D.ALPHA),
				_ => new Colorf(addedValue, addedValue, 1, Gizmo3D.ALPHA),
			};
		}

		protected override void OnAttach() {
			base.OnAttach();
			var rotationMeshRender = Entity.AttachComponent<MeshRender>();
			Rotation.Target = rotationMeshRender.Enabled;
			var rotMesh = Entity.AttachComponent<TorusMesh>();
			rotMesh.MajorRadius.Value = 1.7f;
			rotMesh.MinorRadius.Value = 0.05f;
			rotationMeshRender.mesh.Target = rotMesh;
			//var rotColider = Entity.AttachComponent<MeshShape>();
			//rotColider.TargetMesh.Target = rotMesh;
			//RotationCollider.Target = rotColider.Enabled;
			//RotationColliderTarget.Target = rotColider;

			var mainBoxShapeColider = Entity.AttachComponent<BoxShape>();


			var scaleMeshRender = Entity.AttachComponent<MeshRender>();
			Scale.Target = scaleMeshRender.Enabled;
			var boxStickMesh = Entity.AttachComponent<TrivialBox3Mesh>();
			boxStickMesh.Extent.Value = new Vector3f(0.2f);
			boxStickMesh.Center.Value = new Vector3f(0, 2.6f, 0);
			mainBoxShapeColider.PosOffset.Value = new Vector3f(0, 2.6f, 0);
			mainBoxShapeColider.Size.Value = boxStickMesh.Extent.Value;
			ScaleCollider.Target = mainBoxShapeColider.Enabled;
			scaleMeshRender.mesh.Target = boxStickMesh;

			var scaletwo = Entity.AttachComponent<MeshRender>();
			Scale2.Target = scaletwo.Enabled;
			var scalecyl = Entity.AttachComponent<CylinderMesh>();
			scaletwo.mesh.Target = scalecyl;
			scalecyl.Height.Value = 2.6f;
			scalecyl.BaseRadius.Value = scalecyl.TopRadius.Value = 0.03f;

			var scaleColider = Entity.AttachComponent<CylinderShape>();
			scaleColider.PosOffset.Value = new Vector3f(0, 1.3f, 0);
			scaleColider.Height.Value = 2.6f;
			scaleColider.Radius.Value = 0.03f;
			Scale2Collider.Target = scaleColider.Enabled;

			var positionMeshRender = Entity.AttachComponent<MeshRender>();
			Position.Target = positionMeshRender.Enabled;
			var arrowMesh = Entity.AttachComponent<ArrowMesh>();
			arrowMesh.StickLength.Value = 1.98f;
			arrowMesh.StickRadius.Value = 0.04f;
			arrowMesh.HeadBaseRadius.Value = 0.1f;
			arrowMesh.HeadLength.Value = 0.4f;
			var posColider = Entity.AttachComponent<CylinderShape>();
			posColider.PosOffset.Value = new Vector3f(0, 2.3f / 2, 0);
			posColider.Height.Value = 2.3f;
			posColider.Radius.Value = 0.05f;
			PositionCollider.Target = posColider.Enabled;
			PositionColliderTarget.Target = posColider;

			positionMeshRender.mesh.Target = arrowMesh;

			var rotmit = Entity.AttachComponent<UnlitMaterial>();
			var scalemit = Entity.AttachComponent<UnlitMaterial>();
			var posmit = Entity.AttachComponent<UnlitMaterial>();

			rotationMeshRender.materials.Add().Target = rotmit;
			scaleMeshRender.materials.Add().Target = scalemit;
			scaletwo.materials.Add().Target = scalemit;
			positionMeshRender.materials.Add().Target = posmit;
			rotmit.Transparency.Value = Transparency.Blend;
			scalemit.Transparency.Value = Transparency.Blend;
			posmit.Transparency.Value = Transparency.Blend;

			ColorOfRotationGizmo.Target = rotmit.Tint;
			ColorOfPositionGizmo.Target = posmit.Tint;
			ColorOfScaleGizmo.Target = scalemit.Tint;
			ScaleColliderTarget.Target = mainBoxShapeColider;
			Scale2ColliderTarget.Target = scaleColider;

		}

		protected override void OnLoaded() {
			base.OnLoaded();
			UpdateMeshes();
		}

	}
}