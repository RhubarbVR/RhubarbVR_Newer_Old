using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
namespace RhuEngine.Components
{
	public abstract class LocomotionModule : Component
	{
		public AssetRef<RTexture2D> icon;

		public Sync<string> locmotionName;

		public abstract void ProcessMovement();

		public float MoveSpeed => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.MoveSpeed);

		public float Jump => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Jump) * RTime.Elapsedf;

		public float Forward => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Forward) * RTime.Elapsedf;

		public float Back => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Back) * RTime.Elapsedf;

		public float Right => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Right) * RTime.Elapsedf;

		public float Left => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.Left) * RTime.Elapsedf;

		public float FlyDown => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyDown) * RTime.Elapsedf;

		public float FlyUp => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.FlyUp) * RTime.Elapsedf;

		public float RotateLeft => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateLeft) * RTime.Elapsedf;

		public float RotateRight => Engine.inputManager.GetInputFloat(Managers.InputManager.InputTypes.RotateRight) * RTime.Elapsedf;

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
