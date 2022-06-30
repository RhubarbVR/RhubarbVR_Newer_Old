using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using RhuEngine.Physics;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "Interaction" })]
	public class GrabbableHolder : Component
	{
		public readonly SyncRef<Entity> holder;

		public readonly SyncRef<IWorldObject> Referencer;

		public readonly SyncRef<Entity> RefrencerEntity;

		public readonly SyncRef<User> user;

		public readonly Sync<Handed> source;

		public readonly List<Grabbable> GrabbedObjects = new();

		[OnChanged(nameof(PhysicsObjectChanged))]
		public readonly SyncRef<PhysicsObject> OverlapingPhysicsObject;

		[NoLoad]
		[NoSave]
		[NoSync]
		private PhysicsObject _physicsObject;

		public override void OnLoaded() {
			base.OnLoaded();
			PhysicsObjectChanged();
		}

		private void PhysicsObjectChanged() {
			if (_physicsObject == OverlapingPhysicsObject.Target) {
				return;
			}
			if (_physicsObject is not null) {
				_physicsObject.rigidBody.Overlap -= RigidBody_Overlap; 
				_physicsObject.AddedData -= PhysicsObject_AddedData;
			}
			_physicsObject = OverlapingPhysicsObject.Target;
			if (_physicsObject is null) {
				return;
			}
			_physicsObject.AddedData += PhysicsObject_AddedData;
			PhysicsObject_AddedData(_physicsObject.rigidBody);
		}

		private readonly List<RigidBodyCollider> _overLappingObjects = new();

		private void RigidBody_Overlap(Vector3f PositionWorldOnA, Vector3f PositionWorldOnB, Vector3f NormalWorldOnB, double Distance, double Distance1, RigidBodyCollider hit) {
			_overLappingObjects.Add(hit);
		}

		private void PhysicsObject_AddedData(RigidBodyCollider obj) {
			if (obj is null) {
				return;
			}
			obj.Overlap += RigidBody_Overlap;
		}

		public override void OnAttach() {
			base.OnAttach();
			holder.Target = Entity.AddChild("Holder");
		}

		public void DeleteGrabObjects() {
			foreach (var item in GrabbedObjects) {
				item.DestroyGrabbedObject();
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
				if ((World.HeadGrabbableHolder.Referencer.Target != this) && World.HeadGrabbableHolder.Referencer.Target is not null) {
					if (World.HeadGrabbableHolder.Referencer.Target is not null) {
						return World.HeadGrabbableHolder.Referencer.Target;
					}
				}
				return Referencer.Target;
			}
		}

		public void InitializeGrabHolder(Handed _source) {
			user.Target = LocalUser;
			source.Value = _source;
			if(_source != Handed.Max) {
				var shape = Entity.AttachComponent<SphereShape>();
				shape.Radus.Value = 0.03f / 2;
				OverlapingPhysicsObject.Target = shape;
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

		bool _gripping = false;

		public override void Step() {
			base.Step();
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (holder.Target == null) {
				return;
			}
			if (user.Target != LocalUser) {
				return;
			}
			if(source.Value == Handed.Max) {
				if (RWorld.IsInVR) {
					return;
				}
			}
			var grabForce = source.Value switch {
				Handed.Left => Engine.inputManager.GetInputFloatFromController(Managers.InputManager.InputTypes.Grab, RInput.Controller(Handed.Left), Engine.MainSettings.InputSettings.SecondaryControllerInputSettings),
				Handed.Right => Engine.inputManager.GetInputFloatFromController(Managers.InputManager.InputTypes.Grab, RInput.Controller(Handed.Right), Engine.MainSettings.InputSettings.MainControllerInputSettings),
				Handed.Max => Engine.inputManager.GetInputFloatFromKeyboard(Managers.InputManager.InputTypes.Grab),
				_ => 0f,
			};
			var isGrab = grabForce > 0.5;
			if (isGrab && !_gripping) {
				foreach (var item in _overLappingObjects) {
					if (item.CustomObject is PhysicsObject physicsObject) {
						physicsObject.Entity.CallOnGrip(this, false, grabForce);
					}
				}
				//StartGabbing
				RWorld.ExecuteOnStartOfFrame(() => {
					switch (source.Value) {
						case Handed.Left:
							break;
						case Handed.Right:
							break;
						case Handed.Max:
							WorldManager.PrivateSpaceManager.DisableHeadLaser = true;
							break;
						default:
							break;
					}
				});
				_gripping = true;
			}
			if (_gripping && !isGrab) {
				//DoneGrabbing
				RWorld.ExecuteOnStartOfFrame(() => {
					switch (source.Value) {
						case Handed.Left:
							break;
						case Handed.Right:
							break;
						case Handed.Max:
							WorldManager.PrivateSpaceManager.DisableHeadLaser = false;
							break;
						default:
							break;
					}
				});
				for (var i = GrabbedObjects.Count - 1; i >= 0; i--) {
					try {
						GrabbedObjects[i]?.Drop();
					}
					catch { }
				}
				_gripping = false;
			}
			_overLappingObjects.Clear();
		}
	}
}
