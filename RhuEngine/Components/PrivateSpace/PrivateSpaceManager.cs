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
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public KeyBoard keyBoard;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity KeyBoardHolder;

		public override void OnAttach() {
			base.OnAttach();
			var TaskBarHoldermover = World.RootEntity.AddChild("TaskBarMover");
			TaskBarHoldermover.AttachComponent<UserInterfacePositioner>();
			TaskBarHolder = TaskBarHoldermover.AddChild("TaskBarHolder");
			taskBar = TaskBarHolder.AddChild("TaskBar").AttachComponent<TaskBar>();
			if (Engine.EngineLink.CanRender) {
				RWorld.ExecuteOnStartOfFrame(() => RWorld.ExecuteOnEndOfFrame(() => {
					if(LocalUser.userRoot.Target is null) {
						return;
					}
					KeyBoardHolder = LocalUser.userRoot.Target.Entity.AddChild("KeyBoardHolder");
					KeyBoardHolder.enabled.Value = false;
					keyBoard = KeyBoardHolder.AddChild("KeyBoard").AttachComponent<KeyBoard>();
				}));
			}
		}

		RSphereShape _shape;
		RSphereShape _lazershape;

		public override void OnLoaded() {
			base.OnLoaded();
			WorldManager.PrivateSpaceManager = this;
			_shape = new RSphereShape(0.02f);
			_lazershape = new RSphereShape(0.005f);
		}
		public override void RenderStep() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			var head = LocalUser.userRoot.Target?.head.Target;
			var left = LocalUser.userRoot.Target?.leftHand.Target;
			var right = LocalUser.userRoot.Target?.rightHand.Target;

			if (head != null) {
				if (!RWorld.IsInVR && !Engine.inputManager.screenInput.MouseFree) {
					UpdateHeadLazer(head);
				}
				if (RWorld.IsInVR) {
					UpdateLazer(left, Handed.Left);
					UpdateLazer(right, Handed.Right);
				}
				UpdateTouch(RInput.Hand(Handed.Right).Wrist, 2);
				UpdateTouch(RInput.Hand(Handed.Left).Wrist, 1);
			}
		}

		public Matrix[] poses = new Matrix[2];

		public bool RunTouchCastInWorld(uint handed, World world, Matrix pos, ref Matrix Frompos, ref Matrix ToPos) {
			if (World is null) {
				return false;
			}
			try {
				if (world.PhysicsSim.ConvexRayTest(_shape, ref Frompos, ref ToPos, out var collider, out var hitnormal, out var hitpointworld)) {
					if (collider.CustomObject is RenderUIComponent uIComponent) {
						if (Matrix.Identity == poses[(int)handed]) {
							poses[(int)handed] = pos;
						}
						var pressForce = (poses[(int)handed] * pos.Inverse).Translation.z * 20;
						World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
						//uIComponent.Rect.AddHitPoses(new HitData { Laser = false, HitPosWorld = hitpointworld, HitNormalWorld = hitnormal, PressForce = pressForce, Touchindex = handed, Handed = (handed == 2) ? Handed.Right : Handed.Left });
					}
					if (collider.CustomObject is PhysicsObject physicsObject) {
						World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
						physicsObject.Touch(handed, hitnormal, hitpointworld);
					}
					return true;
				}
			}
			catch { 
			}
			return false;
		}


		public void UpdateTouch(Matrix pos, uint handed) {
			var Frompos = Matrix.T(Vector3f.AxisY * -0.07f) * pos;
			var ToPos = Matrix.T(Vector3f.AxisY * 0.03f) * pos;
			World.DrawDebugSphere(Frompos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			World.DrawDebugSphere(ToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			if (RunTouchCastInWorld(handed, World, pos, ref Frompos, ref ToPos)) {

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

		public bool RunLaserCastInWorld(World world, ref Matrix headFrompos, ref Matrix headToPos, uint touchUndex, float pressForce,float gripForces,Handed side) {
			if (world.PhysicsSim.ConvexRayTest(_lazershape, ref headFrompos, ref headToPos, out var collider, out var hitnormal, out var hitpointworld)) {

				if (collider.CustomObject is RenderUIComponent uIComponent) {
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.005f), new Colorf(1, 1, 0, 0.5f));
					//uIComponent.Rect.AddHitPoses(new HitData { Touchindex = touchUndex, Laser = true, HitPosWorld = hitpointworld, HitNormalWorld = hitpointworld, PressForce = pressForce, GripForce = gripForces, Handed = side });
				}
				if (collider.CustomObject is PhysicsObject physicsObject) {
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
					physicsObject.Lazer(touchUndex, hitnormal, hitpointworld, pressForce, gripForces, side);
				}
				return true;
			}
			return false;
		}
		public bool DisableRightLaser;

		public bool DisableLeftLaser;

		public bool DisableHeadLaser;
		public void UpdateLazer(Entity heand, Handed handed) {
			if (handed == Handed.Left) {
				if (DisableLeftLaser) {
					return;
				}
			}
			else {
				if (DisableRightLaser) {
					return;
				}
			}
			var PressForce = Engine.inputManager.GetInputFloatFromController(Managers.InputManager.InputTypes.Primary, handed);
			var GripForce = Engine.inputManager.GetInputFloatFromController(Managers.InputManager.InputTypes.Grab, handed);
			var headPos = heand.GlobalTrans;
			var headFrompos = headPos;
			var headToPos = Matrix.T(Vector3f.AxisZ * -5) * headPos;
			World.DrawDebugSphere(headToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			if (RunLaserCastInWorld(World, ref headFrompos, ref headToPos, 10, PressForce, GripForce, handed)) {

			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref headFrompos, ref headToPos, 10, PressForce, GripForce, handed)) {

			}
		}

		public void UpdateHeadLazer(Entity head) {
			if (DisableHeadLaser) {
				return;
			}
			var PressForce = Engine.inputManager.GetInputFloatFromKeyboard(Managers.InputManager.InputTypes.Primary);
			var GripForce = Engine.inputManager.GetInputFloatFromKeyboard(Managers.InputManager.InputTypes.Grab);
			var headPos = head.GlobalTrans;
			var headFrompos = headPos;
			var headToPos = Matrix.T(Vector3f.AxisZ * -5) * headPos;
			World.DrawDebugSphere(headToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			if (RunLaserCastInWorld(World, ref headFrompos, ref headToPos,10,PressForce,GripForce, Handed.Max)) {

			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref headFrompos, ref headToPos, 10, PressForce, GripForce, Handed.Max)) {

			}
		}

		public void KeyBoardUpdate(Matrix openLocation) {
			if (keyBoard is null) {
				return;
			}
			KeyBoardHolder.enabled.Value = Engine.HasKeyboard;
			keyBoard.uICanvas.Entity.GlobalTrans = Matrix.T(new Vector3f(0,-0.25f,0.1f)) * openLocation;
		}
	}
}
