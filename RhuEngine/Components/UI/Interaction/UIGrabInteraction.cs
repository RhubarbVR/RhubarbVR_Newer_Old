using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/Interaction" })]
	public class UIGrabInteraction : UIInteractionComponent
	{
		[Default(0.5f)]
		public readonly Sync<float> GrabForce;

		public readonly SyncDelegate<Action<Handed>> Grabeded;

		public void Grab(Handed handed) {
			RUpdateManager.ExecuteOnEndOfFrame(() => Grabeded.Target?.Invoke(handed));
		}

		public bool Grabed = true;
		protected override void Step() {
			base.Step();
			if (UIRect is null) {
				return;
			}
			var gabbedthisframe = false;
			foreach (var item in UIRect.GetRectHitData()) {
				if (item.GripForces > GrabForce.Value) {
					if (!Grabed) {
						Grabed = true;
						Grab(item.Side);
					}
					gabbedthisframe = true;
				}
			}
			if (!gabbedthisframe && Grabed) {
				Grabed = false;
			}
		}
	}
}
