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
	public sealed class Gizmo3D : Component
	{
		public const float ALPHA = 0.8f;
		[Default(GizmoMode.All)]
		public readonly Sync<GizmoMode> Mode;
		public readonly Sync<bool> UseLocalRot;

		public readonly Sync<float> PosStep;
		public readonly Sync<float> ScaleStep;
		public readonly Sync<float> AngleStep;


		public readonly Linker<Vector3f> ScaleLink;
		public readonly Linker<Vector3f> PosLink;
		public readonly Linker<Quaternionf> RotLink;

		public readonly SyncRef<GizmoAixe> X;
		public readonly SyncRef<GizmoAixe> Y;
		public readonly SyncRef<GizmoAixe> Z;
		public readonly SyncRef<GizmoPlane> XPlane;
		public readonly SyncRef<GizmoPlane> YPlane;
		public readonly SyncRef<GizmoPlane> ZPlane;

		public readonly SyncRef<Entity> TransformSpace;
		public readonly SyncRef<Entity> ParentEntity;

		public readonly SyncRef<IValueSource<Vector3f>> Pos;
		public readonly SyncRef<IValueSource<Vector3f>> Scale;
		public readonly SyncRef<IValueSource<Quaternionf>> Rot;

		public Matrix GlobalPos => ParentEntity.Target?.LocalToGlobal(Matrix.TRS(Pos.Target?.Value ?? Vector3f.Zero, Rot.Target?.Value ?? Quaternionf.Identity, Scale.Target?.Value ?? Vector3f.One)) ?? Matrix.Identity;
		public Matrix LocalPos => TransformSpace.Target?.GlobalToLocal(GlobalPos) ?? Matrix.Identity;

		[Default(0.15f)]
		public readonly Sync<float> TargetSize;

		public bool GetIfOtherIsActive(Component component) {
			var isActive = false;
			if (X.Target != component) {
				isActive |= X.Target?.IsActive ?? false;
			}
			if (Y.Target != component) {
				isActive |= Y.Target?.IsActive ?? false;
			}
			if (Z.Target != component) {
				isActive |= Z.Target?.IsActive ?? false;
			}
			if (XPlane.Target != component) {
				isActive |= XPlane.Target?.isInPos ?? false;
			}
			if (YPlane.Target != component) {
				isActive |= YPlane.Target?.isInPos ?? false;
			}
			if (ZPlane.Target != component) {
				isActive |= ZPlane.Target?.isInPos ?? false;
			}
			return isActive;
		}

		protected override void Step() {
			base.Step();
			var space = TransformSpace.Target ?? World.RootEntity;
			var parrent = (ParentEntity.Target ?? World.RootEntity).GlobalTrans;
			var globalTrans = Matrix.TRS(Pos.Target?.Value ?? Vector3f.Zero, Rot.Target?.Value ?? Quaternionf.Identity, Scale.Target?.Value ?? Vector3f.One) * parrent;
			if (RotLink.Linked) {
				RotLink.LinkedValue = UseLocalRot ? globalTrans.Rotation : space.GlobalTrans.Rotation;
			}
			if (PosLink.Linked) {
				PosLink.LinkedValue = globalTrans.Translation;
			}
			if (ScaleLink.Linked) {
				var userRoot = LocalUser?.userRoot?.Target?.head.Target;
				if (userRoot is null) {
					return;
				}
				var distance = userRoot.GlobalTrans.Translation.Distance(Entity.GlobalTrans.Translation);
				ScaleLink.LinkedValue = new Vector3f(distance * TargetSize);
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			Pos.AllowCrossWorld();
			Scale.AllowCrossWorld();
			Rot.AllowCrossWorld();
			TransformSpace.AllowCrossWorld();
			ParentEntity.AllowCrossWorld();

			ScaleLink.Target = Entity.scale;
			RotLink.Target = Entity.rotation;
			PosLink.Target = Entity.position;

			var driver = Entity.AttachComponent<ValueMultiDriver<GizmoMode>>();
			driver.source.Target = Mode;
			var x = Entity.AddChild("X");
			x.rotation.Value = Quaternionf.Rolled.Inverse * Quaternionf.Yawed180;
			X.Target = x.AttachComponent<GizmoAixe>();
			X.Target.Gizmo3DTarget.Target = this;
			X.Target.Direction.Value = GizmoDir.X;
			XPlane.Target = x.AttachComponent<GizmoPlane>();
			XPlane.Target.Direction.Value = GizmoDir.X;
			XPlane.Target.Gizmo3DTarget.Target = this;
			driver.drivers.Add().Target = XPlane.Target.Mode;
			driver.drivers.Add().Target = X.Target.Mode;

			var y = Entity.AddChild("Y");
			y.rotation.Value = Quaternionf.Yawed;
			Y.Target = y.AttachComponent<GizmoAixe>();
			Y.Target.Direction.Value = GizmoDir.Y;
			Y.Target.Gizmo3DTarget.Target = this;
			YPlane.Target = y.AttachComponent<GizmoPlane>();
			YPlane.Target.Gizmo3DTarget.Target = this;
			YPlane.Target.Direction.Value = GizmoDir.Y;
			driver.drivers.Add().Target = YPlane.Target.Mode;
			driver.drivers.Add().Target = Y.Target.Mode;

			var z = Entity.AddChild("Z");
			z.rotation.Value = Quaternionf.Pitched.Inverse;
			Z.Target = z.AttachComponent<GizmoAixe>();
			Z.Target.Gizmo3DTarget.Target = this;
			Z.Target.Direction.Value = GizmoDir.Z;
			ZPlane.Target = z.AttachComponent<GizmoPlane>();
			ZPlane.Target.Gizmo3DTarget.Target = this;
			ZPlane.Target.Direction.Value = GizmoDir.Z;
			driver.drivers.Add().Target = ZPlane.Target.Mode;
			driver.drivers.Add().Target = Z.Target.Mode;
		}

		public void SetMatrix(Matrix newMatrix) {
			newMatrix = TransformSpace.Target?.LocalToGlobal(newMatrix) ?? Matrix.Identity;
			newMatrix = ParentEntity.Target?.GlobalToLocal(newMatrix) ?? Matrix.Identity;
			newMatrix.Decompose(out var trans, out var quaternionf, out var scale);
			if (Pos.Target is not null) {
				Pos.Target.Value = trans;
			}
			if (Scale.Target is not null) {
				Scale.Target.Value = scale;
			}
			if (Rot.Target is not null) {
				Rot.Target.Value = quaternionf;
			}
		}
	}
}