using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class UserInterfacePositioner : Component
	{
		public readonly SyncRef<User> targetUser;
		[Default(true)]
		public readonly Sync<bool> rotateVerticalOnly;
		[Default(3f)]
		public readonly Sync<float> positionSpeed;
		[Default(3f)]

		public readonly Sync<float> rotationSpeed;
		[Default(0.65f)]

		public readonly Sync<float> activationDistance;
		[Default(125f)]

		public readonly Sync<float> activationAngle;
		[Default(0.15f)]

		public readonly Sync<float> deactivationDistance;
		[Default(10f)]

		public readonly Sync<float> deactivationAngle;

		private bool _activated;

		private Vector3f _targetPosition = Vector3f.Zero;

		private Quaternionf _targetRotation = Quaternionf.Identity;

		protected override void OnAttach() {
			targetUser.Target = LocalUser;
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			_activated = true;
			_targetPosition = Entity.GlobalTrans.Translation;
			_targetRotation = Entity.GlobalTrans.Rotation;
		}

		protected override void RenderStep() {
			if (targetUser.Target == LocalUser && LocalUser.userRoot.Target != null) {
				var HeadPos = LocalUser.userRoot.Target.head.Target.GlobalTrans.Translation;
				var HeadRot = LocalUser.userRoot.Target.head.Target.GlobalTrans.Rotation;
				if (rotateVerticalOnly.Value) {
					var UserEntity = LocalUser.userRoot.Target.Entity;
					HeadRot = UserEntity.GlobalRotToLocal(HeadRot);
					var temp = HeadRot * Vector3f.AxisZ;
					var forward = new Vector3f(temp.x, 0, temp.z).Normalized;
					HeadRot = Quaternionf.LookRotation(forward, Vector3f.AxisY);
					HeadRot = UserEntity.LocalRotToGlobal(HeadRot);
				}
				var dist = HeadPos.Distance(Entity.GlobalTrans.Translation);
				var disAngle = HeadRot.Angle(Entity.GlobalTrans.Rotation);
				if (dist >= activationDistance.Value || disAngle >= activationAngle.Value) {
					_activated = true;
				}
				if (dist <= deactivationDistance.Value && disAngle <= deactivationAngle.Value) {
					_activated = false;
				}
				if (_activated) {
					_targetPosition = HeadPos;
					_targetRotation = HeadRot;
				}
			}
			var pos = Vector3f.Lerp(Entity.GlobalTrans.Translation, _targetPosition, (float)(RTime.Elapsedf * positionSpeed.Value));
			var rot = Quaternionf.CreateFromEuler(0f, 0f, 0f);
			rot.SetToSlerp(Entity.GlobalTrans.Rotation, _targetRotation, (float)RTime.Elapsedf * rotationSpeed.Value);
			Entity.GlobalTrans = Matrix.S(1f) * Matrix.R(rot) * Matrix.T(pos);
		}

	}
}