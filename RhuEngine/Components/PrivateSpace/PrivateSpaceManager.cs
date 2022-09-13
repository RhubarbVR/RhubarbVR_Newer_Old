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
	public sealed class PrivateSpaceManager : Component
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

		protected override void OnAttach() {
			base.OnAttach();
			var TaskBarHoldermover = World.RootEntity.AddChild("TaskBarMover");
			TaskBarHoldermover.AttachComponent<UserInterfacePositioner>();
			TaskBarHolder = TaskBarHoldermover.AddChild("TaskBarHolder");
			taskBar = TaskBarHolder.AddChild("TaskBar").AttachComponent<TaskBar>();
			if (Engine.EngineLink.CanRender) {
				RUpdateManager.ExecuteOnStartOfFrame(() => RUpdateManager.ExecuteOnEndOfFrame(() => {
					if (LocalUser.userRoot.Target is null) {
						return;
					}
					KeyBoardHolder = LocalUser.userRoot.Target.Entity.AddChild("KeyBoardHolder");
					KeyBoardHolder.enabled.Value = false;
					keyBoard = KeyBoardHolder.AddChild("KeyBoard").AttachComponent<KeyBoard>();
				}));
			}
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			WorldManager.PrivateSpaceManager = this;
		}
		protected override void RenderStep() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			var head = LocalUser.userRoot.Target?.head.Target;
			var left = LocalUser.userRoot.Target?.leftController.Target;
			var right = LocalUser.userRoot.Target?.rightController.Target;

			if (head != null) {
				if (!Engine.IsInVR && !Engine.inputManager.screenInput.MouseFree) {
					UpdateHeadLazer(head);
				}
				if (Engine.IsInVR) {
					UpdateLazer(left, Handed.Left);
					UpdateLazer(right, Handed.Right);
				}
				UpdateTouch(RInput.Hand(Handed.Right).Wrist, 2, Handed.Right);
				UpdateTouch(RInput.Hand(Handed.Left).Wrist, 1, Handed.Left);
			}
		}


		public bool RunTouchCastInWorld(uint handed, World world, Vector3f pos, ref Vector3f Frompos, ref Vector3f ToPos, Handed handedSide) {
			if (World is null) {
				return false;
			}
			try {
				if (world.PhysicsSim.RayTest(ref Frompos, ref ToPos, out var collider, out var hitnormal, out var hitpointworld)) {
					if (collider.CustomObject is UICanvas uIComponent) {
						World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
						uIComponent.ProcessHitTouch(handed, hitnormal, hitpointworld, handedSide);
					}
					if (collider.CustomObject is PhysicsObject physicsObject) {
						World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
						physicsObject.Touch(handed, hitnormal, hitpointworld, handedSide);
					}
					return true;
				}
			}
			catch {
			}
			return false;
		}


		public void UpdateTouch(Matrix pos, uint handed, Handed handedSide) {
			var Frompos = Matrix.T(Vector3f.AxisY * -0.07f) * pos;
			var ToPos = Matrix.T(Vector3f.AxisY * 0.03f) * pos;
			World.DrawDebugSphere(Frompos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			World.DrawDebugSphere(ToPos, Vector3f.Zero, new Vector3f(0.02f), new Colorf(0, 1, 0, 0.5f));
			var vpos = pos.Translation;
			var vFrompos = Frompos.Translation;
			var vToPos = ToPos.Translation;
			if (RunTouchCastInWorld(handed, World, vpos, ref vFrompos, ref vToPos, handedSide)) {

			}
			else if (RunTouchCastInWorld(handed, World.worldManager.FocusedWorld, vpos, ref vFrompos, ref vToPos, handedSide)) {

			}
		}

		public bool RunLaserCastInWorld(World world, ref Vector3f headFrompos, ref Vector3f headToPos, uint touchUndex, float pressForce, float gripForces, Handed side) {
			if (world.PhysicsSim.RayTest(ref headFrompos, ref headToPos, out var collider, out var hitnormal, out var hitpointworld)) {

				if (collider.CustomObject is UICanvas uIComponent) {
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.005f), new Colorf(1, 1, 0, 0.5f));
					uIComponent.ProcessHitLazer(touchUndex, hitnormal, hitpointworld, pressForce, gripForces, side);
				}
				if (collider.CustomObject is PhysicsObject physicsObject) {
					World.DrawDebugSphere(Matrix.T(hitpointworld), Vector3f.Zero, new Vector3f(0.02f), new Colorf(1, 1, 0, 0.5f));
					physicsObject.Lazer(touchUndex, hitnormal, hitpointworld, pressForce, gripForces, side);
				}
				if (collider.CustomObject is IWorldObject syncObject) {
					if (RInput.Key(Key.I).IsJustActive() && !Engine.HasKeyboard) {
						if (syncObject.World.IsPersonalSpace) {
							return true;
						}
						var ReletiveEntity = syncObject.World.GetLocalUser()?.userRoot.Target?.head.Target ?? syncObject.World.RootEntity;
						var observer = (syncObject.World.GetLocalUser()?.userRoot.Target?.Entity.parent.Target ?? syncObject.World.RootEntity).AddChild("Observer");
						observer.AttachComponent<ObserverWindow>().Observerd.Target = syncObject.GetClosedEntity();
						observer.GlobalTrans = Matrix.T(-0.5f, -0.5f, -1) * ReletiveEntity.GlobalTrans;
					}
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
			var vheadFrompos = headFrompos.Translation;
			var vheadToPos = headToPos.Translation;
			if (RunLaserCastInWorld(World, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, handed)) {

			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, handed)) {

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
			var vheadFrompos = headFrompos.Translation;
			var vheadToPos = headToPos.Translation;
			if (RunLaserCastInWorld(World, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, Handed.Max)) {

			}
			else if (RunLaserCastInWorld(World.worldManager.FocusedWorld, ref vheadFrompos, ref vheadToPos, 10, PressForce, GripForce, Handed.Max)) {

			}
		}

		public void KeyBoardUpdate(Matrix openLocation) {
			if (keyBoard is null) {
				return;
			}
			KeyBoardHolder.enabled.Value = Engine.HasKeyboard;
			keyBoard.uICanvas.Entity.GlobalTrans = Matrix.T(new Vector3f(0, -0.25f, 0.1f)) * openLocation;
		}
	}
}
