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

		public float Jump => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Jump) * Time.Elapsedf;

		public float Forward => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Forward) * Time.Elapsedf;

		public float Back => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Back) * Time.Elapsedf;

		public float Right => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Right) * Time.Elapsedf;

		public float Left => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Left) * Time.Elapsedf;

		public float FlyDown => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyDown) * Time.Elapsedf;

		public float FlyUp => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyUp) * Time.Elapsedf;

		public float RotateLeft => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateLeft) * Time.Elapsedf;

		public float RotateRight => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateRight) * Time.Elapsedf;

		public Entity UserRootEnity => World.GetLocalUser()?.userRoot.Target?.Entity;

		public void ProcessGlobalRotToUserRootMovement(Matrix addingMatrix,Matrix globalmat) {
			if(UserRootEnity is null) {
				return;
			}
			var childM = UserRootEnity.GlobalToLocal(globalmat);
			var targetHeadM = addingMatrix * childM;
			var userRootAdd = childM.Inverse * targetHeadM;
			UserRootEnity.LocalTrans = userRootAdd * UserRootEnity.LocalTrans;
		}

	}
}
