using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	public abstract class LocomotionModule : Component
	{
		public AssetRef<Tex> icon;

		public Sync<string> locmotionName;

		public abstract void ProcessMovement();

		public float MoveSpeed => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.MoveSpeed);

		public float Jump => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Jump);

		public float Forward => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Forward);

		public float Back => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Back);

		public float Right => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Right);

		public float Left => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Left);

		public float FlyDown => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyDown);

		public float FlyUp => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyUp);

		public float RotateLeft => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateLeft);

		public float RotateRight => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateRight);


	}
}
