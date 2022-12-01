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

		[Default(RCursorShape.Arrow)]
		public readonly Sync<RCursorShape> CursorShape;

		protected override void OnAttach() {
			base.OnAttach();
			if(Entity.GetFirstComponent<Grabbable>() is not null) {
				CursorShape.Value = RCursorShape.Move;
			}
		}

		public abstract ColliderShape PysicsBuild();
		public void RebuildPysics() {
			RUpdateManager.ExecuteOnEndOfFrame(this, () => {
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
			rigidBody.Enabled = Entity.enabled;
			AddedData?.Invoke(rigidBody);
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			Entity.GlobalTransformChange += Entity_GlobalTransformChange;
			Entity.enabled.Changed += Enabled_Changed;
			RebuildPysics();
		}

		private void Enabled_Changed(IChangeable obj) {
			rigidBody.Enabled = Entity.enabled;
		}

		public override void Dispose() {
			Entity.GlobalTransformChange -= Entity_GlobalTransformChange;
			Entity.enabled.Changed -= Enabled_Changed;
			base.Dispose();
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

		public void Touch(uint handed, Vector3f hitnormal, Vector3f hitpointworld,Handed handedSide) {
			Entity.CallOnTouch(handed, hitnormal, hitpointworld, handedSide);
		}

		public void Lazer(uint v, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForce,Handed hand) {
			Entity.CallOnLazer(v,hitnormal, hitpointworld, pressForce, gripForce,hand);
		}
	}
}
