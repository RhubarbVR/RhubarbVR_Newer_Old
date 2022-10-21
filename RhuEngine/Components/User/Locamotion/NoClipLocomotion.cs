using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "User" })]
	public sealed class NoClipLocomotion : LocomotionModule
	{
		[Default(1f)]
		public readonly Sync<float> MovementSpeed;
		[Default(50f)]
		public readonly Sync<float> RotationSpeed;
		[Default(2f)]
		public readonly Sync<float> MaxSprintSpeed;
		[Default(80f)]
		public readonly Sync<float> MaxSprintRotationSpeed;
		[Default(true)]
		public readonly Sync<bool> AllowMultiplier;

		protected override void OnAttach() {
			base.OnAttach();
			locmotionName.Value = "No Clip";
		}

		private void ProcessController(bool isMain) {
			var speed = AllowMultiplier ? MathUtil.Lerp(MovementSpeed, MaxSprintSpeed, MoveSpeed) : MovementSpeed;
			var tempRight = InputManager.GetInputAction(InputTypes.Right).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsedf;
			var tempLeft = InputManager.GetInputAction(InputTypes.Left).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsedf;
			var tempFlyUp = InputManager.GetInputAction(InputTypes.FlyUp).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsedf;
			var tempFlyDown = InputManager.GetInputAction(InputTypes.FlyDown).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsedf;
			var tempForward = InputManager.GetInputAction(InputTypes.Forward).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsedf;
			var tempBack = InputManager.GetInputAction(InputTypes.Back).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsedf;
			var pos = new Vector3f(tempRight - tempLeft, tempFlyUp - tempFlyDown, -tempForward + tempBack) * speed;
			var Rotspeed = AllowMultiplier ? MathUtil.Lerp(RotationSpeed, MaxSprintRotationSpeed, MoveSpeed) : RotationSpeed;
			var tempRotateRight = InputManager.GetInputAction(InputTypes.RotateLeft).HandedValue(InputManager.GetHand(!isMain)) * RTime.Elapsedf;
			var tempRotateLeft = InputManager.GetInputAction(InputTypes.RotateRight).HandedValue(InputManager.GetHand(!isMain)) * RTime.Elapsedf;
			var rot = Quaternionf.CreateFromEuler((tempRotateRight - tempRotateLeft) * RotationSpeed, 0, 0);
			var AddToMatrix = Matrix.T(pos);
			switch (Engine.inputManager.GetHand(isMain)) {
				case Handed.Left:
					ProcessGlobalRotToUserRootMovement(AddToMatrix, LocalUser.userRoot.Target?.leftController.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
					break;
				case Handed.Right:
					ProcessGlobalRotToUserRootMovement(AddToMatrix, LocalUser.userRoot.Target?.rightController.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
					break;
				default:
					break;
			}
			UserRootEnity.rotation.Value *= rot;
		}

		private void ProcessHeadBased() {
			var speed = AllowMultiplier ? MathUtil.Lerp(MovementSpeed, MaxSprintSpeed, MoveSpeed) : MovementSpeed;
			var pos = new Vector3f(Right - Left, FlyUp - FlyDown, Back - Forward) * speed;
			var Rotspeed = AllowMultiplier ? MathUtil.Lerp(RotationSpeed, MaxSprintRotationSpeed, MoveSpeed) : RotationSpeed;
			var rot = Quaternionf.CreateFromEuler((RotateRight - RotateLeft) * RotationSpeed, 0, 0);
			var AddToMatrix = Matrix.T(pos);
			ProcessGlobalRotToUserRootMovement(AddToMatrix, LocalUser.userRoot?.Target.head.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
			UserRootEnity.rotation.Value *= rot;
		}

		public override void ProcessMovement() {
			if (UserRootEnity is null) {
				return;
			}
			if (!Engine.IsInVR) {
				ProcessHeadBased();
			}
			else {
				if (Engine.MainSettings.InputSettings.MovmentSettings.HeadBasedMovement) {
					ProcessHeadBased();
				}
				else {
					ProcessController(false);
					ProcessController(true);
				}
			}

		}
	}
}
