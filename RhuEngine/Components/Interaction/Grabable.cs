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
	public sealed partial class Grabbable : Component
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
				grabbableHolder.Target?.DropObject(this);
			}
			catch { }
			grabbableHolder.Target = null;
			Entity.CallOnDroped();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			Entity.OnGrip += GripProcess;
		}

		public override void Dispose() {
			Entity.OnGrip -= GripProcess;
			base.Dispose();
		}

		private float _lazerDist;
		private Matrix _offsetFromLazer;

		public static bool HITGRABABLE;

		internal void GripProcess(GrabbableHolder obj, bool Laser, float gripForce) {
			HITGRABABLE = true;
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
			if (grabbableHolder.Target is not null) {
				if (Laser && PrivateSpaceManager.GetLazer(grabbableHolder.Target.source.Value).Locked.Value) {
					return;
				}
			}
			LaserGrabbed = Laser;
			if (!Grabbed) {
				lastParent.Target = Entity.parent.Target;
			}
			grabbableHolder.Target = obj;
			grabbingUser.Target = LocalUser;
			if (Laser) {
				if (grabbableHolder.Target is not null) {
					var hitPoint = PrivateSpaceManager.LazerHitPoint(grabbableHolder.Target.source.Value);
					_lazerDist = PrivateSpaceManager.LazerStartPos(grabbableHolder.Target.source.Value).Distance(hitPoint);
					_offsetFromLazer = Entity.GlobalTrans * Matrix.TR(hitPoint, Quaternionf.LookAt(hitPoint, PrivateSpaceManager.LazerStartPos(grabbableHolder.Target.source.Value))).Inverse;
					grabbableHolder.Target?.AddLazerEntity(Entity);
				}
			}
			else {
				Entity.SetParent(obj.holder.Target);
			}
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
				if (grabbableHolder.Target is not null) {
					PrivateSpaceManager.GetLazer(grabbableHolder.Target.source.Value).Locked.Value = true;
				}
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
			if (!(Entity.position.IsLinkedTo & Entity.scale.IsLinkedTo & Entity.rotation.IsLinkedTo)) {
				return;
			}
			var lastDis = _lazerDist;
			_lazerDist = ((InputManager.ObjectPush.HandedValue(GabbedSide) - InputManager.ObjectPull.HandedValue(GabbedSide)) * RTime.ElapsedF) + _lazerDist;
			var rotate = (InputManager.RotateRight.HandedValue(GabbedSide) - InputManager.RotateLeft.HandedValue(GabbedSide)) * RTime.ElapsedF * 50;
			_offsetFromLazer *= Matrix.R(Quaternionf.CreateFromEuler(rotate, 0, 0));
			if (_lazerDist <= -0.1) {
				if(grabbableHolder.Target.source.Value == Handed.Max) {
					_lazerDist = lastDis;
					return;
				}
				LaserGrabbed = false;
				Entity.SetParent(grabbableHolder.Target.holder.Target);
				PrivateSpaceManager.GetLazer(grabbableHolder.Target.source.Value).Locked.Value = false;
				return;
			}
			var netPos = (PrivateSpaceManager.LazerNormal(grabbableHolder.Target.source.Value) * _lazerDist) + PrivateSpaceManager.LazerStartPos(grabbableHolder.Target.source.Value);
			var targetPos = Matrix.TR(netPos, Quaternionf.LookAt(netPos, PrivateSpaceManager.LazerStartPos(grabbableHolder.Target.source.Value)));
			Entity.GlobalTrans = _offsetFromLazer * targetPos;
			PrivateSpaceManager.SetLazerHitPoint(grabbableHolder.Target.source.Value, targetPos.Translation);
			if (InputManager.Primary.HandedValue(GabbedSide) > 0.5) {
				Entity.RotateToUpVector(LocalUser.userRoot.Target.Entity);
				_offsetFromLazer = Entity.GlobalTrans * targetPos.Inverse;

			}
		}
	}
}
