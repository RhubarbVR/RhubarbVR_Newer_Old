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
using RhuEngine.WorldObjects;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public class PrivateSpaceManager : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public TaskBar taskBar;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity TaskBarHolder;
		public override void OnAttach() {
			base.OnAttach();
			var TaskBarHoldermover = World.RootEntity.AddChild("TaskBarMover");
			TaskBarHoldermover.AttachComponent<UserInterfacePositioner>();
			TaskBarHolder = TaskBarHoldermover.AddChild("TaskBarHolder");
			taskBar = TaskBarHolder.AddChild("TaskBar").AttachComponent<TaskBar>();
		}

		RSphereShape _shape;
		public override void OnLoaded() {
			base.OnLoaded();
			_shape = new RSphereShape(0.02f);
		}
		public override void Step() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			var head = LocalUser.userRoot.Target?.head.Target;
			if (head != null) {
				UpdateHeadLazer(head);
				UpdateTouch(RInput.Hand(Handed.Right).Wrist,2);
				UpdateTouch(RInput.Hand(Handed.Left).Wrist,1);
			}
		}

		public Matrix[] poses = new Matrix[2];

		public bool RunTouchCastInWorld(uint handed, World world, Matrix pos, ref Matrix Frompos, ref Matrix ToPos) {
			if(World is null) {
				return false;
			}
			if (world.PhysicsSim.ConvexRayTest(_shape, ref Frompos, ref ToPos, out var collider, out var hitnormal, out var hitpointworld)) {
				if (collider.CustomObject is RenderUIComponent uIComponent) {
					if (Matrix.Identity == poses[(int)handed]) {
						poses[(int)handed] = pos;
					}
					var pressForce = (poses[(int)handed] * pos.Inverse).Translation.z * 20;
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
					uIComponent.Rect.AddHitPoses(new HitData { Laser = false, HitPosWorld = hitpointworld, HitNormalWorld = hitnormal, PressForce = pressForce, Touchindex = handed, Handed = (handed == 2)?Handed.Right:Handed.Left });
				}
				return true;
			}
			return false;
		}


		public void UpdateTouch(Matrix pos, uint handed) {
			var Frompos = Matrix.T(Vector3f.AxisY * -0.07f) * pos;
			var ToPos = Matrix.T(Vector3f.AxisY * 0.03f) * pos;
			World.DrawDebugSphere(Frompos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			World.DrawDebugSphere(ToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			if(RunTouchCastInWorld(handed,World,pos,ref Frompos,ref ToPos)) {

			}
			else if (RunTouchCastInWorld(handed, World.worldManager.FocusedWorld, pos, ref Frompos, ref ToPos)) {

			}
			else {
				if (poses.Length <= (int)handed) {
					Array.Resize(ref poses, (int)handed + 1);
				}
				poses[(int)handed] = Matrix.Identity;
			}
		}

		public bool RunLaserCastInWorld(World world, ref Matrix headFrompos, ref Matrix headToPos) {
			if (world.PhysicsSim.ConvexRayTest(_shape, ref headFrompos, ref headToPos, out var collider, out var hitnormal, out var hitpointworld)) {
				if (collider.CustomObject is RenderUIComponent uIComponent) {
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
					uIComponent.Rect.AddHitPoses(new HitData { Touchindex = 10, Laser = true, HitPosWorld = hitpointworld, HitNormalWorld = hitpointworld, PressForce = Engine.inputManager.GetInputFloatFromKeyboard(Managers.InputManager.InputTypes.Primary), GripForce = Engine.inputManager.GetInputFloatFromKeyboard(Managers.InputManager.InputTypes.Grab), Handed = Handed.Max });
				}
				return true;
			}
			return false;
		}

		public void UpdateHeadLazer(Entity head) {
			var headPos = head.GlobalTrans;
			var headFrompos = headPos;
			var headToPos = Matrix.T(Vector3f.AxisZ * -5) * headPos;
			World.DrawDebugSphere(headToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			if (RunLaserCastInWorld(World, ref headFrompos, ref headToPos)) {

			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref headFrompos, ref headToPos)) {

			}
		}

	}
}
