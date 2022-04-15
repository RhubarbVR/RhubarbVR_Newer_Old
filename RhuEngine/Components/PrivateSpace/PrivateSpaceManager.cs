using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Normal)]
	public class PrivateSpaceManager : Component
	{
		public override void OnLoaded() {
			base.OnLoaded();
			var shape = new RCapsuleShape(1, 1);
			var colider = shape.GetCollider(World.PhysicsSim);
			colider.Active = true;
			colider.Matrix = Entity.GlobalTrans;
		}
		public override void Step() {
			var head = LocalUser.userRoot.Target?.head.Target;
			if (head != null) {
				var headPos = head.GlobalTrans;
				var headFrompos = headPos.Translation;
				var headToPos = headPos.Translation + (headPos.Rotation.AxisZ * -10);
				if (World.PhysicsSim.RayTest(ref headFrompos, ref headToPos, out var collider, out var hitnormal,out var hitpointworld)) {
					RLog.Info($"Hit pos {hitpointworld}");
				}
			}
		}
	}
}
