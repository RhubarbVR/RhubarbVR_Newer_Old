using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Interaction" })]
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
		public Vector3f StartingPos;

		public bool Grabbed => (grabbableHolder.Target is not null) && (grabbingUser.Target is not null);


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
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			Entity.OnLazerPyhsics += Entity_OnLazerPyhsics;
			Entity.OnGrip += GripProcess;
		}

		public override void Dispose() {
			base.Dispose();
			Entity.OnLazerPyhsics -= Entity_OnLazerPyhsics;
			Entity.OnGrip -= GripProcess;
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
			StartingPos = Entity.position.Value;
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
			Entity.rotation.Value = Quaternionf.CreateFromEuler(rotate * RTime.Elapsedf * 50, 0, 0) * Entity.rotation.Value;
			Entity.position.Value += StartingPos * pushBackAndForth * RTime.Elapsedf * 3;
			var xAreSame = (StartingPos.x > 0) == (Entity.position.Value.x > 0);
			var yAreSame = (StartingPos.y > 0) == (Entity.position.Value.y > 0);
			var zAreSame = (StartingPos.z > 0) == (Entity.position.Value.z > 0);
			if (!(xAreSame && yAreSame && zAreSame)) {
				LaserGrabbed = false;
			}
		}
	}
}
