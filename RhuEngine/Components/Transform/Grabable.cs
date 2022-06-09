using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "Interaction" })]
	public class Grabbable : Component
	{
		public readonly SyncRef<Entity> lastParent;

		public readonly Sync<bool> CanNotDestroy;

		public readonly SyncRef<User> grabbingUser;

		public readonly SyncRef<GrabbableHolder> grabbableHolder;

		public bool LaserGrabbed;

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


		private void Entity_onGrip(GrabbableHolder obj, bool Laser) {
			if (grabbableHolder.Target == obj) {
				return;
			}

			if (obj == null) {
				return;
			}

			if (obj.holder.Target == null) {
				return;
			}

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
		}
		[Exsposed]
		public void RemoteGrab(Handed hand) {
			RLog.Info("Remote grab");
			var grabbableHolder = hand switch {
				Handed.Left => World.LeftGrabbableHolder,
				Handed.Right => World.RightGrabbableHolder,
				_ => World.HeadGrabbableHolder,
			};
			if (grabbableHolder is not null) {
				Entity_onGrip(grabbableHolder, true);
			}
		}
	}
}
