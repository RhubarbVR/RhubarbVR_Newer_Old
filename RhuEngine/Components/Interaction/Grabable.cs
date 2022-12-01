using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using RhuEngine.Input.XRInput;

namespace RhuEngine.Components
{
	[Category(new string[] { "Interaction" })]
	[SingleComponentLock]
	public sealed class Grabbable : Component
	{
		public readonly SyncRef<Entity> lastParent;

		[Default(true)]
		public readonly Sync<bool> IsLaserGrabbable;

		public readonly Sync<bool> CanNotDestroy;

		[Default(0.5f)]
		public readonly Sync<float> GripForce;

		public readonly SyncRef<User> grabbingUser;

		public readonly SyncRef<GrabbableHolder> grabbableHolder;

		public Handed GabbedSide => grabbableHolder.Target.source.Value;

		public bool LaserGrabbed;
		public Matrix StartingPos;

		public bool Grabbed => grabbableHolder.Target is not null && grabbingUser.Target is not null;


		protected override void OnAttach() {
			base.OnAttach();
			foreach (var item in Entity.GetAllComponents<PhysicsObject>()) {
				item.CursorShape.Value = RCursorShape.Move;
			}
		}

		public void DestroyGrabbedObject() {
			if (!CanNotDestroy.Value) {
				Entity.Destroy();
			}
		}
		public void Drop() {
			if (LocalUser != grabbingUser.Target) {
				return;
			}

			grabbingUser.Target = null;
			Entity.SetParent(lastParent.Target);
			try {
				grabbableHolder.Target.GrabbedObjects.Remove(this);
			}
			catch { }
			grabbableHolder.Target = null;
			Entity.CallOnDroped();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			Entity.OnLazerPyhsics += Entity_OnLazerPyhsics;
			Entity.OnGrip += GripProcess;
		}

		public override void Dispose() {
			Entity.OnLazerPyhsics -= Entity_OnLazerPyhsics;
			Entity.OnGrip -= GripProcess;
			base.Dispose();
		}

		private void Entity_OnLazerPyhsics(uint arg1, Vector3f arg2, Vector3f arg3, float arg4, float arg5, Handed handed) {
			if (arg1 == 10) {
				switch (handed) {
					case Handed.Left:
						GripProcess(World.LeftGrabbableHolder, true, arg5);
						break;
					case Handed.Right:
						GripProcess(World.RightGrabbableHolder, true, arg5);
						break;
					case Handed.Max:
						GripProcess(World.HeadGrabbableHolder, true, arg5);
						break;
					default:
						break;
				}
			}
		}

		internal void GripProcess(GrabbableHolder obj, bool Laser, float gripForce) {
			if (gripForce < GripForce.Value) {
				return;
			}
			if (grabbableHolder.Target == obj) {
				return;
			}
			if (obj == null) {
				return;
			}
			if (obj.holder.Target == null) {
				return;
			}

			if (obj.grippingLastFrame) {
				return;
			}
			if (!IsLaserGrabbable.Value && Laser) {
				return;
			}
			LaserGrabbed = Laser;
			if (!Grabbed) {
				lastParent.Target = Entity.parent.Target;
			}
			grabbableHolder.Target = obj;
			grabbingUser.Target = LocalUser;
			Entity.SetParent(obj.holder.Target);
			try {
				grabbableHolder.Target.GrabbedObjects.Add(this);
			}
			catch { }
			var LocalToUser = LocalUser.userRoot.Target.Entity.GlobalToLocal(Entity.GlobalTrans);
			var aimPos = InputManager.XRInputSystem.GetHand(obj.source.Value)?[TrackerPos.Aim];
			var aimPosMatrix = Matrix.TR(aimPos?.Position ?? Vector3f.Zero, aimPos?.Rotation ?? Quaternionf.Identity);
			if (aimPos is null) {
				aimPosMatrix = Matrix.Identity;
			}
			StartingPos = LocalToUser * aimPosMatrix.Inverse;
			if (Laser) {
				grabbableHolder.Target?.UpdateReferencer();
			}
			Entity.CallOnGrabbed(this);
		}
		[Exposed]
		public void RemoteGrab(Handed hand) {
			var grabbableHolder = hand switch {
				Handed.Left => World.LeftGrabbableHolder,
				Handed.Right => World.RightGrabbableHolder,
				_ => World.HeadGrabbableHolder,
			};
			if (grabbableHolder is not null) {
				GripProcess(grabbableHolder, true, 1f);
			}
		}

		internal void UpdateGrabbedObject() {
			if (!LaserGrabbed) {
				return;
			}
			if (grabbableHolder.Target is null) {
				return;
			}
			var pushBackAndForth = InputManager.ObjectPush.HandedValue(GabbedSide) - InputManager.ObjectPull.HandedValue(GabbedSide);
			var rotate = InputManager.RotateRight.HandedValue(GabbedSide) - InputManager.RotateLeft.HandedValue(GabbedSide);
			var aimPos = InputManager.XRInputSystem.GetHand(grabbableHolder.Target.source.Value)?[TrackerPos.Aim];
			var aimPosMatrix = Matrix.TR(aimPos?.Position ?? Vector3f.Zero, aimPos?.Rotation ?? Quaternionf.Identity) * LocalUser.userRoot.Target.Entity.GlobalTrans;
			if (aimPos is null) {
				aimPosMatrix = LocalUser.userRoot.Target.head.Target.GlobalTrans;
			}
			Entity.GlobalTrans = Matrix.RS(Quaternionf.CreateFromEuler((float)(rotate * RTime.Elapsed * 25), 0, 0) * Entity.GlobalTrans.Rotation, Entity.GlobalTrans.Scale) * Matrix.T(Entity.GlobalTrans.Translation);
			var localPos = Entity.GlobalTrans * aimPosMatrix.Inverse;
			localPos.Translation -= new Vector3f(0, 0, pushBackAndForth * RTime.Elapsed);
			if (localPos.Translation.z >= -0.01f) {
				LaserGrabbed = false;
			}
			Entity.GlobalTrans = localPos * aimPosMatrix;
			if (InputManager.Primary.HandedValue(GabbedSide) > 0.5) {
				Entity.RotateToUpVector(LocalUser.userRoot.Target.Entity);
			}
		}
	}
}
