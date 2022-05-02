using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "User" })]
	public class NoClipLocomotion : LocomotionModule
	{
		[Default(1f)]
		public Sync<float> MovementSpeed;
		[Default(50f)]
		public Sync<float> RotationSpeed;
		[Default(2f)]
		public Sync<float> MaxSprintSpeed;
		[Default(80f)]
		public Sync<float> MaxSprintRotationSpeed;
		[Default(true)]
		public Sync<bool> AllowMultiplier;

		public override void OnAttach() {
			base.OnAttach();
			locmotionName.Value = "No Clip";
		}

		private void ProcessController(bool isMain) {
			var speed = AllowMultiplier ? MathUtil.Lerp(MovementSpeed, MaxSprintSpeed, MoveSpeed) : MovementSpeed;
			var tempRight = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Right, isMain) * RTime.Elapsedf;
			var tempLeft = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Left, isMain) * RTime.Elapsedf;
			var tempFlyUp = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyUp, isMain) * RTime.Elapsedf;
			var tempFlyDown = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyDown, isMain) * RTime.Elapsedf;
			var tempForward = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Forward, isMain) * RTime.Elapsedf;
			var tempBack = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Back, isMain) * RTime.Elapsedf;
			var pos = new Vector3f(tempRight - tempLeft, tempFlyUp - tempFlyDown, -tempForward + tempBack) * speed;
			var Rotspeed = AllowMultiplier ? MathUtil.Lerp(RotationSpeed, MaxSprintRotationSpeed, MoveSpeed) : RotationSpeed;
			var tempRotateRight = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateLeft, isMain) * RTime.Elapsedf;
			var tempRotateLeft = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateRight, isMain) * RTime.Elapsedf;
			var rot = Quaternionf.CreateFromEuler(0, (tempRotateRight - tempRotateLeft) * RotationSpeed, 0);
			var AddToMatrix = Matrix.T(pos);
			switch (Engine.inputManager.GetHand(isMain)) {
				case Handed.Left:
					ProcessGlobalRotToUserRootMovement(AddToMatrix,Matrix.R(Quaternionf.CreateFromEuler(180,0,90)) * LocalUser.userRoot.Target?.leftHand.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
					break;
				case Handed.Right:
					ProcessGlobalRotToUserRootMovement(AddToMatrix, Matrix.R(Quaternionf.CreateFromEuler(0, 0, -90)) * LocalUser.userRoot.Target?.rightHand.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
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
			var rot = Quaternionf.CreateFromEuler(0, (RotateRight - RotateLeft) * RotationSpeed, 0);
			var AddToMatrix = Matrix.T(pos);
			ProcessGlobalRotToUserRootMovement(AddToMatrix, LocalUser.userRoot?.Target.head.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
			UserRootEnity.rotation.Value *= rot;
		}

		public override void ProcessMovement() {
			if(UserRootEnity is null) {
				return;
			}
			if(!RWorld.IsInVR) {
				ProcessHeadBased();
			}
			else {
				if (Engine.MainSettings.InputSettings.HeadBasedMovement) {
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
