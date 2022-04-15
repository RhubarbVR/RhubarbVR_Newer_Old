using System;

using BulletSharp;

using RhuEngine.Physics;

namespace RBullet
{
	public class BulletColliderShape : ILinkedColliderShape
	{
		public RigidBodyCollider GetCollider(ColliderShape obj, PhysicsSim physicsSim) {
			var collider = new RigidBodyCollider {
				CollisionShape = obj,
				PhysicsSim = physicsSim
			};
			new BRigidBodyCollider(collider);
			return collider;
		}

	}
}
