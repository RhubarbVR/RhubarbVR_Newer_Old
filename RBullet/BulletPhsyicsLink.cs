using System;
using System.Runtime.InteropServices;

using RhuEngine.Physics;
using BulletSharp;

namespace RBullet
{  
	public class BulletPhsyicsLink: PhysicsHelper.Physics<BulletRigidBodyCollider, BulletPhysicsSim, BulletColliderShape>
    {
		

		public BulletPhsyicsLink() {
			if (!Native.Load()) {
				throw new Exception("Failed to load lib");
			}
		}

	}
}
