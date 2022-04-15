using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Physics
{
	public interface ILinkedColliderShape
	{
		public RigidBodyCollider GetCollider(ColliderShape obj, PhysicsSim physicsSim);
	}

	public abstract class ColliderShape
	{
		public static ILinkedColliderShape Manager { get; set; }

		public object obj;

		public RigidBodyCollider GetCollider(PhysicsSim physicsSim) {
			var col =  Manager?.GetCollider(this, physicsSim);
			return col;
		}
	}
}
