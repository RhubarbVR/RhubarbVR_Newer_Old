using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "User" })]
	public class NoClipLocomotion : LocomotionModule
	{
		[Default(1f)]
		public Sync<float> MovementSpeed;
		[Default(30f)]
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
			var speed = AllowMultiplier ? SKMath.Lerp(MovementSpeed, MaxSprintSpeed, MoveSpeed) : MovementSpeed;
			var tempRight = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Right, isMain) * Time.Elapsedf;
			var tempLeft = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Left, isMain) * Time.Elapsedf;
			var tempFlyUp = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyUp, isMain) * Time.Elapsedf;
			var tempFlyDown = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyDown, isMain) * Time.Elapsedf;
			var tempForward = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Forward, isMain) * Time.Elapsedf;
			var tempBack = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Back, isMain) * Time.Elapsedf;
			var pos = new Vec3(tempRight - tempLeft, tempFlyUp - tempFlyDown, -tempForward + tempBack) * speed;
			var Rotspeed = AllowMultiplier ? SKMath.Lerp(RotationSpeed, MaxSprintRotationSpeed, MoveSpeed) : RotationSpeed;
			var tempRotateRight = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateRight, isMain) * Time.Elapsedf;
			var tempRotateLeft = Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateLeft, isMain) * Time.Elapsedf;
			var rot = Quat.FromAngles(0, (tempRotateRight - tempRotateLeft) * RotationSpeed, 0);
			var AddToMatrix = Matrix.T(pos);
			switch (Engine.inputManager.GetHand(isMain)) {
				case Handed.Left:
					ProcessGlobalRotToUserRootMovement(AddToMatrix,Matrix.R(Quat.FromAngles(0,90,0)) * LocalUser.userRoot.Target?.leftHand.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
					break;
				case Handed.Right:
					ProcessGlobalRotToUserRootMovement(AddToMatrix, Matrix.R(Quat.FromAngles(0, 90, 0)) * LocalUser.userRoot.Target?.rightHand.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
					break;
				default:
					break;
			}
			UserRootEnity.rotation.Value *= rot;
		}

		private void ProcessHeadBased() {
			var speed = AllowMultiplier ? SKMath.Lerp(MovementSpeed, MaxSprintSpeed, MoveSpeed) : MovementSpeed;
			var pos = new Vec3(Right - Left, FlyUp - FlyDown, -Forward + Back) * speed;
			var Rotspeed = AllowMultiplier ? SKMath.Lerp(RotationSpeed, MaxSprintRotationSpeed, MoveSpeed) : RotationSpeed;
			var rot = Quat.FromAngles(0, (RotateRight - RotateLeft) * RotationSpeed, 0);
			var AddToMatrix = Matrix.T(pos);
			ProcessGlobalRotToUserRootMovement(AddToMatrix, LocalUser.userRoot?.Target.head.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
			UserRootEnity.rotation.Value *= rot;
		}

		public override void ProcessMovement() {
			if(UserRootEnity is null) {
				return;
			}
			if(SK.ActiveDisplayMode == DisplayMode.Flatscreen) {
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
