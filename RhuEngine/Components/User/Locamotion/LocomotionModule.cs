using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
namespace RhuEngine.Components
{
	public abstract class LocomotionModule : Component
	{
		public readonly AssetRef<RTexture2D> icon;

		public readonly Sync<string> locmotionName;

		public abstract void ProcessMovement();

		public float MoveSpeed => Engine.inputManager.GetInputAction(InputTypes.MoveSpeed).RawValue();

		public float Jump => Engine.inputManager.GetInputAction(InputTypes.Jump).RawValue() * RTime.Elapsedf;

		public float Forward => Engine.inputManager.GetInputAction(InputTypes.Forward).RawValue() * RTime.Elapsedf;

		public float Back => Engine.inputManager.GetInputAction(InputTypes.Back).RawValue() * RTime.Elapsedf;

		public float Right => Engine.inputManager.GetInputAction(InputTypes.Right).RawValue() * RTime.Elapsedf;

		public float Left => Engine.inputManager.GetInputAction(InputTypes.Left).RawValue() * RTime.Elapsedf;

		public float FlyDown => Engine.inputManager.GetInputAction(InputTypes.FlyDown).RawValue() * RTime.Elapsedf;

		public float FlyUp => Engine.inputManager.GetInputAction(InputTypes.FlyUp).RawValue() * RTime.Elapsedf;

		public float RotateLeft => Engine.inputManager.GetInputAction(InputTypes.RotateLeft).RawValue() * RTime.Elapsedf;

		public float RotateRight => Engine.inputManager.GetInputAction(InputTypes.RotateRight).RawValue() * RTime.Elapsedf;

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
