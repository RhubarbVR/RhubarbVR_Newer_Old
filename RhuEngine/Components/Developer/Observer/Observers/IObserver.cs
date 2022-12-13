using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	public static class ObserverHelper
	{

		public static Type GetObserverFromType(this Type checktype) {
			if (!typeof(IWorldObject).IsAssignableFrom(checktype)) {
				return null;
			}
			if (typeof(Entity).IsAssignableFrom(checktype)) {
				return typeof(ObserverEntity);
			}
			if (typeof(Component).IsAssignableFrom(checktype)) {
				return typeof(ObserverComponent);
			}
			if (typeof(ISyncMember).IsAssignableFrom(checktype)) {
				if (typeof(ILinkerMember<bool>).IsAssignableFrom(checktype)) {
					return typeof(BoolSyncObserver);
				}
				if (typeof(ILinkerMember<byte>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<byte>);
				}
				if (typeof(ILinkerMember<char>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<char>);
				}
				if (typeof(ILinkerMember<double>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<double>);
				}
				if (typeof(ILinkerMember<short>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<short>);
				}
				if (typeof(ILinkerMember<int>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<int>);
				}
				if (typeof(ILinkerMember<long>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<long>);
				}
				if (typeof(ILinkerMember<sbyte>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<sbyte>);
				}
				if (typeof(ILinkerMember<float>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<float>);
				}
				if (typeof(ILinkerMember<ushort>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<ushort>);
				}
				if (typeof(ILinkerMember<uint>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<uint>);
				}
				if (typeof(ILinkerMember<ulong>).IsAssignableFrom(checktype)) {
					return typeof(NumberSyncObserver<ulong>);
				}
				if (typeof(ILinkerMember<string>).IsAssignableFrom(checktype)) {
					return typeof(PrimitiveSyncObserver);
				}
				if (typeof(ILinkerMember<Type>).IsAssignableFrom(checktype)) {
					return typeof(PrimitiveSyncObserver);
				}
				if (typeof(ILinkerMember<decimal>).IsAssignableFrom(checktype)) {
					return typeof(PrimitiveSyncObserver);
				}
				if (typeof(ISync).IsAssignableFrom(checktype)) {
					if (checktype.IsGenericType) {
						var innerTypes = checktype.GetGenericArguments();
						if (innerTypes.Length == 1) {
							if (innerTypes[0].IsEnum) {
								return typeof(PrimitiveSyncObserver);
							}
							if (innerTypes[0].IsValueType) {
								return typeof(MultiNumberSyncObserver<>).MakeGenericType(innerTypes[0]);
							}
						}
					}
					return typeof(PrimitiveSyncObserver);
				}
				if (typeof(ISyncList).IsAssignableFrom(checktype)) {
					if (checktype.IsGenericType) {
						var innerTypes = checktype.GetGenericArguments();
						if (innerTypes.Length == 1) {
							return typeof(ObserverListBase<>).MakeGenericType(innerTypes[0]);
						}
					}
				}
			}
			return typeof(ObserverWorldObject);
		}

		public static Type GetObserver(this IWorldObject worldObject) {
			return worldObject.GetType().GetObserverFromType();
		}

	}

	public interface IObserver : IComponent
	{
		public Task SetObserverd(IWorldObject target);

	}

	public abstract class ObserverBase<T> : Component, IObserver where T : class, IWorldObject
	{
		public const int ELMENTHIGHTSIZE = 32;
		public T TargetElement => Observerd.Target;

		[OnChanged(nameof(RunChangeObserverd))]
		public readonly SyncRef<T> Observerd;

		protected abstract void LoadValueIn();
		protected void LoadChangeUpdate(IChangeable changeable) {
			LoadValueIn();
		}

		IChangeable _lastChangeable;

		protected virtual void EveryUserOnLoad() {
			if (TargetElement is IChangeable changeable) {
				if (_lastChangeable is not null) {
					_lastChangeable.Changed -= LoadChangeUpdate;
				}
				_lastChangeable = changeable;
				changeable.Changed += LoadChangeUpdate;
			}
			LoadValueIn();
		}

		public void RunChangeObserverd() {
			Task.Run(ChangeObserverd);
		}

		protected async Task ChangeObserverd() {
			if (LocalUser != MasterUser) {
				EveryUserOnLoad();
				return;
			}
			Entity.DestroyChildren();
			if (Entity.parent.Target is null) {
				return;
			}
			var uiBuilder = new UIBuilder2D(Entity);
			await LoadObservedUI(uiBuilder);
			EveryUserOnLoad();
		}

		protected abstract Task LoadObservedUI(UIBuilder2D ui);

		public Task SetObserverd(IWorldObject target) {
			Observerd.SetTargetNoChange(target as T);
			return ChangeObserverd();
		}


	}
}