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
		RSphereShape _shape;
		public override void OnLoaded() {
			base.OnLoaded();
			_shape = new RSphereShape(0.01f);
		}
		public override void Step() {
			var head = LocalUser.userRoot.Target?.head.Target;
			if (head != null) {
				UpdateHeadLazer(head);
				UpdateTouch(Handed.Right);
				UpdateTouch(Handed.Left);
			}
		}

		public Vector3f[] poses = new Vector3f[2];

		public void UpdateTouch(Handed handed) {
			var pos = RInput.Hand(handed).Wrist;
			var Frompos = Matrix.T(Vector3f.AxisY * -0.07f) * pos;
			var ToPos = Matrix.T(Vector3f.AxisY * 0.03f) * pos;
			World.DrawDebugSphere(Frompos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			World.DrawDebugSphere(ToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			if (World.PhysicsSim.ConvexRayTest(_shape, ref Frompos, ref ToPos, out var collider, out var hitnormal, out var hitpointworld)) {
				RLog.Info($"Hit Tocuh Local {Frompos} {ToPos} pos {hitpointworld}");
			}
			else {
				if (WorldManager.FocusedWorld?.PhysicsSim.ConvexRayTest(_shape,ref Frompos, ref ToPos, out collider, out hitnormal, out hitpointworld) ?? false) {
					if (collider.CustomObject is RenderUIComponent uIComponent) {
						if (Vector3f.Zero == poses[(int)handed]) {
							poses[(int)handed] = Frompos.Translation;
						}
						var pressForce = poses[(int)handed] - Frompos.Translation;
						World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
						uIComponent.Rect.AddHitPoses(new HitData { Laser = false, HitPosWorld = hitpointworld, HitNormalWorld = hitnormal, PressForce = 1, Touchindex = (uint)handed });
					}
				}
				else {
					poses[(int)handed] = Vector3f.Zero;
				}
			}
		}

		public void UpdateHeadLazer(Entity head) {
			var headPos = head.GlobalTrans;
			var headFrompos = headPos;
			var headToPos = Matrix.T(Vector3f.AxisZ * -7) * headPos;
			World.DrawDebugSphere(headToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			if (World.PhysicsSim.ConvexRayTest(_shape, ref headFrompos, ref headToPos, out var collider, out var hitnormal, out var hitpointworld)) {
				RLog.Info($"Hit Local pos {hitpointworld}");
			}
			else {
				if (WorldManager.FocusedWorld?.PhysicsSim.ConvexRayTest(_shape, ref headFrompos, ref headToPos, out collider, out hitnormal, out hitpointworld) ?? false) {
					if (collider.CustomObject is RenderUIComponent uIComponent) {
						World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
						uIComponent.Rect.AddHitPoses(new HitData {Touchindex = 10, Laser = true, HitPosWorld = hitpointworld, HitNormalWorld = hitpointworld, PressForce = RInput.Key(Key.MouseLeft).IsActive() ? 1f : 0f });
					}
				}
			}
		}

	}
}
