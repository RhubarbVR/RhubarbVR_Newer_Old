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

		[Default(0.5f)]
		public readonly Sync<float> GripForce;

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

		public override void OnLoaded() {
			base.OnLoaded();
			Entity.OnLazerPyhsics += Entity_OnLazerPyhsics;
			Entity.OnGrip += GripProcess;
		}

		public override void Dispose() {
			base.Dispose();
			Entity.OnLazerPyhsics -= Entity_OnLazerPyhsics;
			Entity.OnGrip -= GripProcess;
		}

		private void Entity_OnLazerPyhsics(uint arg1, Vector3f arg2, Vector3f arg3, float arg4, float arg5) {
			if(arg1 == 10) {
				GripProcess(World.HeadGrabbableHolder, true, arg5);
			}
		}

		internal void GripProcess(GrabbableHolder obj, bool Laser,float gripForce) {
			if (gripForce < GripForce.Value) {
				return;
			}
			if (grabbableHolder.Target == obj) {
				return;
			}
			if (Laser) {
				switch (obj.source.Value) {
					case Handed.Left:
						WorldManager.PrivateSpaceManager.DisableLeftLaser = true;
						break;
					case Handed.Right:
						WorldManager.PrivateSpaceManager.DisableRightLaser = true;
						break;
					case Handed.Max:
						WorldManager.PrivateSpaceManager.DisableHeadLaser = true;
						break;
					default:
						break;
				}
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
		[Exposed]
		public void RemoteGrab(Handed hand) {
			var grabbableHolder = hand switch {
				Handed.Left => World.LeftGrabbableHolder,
				Handed.Right => World.RightGrabbableHolder,
				_ => World.HeadGrabbableHolder,
			};
			if (grabbableHolder is not null) {
				GripProcess(grabbableHolder, true,1f);
			}
		}
	}
}
