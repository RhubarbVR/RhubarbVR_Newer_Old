using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using RhuEngine.Physics;
using System;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "Interaction" })]
	public sealed class GrabbableHolder : Component
	{
		public readonly SyncRef<Entity> holder;

		[OnChanged(nameof(UpdateReferencer))]
		public readonly SyncRef<IWorldObject> Referencer;

		public readonly SyncRef<Entity> RefrencerEntity;

		public readonly SyncRef<User> user;

		public readonly Sync<Handed> source;

		public readonly List<Grabbable> GrabbedObjects = new();

		private readonly List<RigidBodyCollider> _overLappingObjects = new();

		internal void UpdateReferencer() {
			try {
				PrivateSpaceManager.GetGrabbableHolder(source.Value).UpdateHolderReferen();
			}
			catch { }
		}

		private void RigidBody_Overlap(Vector3f PositionWorldOnA, Vector3f PositionWorldOnB, Vector3f NormalWorldOnB, double Distance, double Distance1, RigidBodyCollider hit) {
			_overLappingObjects.Add(hit);

		}

		private void PhysicsObject_AddedData(RigidBodyCollider obj) {
			if (obj is null) {
				return;
			}
			obj.Overlap += RigidBody_Overlap;
		}

		protected override void OnAttach() {
			base.OnAttach();
			holder.Target = Entity.AddChild("Holder");
		}

		public void DeleteGrabObjects() {
			foreach (var item in GrabbedObjects) {
				item.DestroyGrabbedObject();
			}
		}
		public bool IsAnyLaserGrabbed
		{
			get {
				if (GrabbedObjects.Count <= 0) {
					return false;
				}
				foreach (var item in GrabbedObjects) {
					if (item.LaserGrabbed) {
						return true;
					}
				}
				return false;
			}
		}
		public bool CanDestroyAnyGabbed
		{
			get {
				if (GrabbedObjects.Count <= 0) {
					return false;
				}
				foreach (var item in GrabbedObjects) {
					if (!item.CanNotDestroy.Value) {
						return true;
					}
				}
				return false;
			}
		}
		public IWorldObject HolderReferen
		{
			get {
				return World.HeadGrabbableHolder != this && World.HeadGrabbableHolder.Referencer.Target is not null
					? World.HeadGrabbableHolder.Referencer.Target
					: Referencer.Target;
			}
		}

		public void InitializeGrabHolder(Handed _source) {
			user.Target = LocalUser;
			source.Value = _source;
			if (_source != Handed.Max) {
				var shape = Entity.AttachComponent<SphereShape>();
				shape.Radus.Value = 0.05f / 2;
				shape.AddedData += PhysicsObject_AddedData;
				PhysicsObject_AddedData(shape.rigidBody);
			}
			switch (_source) {
				case Handed.Left:
					World.LeftGrabbableHolder = this;
					break;
				case Handed.Right:
					World.RightGrabbableHolder = this;
					break;
				case Handed.Max:
					World.HeadGrabbableHolder = this;
					break;
				default:
					break;
			}
		}

		public bool gripping = false;
		public bool grippingLastFrame = false;

		protected override void RenderStep() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (holder.Target == null) {
				return;
			}
			if (user.Target != LocalUser) {
				return;
			}
			if (source.Value == Handed.Max) {
				if (Engine.IsInVR) {
					return;
				}
			}
			if (source.Value == Handed.Left && World.LeftGrabbableHolder != this) {
				return;
			}
			else if (source.Value == Handed.Right && World.RightGrabbableHolder != this) {
				return;
			}
			else if (source.Value == Handed.Max && World.HeadGrabbableHolder != this) {
				return;
			}
			else if (source.Value > Handed.Max) {
				return;
			}

			var grabForce = InputManager.Grab.HandedValue(source.Value);
			var isGrab = grabForce > 0.6;
			grippingLastFrame = gripping;
			if (isGrab && !gripping) {
				foreach (var item in _overLappingObjects) {
					if (item.CustomObject is PhysicsObject physicsObject) {
						physicsObject.Entity.CallOnGrip(this, false, grabForce);
						if (physicsObject.Entity == Entity) {
							RLog.Info("Grabing self");
						}
						RLog.Info("Grip " + physicsObject.Entity.name.Value);
					}
				}
				gripping = true;
				UpdateReferencer();
				if (World.IsPersonalSpace) {
					PrivateSpaceManager.HolderGrip();
				}
			}
			if (isGrab && gripping) {
				foreach (var item in GrabbedObjects) {
					item.UpdateGrabbedObject();
				}
			}
			if (gripping && !isGrab) {
				if (World.IsPersonalSpace) {
					PrivateSpaceManager.HolderDrop();
				}
				//DoneGrabbing
				for (var i = GrabbedObjects.Count - 1; i >= 0; i--) {
					try {
						GrabbedObjects[i]?.Drop();
					}
					catch { }
				}
				gripping = false;
				PrivateSpaceManager.GetLazer(source.Value).Locked.Value = false;
				UpdateReferencer();
			}
			_overLappingObjects.Clear();
		}
	}
}
