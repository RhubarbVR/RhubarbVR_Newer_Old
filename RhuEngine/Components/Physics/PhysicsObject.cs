using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;

namespace RhuEngine.Components
{
	public abstract class PhysicsObject : Component
	{
		public ColliderShape collider;

		public RigidBodyCollider rigidBody;

		[OnChanged(nameof(RebuildPysics))]
		[Default(ECollisionFilterGroups.AllNormal)]
		public readonly Sync<ECollisionFilterGroups> Mask;

		[OnChanged(nameof(RebuildPysics))]
		[Default(ECollisionFilterGroups.AllNormal)]
		public readonly Sync<ECollisionFilterGroups> Group;

		public abstract ColliderShape PysicsBuild();
		public void RebuildPysics() {
			RWorld.ExecuteOnEndOfFrame(this, () => {
				var shape = PysicsBuild();
				if (shape is not null) {
					BuildPhysics(shape);
				}
			});
		}

		public event Action<RigidBodyCollider> AddedData;

		public void BuildPhysics(ColliderShape colliderShape) {
			rigidBody?.Remove();
			rigidBody = null;
			collider = colliderShape;
			rigidBody = collider.GetCollider(World.PhysicsSim, this);
			rigidBody.Mask = Mask.Value;
			rigidBody.Group = Group.Value;
			rigidBody.Matrix = Entity.GlobalTrans;
			rigidBody.Active = Entity.enabled;
			AddedData?.Invoke(rigidBody);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			RebuildPysics();
			Entity.GlobalTransformChange += Entity_GlobalTransformChange;
			Entity.enabled.Changed += Enabled_Changed;
		}

		private void Enabled_Changed(IChangeable obj) {
			rigidBody.Active = Entity.enabled;
		}

		public override void Dispose() {
			base.Dispose();
			Entity.GlobalTransformChange -= Entity_GlobalTransformChange;
			Entity.enabled.Changed -= Enabled_Changed;
		}
		private void Entity_GlobalTransformChange(Entity obj,bool isStandaredMove) {
			if (!isStandaredMove) {
				return;
			}
			if (rigidBody is null) {
				return;
			}
			rigidBody.Matrix = obj.GlobalTrans;
		}

		public void Touch(uint handed, Vector3f hitnormal, Vector3f hitpointworld) {
			Entity.CallOnTouch(handed, hitnormal, hitpointworld);
		}

		public void Lazer(uint v, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForce) {
			Entity.CallOnLazer(v,hitnormal, hitpointworld, pressForce, gripForce);
		}
	}
}
