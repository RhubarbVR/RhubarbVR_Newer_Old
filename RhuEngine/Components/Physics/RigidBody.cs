using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Physics" })]
	[UpdateLevel(UpdateEnum.Movement)]
	public class RigidBody : Component
	{
		public readonly Linker<Vector3f> Position;

		public readonly Linker<Quaternionf> Rotation;

		public readonly Linker<Vector3f> Scale;

		[OnChanged(nameof(EntityChanged))]
		public readonly SyncRef<Entity> TargetEntity;

		public override void OnAttach() {
			base.OnAttach();
			TargetEntity.Target = Entity;
		}

		private void EntityChanged() {
			if(TargetEntity.Target is null) {
				return;
			}
			if (Scale.Target is null) {
				Scale.Target = TargetEntity.Target.scale;
			}
			if (Rotation.Target is null) {
				Rotation.Target = TargetEntity.Target.rotation;
			}
			if (Position.Target is null) {
				Position.Target = TargetEntity.Target.position;
			}
		}

		[Default(10f)]
		[OnChanged(nameof(MassChanged))]
		public readonly Sync<float> Mass;

		private void MassChanged() {
			var rig = _physicsObject?.rigidBody;
			if(rig is null) {
				return;
			}
			rig.Mass = Mass;
		}

		[OnChanged(nameof(PhysicsObjectChanged))]
		public readonly SyncRef<PhysicsObject> PhysicsObject;

		[NoLoad]
		[NoSave]
		[NoSync]
		private PhysicsObject _physicsObject;

		public override void OnLoaded() {
			base.OnLoaded();
			PhysicsObjectChanged();
		}

		private void PhysicsObjectChanged() {
			if(_physicsObject == PhysicsObject.Target) {
				return;
			}
			if(_physicsObject is not null) {
				_physicsObject.rigidBody.NoneStaticBody = false;
				_physicsObject.rigidBody.Mass = 0f;
				_physicsObject.AddedData -= PhysicsObject_AddedData;
			}
			_physicsObject = PhysicsObject.Target;
			if (_physicsObject is null) {
				return;
			}
			_physicsObject.AddedData += PhysicsObject_AddedData;
			PhysicsObject_AddedData(_physicsObject.rigidBody);
		}

		private void PhysicsObject_AddedData(RigidBodyCollider obj) {
			if(obj is null) {
				return;
			}
			obj.NoneStaticBody = true;
			obj.Mass = Mass;
			obj.Active = true;
		}

		public override void Step() {
			base.Step();
			var colider = PhysicsObject.Target?.rigidBody;
			if (colider is null) {
				return;
			}
			var entity = TargetEntity.Target;
			if (entity is null) {
				return;
			}
			entity.SetGlobalMatrixPysics(colider.Matrix);
		}
	}
}
