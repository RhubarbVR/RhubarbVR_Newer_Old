using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;

namespace RhuEngine.Components
{
	public abstract class ValueObserverBase<T> : ObserverSyncElement<ILinkerMember<T>>
	{

		protected abstract void BuildUI(UIBuilder ui);

		protected abstract void ValueChanged();

		IChangeable _changeable;
		protected override void LoadSideUI(UIBuilder ui) {
			BuildUI(ui);
		}

		protected override void EveryUserOnLoad() {
			if (TargetElement is IChangeable changeable) {
				if (_changeable is not null) {
					changeable.Changed -= Changeable_Changed;
					_changeable = null;
				}
				changeable.Changed += Changeable_Changed;
				Changeable_Changed(null);
			}
		}

		private void Changeable_Changed(IChangeable obj) {
			if (TargetElement is null) {
				return;
			}
			ValueChanged();
		}

	}
}
