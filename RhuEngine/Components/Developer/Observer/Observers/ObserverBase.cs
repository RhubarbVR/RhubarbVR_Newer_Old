using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;

namespace RhuEngine.Components
{
	public static class ObserverHelper {

		public static Type GetObserverFromType(this Type checktype) {
			if (!typeof(IWorldObject).IsAssignableFrom(checktype)) {
				return null;
			}
			if (typeof(Entity).IsAssignableFrom(checktype)) { 
				return typeof(ObserverEnity);
			}
			if (typeof(Component).IsAssignableFrom(checktype)) {
				return typeof(ObserverComponent);
			}
			if (typeof(ISyncMember).IsAssignableFrom(checktype)) {
				if (typeof(ILinkerMember<bool>).IsAssignableFrom(checktype)) {
					return typeof(BoolSyncObserver);
				}
				if (typeof(ISync).IsAssignableFrom(checktype)) {
					return typeof(PrimitiveSyncObserver);
				}
				return null;
			}
			return typeof(ObserverWorldObject);
		}

		public static Type GetObserver(this IWorldObject worldObject) {
			return worldObject.GetType().GetObserverFromType();
		}

	}

	public interface IObserver : IComponent
	{
		public void SetObserverd(IWorldObject target);

		public void SetUIRectAndMat(IAssetProvider<RMaterial> mat);
	}

	public abstract class ObserverBase<T> : Component, IObserver where T : class, IWorldObject
	{
		public const float ELMENTHIGHTSIZE = 0.65f;
		public T TargetElement => Observerd.Target;

		[OnChanged(nameof(ChangeObserverd))]
		public readonly SyncRef<T> Observerd;

		public readonly AssetRef<RMaterial> Mat;

		protected void ChangeObserverd() {
			if (LocalUser != MasterUser) {
				return;
			}
			Entity.DestroyChildren();
			if(Entity.parent.Target is null) {
				return;
			}
			var uiBuilder = new UIBuilder(Entity, Mat.Target, Entity.UIRect?? BuildMainUIRect(), true,true);
			LoadObservedUI(uiBuilder);
		}
		protected abstract UIRect BuildMainUIRect();

		protected abstract void LoadObservedUI(UIBuilder ui);

		public void SetObserverd(IWorldObject target) {
			Observerd.TargetIWorldObject = target;
		}

		public void SetUIRectAndMat(IAssetProvider<RMaterial> mat) {
			Mat.Target = mat;
		}
	}
}
