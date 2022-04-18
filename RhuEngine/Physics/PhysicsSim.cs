using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Physics
{
	[Flags]
	public enum ECollisionFilterGroups
	{
		AllFilter = -1,
		AllNormal = (int)((uint)DefaultFilter | (uint)StaticFilter | (uint)KinematicFilter | (uint)DebrisFilter | (uint)SensorTrigger | (uint)CharacterFilter | (uint)UI),
		None = 0,
		DefaultFilter = 1,
		StaticFilter = 2,
		KinematicFilter = 4,
		DebrisFilter = 8,
		SensorTrigger = 16,
		CharacterFilter = 32,
		UI = 64,
		Custom1 = 128,
		Custom2 = 256,
		Custom3 = 512,
		Custom4 = 1024,
		Custom5 = 2048,
	}

	public interface ILinkedPhysicsSim
	{
		public object NewSim();
		public void UpdateSim(object obj,float DeltaSeconds);
		public bool ConvexRayTest(object sem,ColliderShape colliderShape, ref Matrix from, ref Matrix to, out RigidBodyCollider rigidBodyCollider, out Vector3f HitNormalWorld, out Vector3f HitPointWorld, ECollisionFilterGroups mask, ECollisionFilterGroups group);

		public bool RayTest(object sem,ref Vector3f rayFromWorld, ref Vector3f rayToWorld, out RigidBodyCollider rigidBodyCollider, out Vector3f HitNormalWorld, out Vector3f HitPointWorld, ECollisionFilterGroups mask, ECollisionFilterGroups group);
	}
	public class PhysicsSim
	{
		public static ILinkedPhysicsSim Manager { get; set; }

		public object obj;

		public void UpdateSim(float DeltaSeconds) {
			Manager?.UpdateSim(obj, DeltaSeconds);
		}
		public bool ConvexRayTest(ColliderShape colliderShaperef,ref Matrix from, ref Matrix to, out RigidBodyCollider rigidBodyCollider, out Vector3f hitNormalWorld, out Vector3f hitPointWorld, ECollisionFilterGroups mask = ECollisionFilterGroups.AllFilter, ECollisionFilterGroups group = ECollisionFilterGroups.AllFilter) {
			return Manager.ConvexRayTest(obj, colliderShaperef, ref from, ref to, out rigidBodyCollider, out hitNormalWorld, out hitPointWorld, mask, group);
		}

		public bool RayTest(ref Vector3f rayFromWorld, ref Vector3f rayToWorld,out RigidBodyCollider rigidBodyCollider,out Vector3f hitNormalWorld,out Vector3f hitPointWorld, ECollisionFilterGroups mask = ECollisionFilterGroups.AllFilter, ECollisionFilterGroups group = ECollisionFilterGroups.AllFilter) {
			return Manager.RayTest(obj, ref rayFromWorld, ref rayToWorld, out rigidBodyCollider,  out hitNormalWorld, out hitPointWorld, mask, group);
		}

		public PhysicsSim() {
			obj = Manager?.NewSim();
		}
	}
}
