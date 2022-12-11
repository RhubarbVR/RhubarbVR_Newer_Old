using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer" })]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class Gizmo3D : Component
	{
		public const float ALPHA = 0.8f;
		[Default(GizmoMode.All)]
		public readonly Sync<GizmoMode> Mode;

		public readonly Linker<Vector3f> ScaleLink;
		public readonly SyncRef<GizmoAixe> X;
		public readonly SyncRef<GizmoAixe> Y;
		public readonly SyncRef<GizmoAixe> Z;
		public readonly SyncRef<GizmoPlane> XPlane;
		public readonly SyncRef<GizmoPlane> YPlane;
		public readonly SyncRef<GizmoPlane> ZPlane;

		public readonly SyncRef<Entity> TransformSpace;

		public readonly SyncRef<IValueSource<Vector3f>> Pos;
		public readonly SyncRef<IValueSource<Vector3f>> Scale;
		public readonly SyncRef<IValueSource<Quaternionf>> Rot;

		[Default(1f)]
		public readonly Sync<float> TargetSize;
		protected override void Step() {
			base.Step();
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
			ScaleLink.Target = Entity.scale;
			var driver = Entity.AttachComponent<ValueMultiDriver<GizmoMode>>();
			driver.source.Target = Mode;
			var x = Entity.AddChild("X");
			x.rotation.Value = Quaternionf.Rolled.Inverse * Quaternionf.Yawed180;
			X.Target = x.AttachComponent<GizmoAixe>();
			X.Target.Direction.Value = GizmoDir.X;
			XPlane.Target = x.AttachComponent<GizmoPlane>();
			XPlane.Target.Direction.Value = GizmoDir.X;
			driver.drivers.Add().Target = XPlane.Target.Mode;
			driver.drivers.Add().Target = X.Target.Mode;

			var y = Entity.AddChild("Y");
			y.rotation.Value = Quaternionf.Yawed;
			Y.Target = y.AttachComponent<GizmoAixe>();
			Y.Target.Direction.Value = GizmoDir.Y;
			YPlane.Target = y.AttachComponent<GizmoPlane>();
			YPlane.Target.Direction.Value = GizmoDir.Y;
			driver.drivers.Add().Target = YPlane.Target.Mode;
			driver.drivers.Add().Target = Y.Target.Mode;

			var z = Entity.AddChild("Z");
			z.rotation.Value = Quaternionf.Pitched.Inverse;
			Z.Target = z.AttachComponent<GizmoAixe>();
			Z.Target.Direction.Value = GizmoDir.Z;
			ZPlane.Target = z.AttachComponent<GizmoPlane>();
			ZPlane.Target.Direction.Value = GizmoDir.Z;
			driver.drivers.Add().Target = ZPlane.Target.Mode;
			driver.drivers.Add().Target = Z.Target.Mode;
		}

	}
}