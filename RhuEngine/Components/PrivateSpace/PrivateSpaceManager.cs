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
		//Overlap test
		//RigidBodyCollider _rBoxShape;
		public override void OnAttach() {
			//var colliderone = new RBoxShape(0.5f).GetCollider(World.PhysicsSim);
			//colliderone.Matrix = Entity.GlobalTrans;

			//_rBoxShape = new RBoxShape(0.5f).GetCollider(World.PhysicsSim);
			//_rBoxShape.Overlap += RBoxShape_Overlap;
		}
		//private void RBoxShape_Overlap(Vector3f PositionWorldOnA, Vector3f PositionWorldOnB, Vector3f NormalWorldOnB, double Distance, double Distance1, RigidBodyCollider hit) {
		//	RLog.Info($"Overlaped in {PositionWorldOnA} {PositionWorldOnB}");
		//}

		public override void Step() {
			var head = LocalUser.userRoot.Target?.head.Target;
			if (head != null) {
				//UpdateHeadLazer(head);
				UpdateTouch(Handed.Right);
				//UpdateTouch(Handed.Left);
			}
		}

		public Vector3f poses;

		public void UpdateTouch(Handed handed) {
			var pos = RInput.Hand(handed).Wrist;
			var Frompos = pos.Translation;
			var FromoPos = pos.Rotation.AxisZ * -0.05f;
			if (World.PhysicsSim.RayTest(ref Frompos, ref FromoPos, out var collider, out var hitnormal, out var hitpointworld)) {
				RLog.Info($"Hit Tocuh Local {Frompos} {FromoPos} pos {hitpointworld}");
			}
			else {
				if (WorldManager.FocusedWorld?.PhysicsSim.RayTest(ref Frompos, ref FromoPos, out collider, out hitnormal, out hitpointworld) ?? false) {
					if (collider.CustomObject is RenderUIComponent uIComponent) {
						if(Vector3f.Zero == poses) {
							poses = Frompos;
						}
						RWorld.ExecuteOnStartOfFrame(() => uIComponent.Rect.AddHitPoses(new HitData { Laser = false, HitPosWorld = hitpointworld, HitNormalWorld = hitnormal,PressForce = 1,Touchindex = (uint)handed}));
					}
					RLog.Info($"Hit Tocuh {Frompos} {FromoPos} pos {hitpointworld}");
				}
				else {
					poses = Vector3f.Zero;
				}
			}
		}

		public void UpdateHeadLazer(Entity head) {
			var headPos = head.GlobalTrans;
			var headFrompos = headPos.Translation;
			var headToPos = headPos.Rotation.AxisZ * -10;
			if (World.PhysicsSim.RayTest(ref headFrompos, ref headToPos, out var collider, out var hitnormal, out var hitpointworld)) {
				RLog.Info($"Hit Local pos {hitpointworld}");
			}
			else {
				if (WorldManager.FocusedWorld?.PhysicsSim.RayTest(ref headFrompos, ref headToPos, out collider, out hitnormal, out hitpointworld) ?? false) {
					if (collider.CustomObject is RenderUIComponent uIComponent) {
						RWorld.ExecuteOnStartOfFrame(() => uIComponent.Rect.AddHitPoses(new HitData { Laser = true, HitPosWorld = hitpointworld, HitNormalWorld = hitpointworld, PressForce = RInput.Key(Key.MouseLeft).IsActive() ? 1f : 0f }));
					}
				}
			}
		}

	}
}
