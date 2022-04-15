using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Physics
{
	public static class PhysicsHelper
	{
		public interface IPhysics {
			void RegisterPhysics();
		}
		public abstract class Physics<LinkedRigidBodyCollider, LinkedPhysicsSim, LinkedColliderShape> : IPhysics where LinkedRigidBodyCollider : ILinkedRigidBodyCollider,new() where LinkedPhysicsSim : ILinkedPhysicsSim, new() where LinkedColliderShape : ILinkedColliderShape, new()
		{
			public void RegisterPhysics() {
				RigidBodyCollider.Manager = new LinkedRigidBodyCollider();
				PhysicsSim.Manager = new LinkedPhysicsSim();
				ColliderShape.Manager = new LinkedColliderShape();
			}
		}
		public static void RegisterPhysics<T>() where T: IPhysics,new() {
			new T().RegisterPhysics();
		}
	}
}
