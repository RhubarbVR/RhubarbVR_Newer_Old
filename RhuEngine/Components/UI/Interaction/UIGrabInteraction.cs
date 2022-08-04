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
		[Default(false)]
		public readonly Sync<bool> AllowOtherZones;

		[Default(0.5f)]
		public readonly Sync<float> GrabForce;

		public readonly SyncDelegate<Action<Handed>> Grabeded;

		public void Grab(Handed handed) {
			RWorld.ExecuteOnEndOfFrame(() => Grabeded.Target?.Invoke(handed));
		}

		public bool Grabed = true;
		//public override void Step() {
		//	base.Step();
		//	if(Rect is null) {
		//		return;
		//	}
		//	var gabbedthisframe = false;
		//	foreach (var item in Rect.HitPoses(!AllowOtherZones.Value))
		//	{
		//		if (item.GripForce > GrabForce.Value) {
		//			if (!Grabed) {
		//				Grabed = true;
		//				Grab(item.Handed);
		//			}
		//			gabbedthisframe = true;
		//		}
		//	}
		//	if(!gabbedthisframe && Grabed) {
		//		Grabed = false;
		//	}
		//}
	}
}
