using System;

using BulletSharp;

using RhuEngine.Physics;

using RNumerics;

namespace RBullet
{
	public class BRigidBodyCollider
	{
		public float Mass = 0f;

		public Matrix Matrix;

		public bool Enabled = true;
		public bool NoneStaticBody = true;

		public ECollisionFilterGroups Group = ECollisionFilterGroups.AllFilter;
		public ECollisionFilterGroups Mask = ECollisionFilterGroups.AllFilter;

		public RigidBodyCollider Collider { get; set; }

		public BRigidBodyCollider(RigidBodyCollider collider) {
			Collider = collider;
			Collider.obj = this;
			ReloadCollission();
		}

		public void StartShape() {
			BuildCollissionObject(LocalCreateRigidBody(Mass, Matrix, (CollisionShape)Collider.CollisionShape.obj));
		}

		public static Matrix CastMet(BulletSharp.Math.Matrix matrix4X4) {
			var t = new Matrix(
				(float)matrix4X4.M11, (float)matrix4X4.M12, (float)matrix4X4.M13, (float)matrix4X4.M14,
				(float)matrix4X4.M21, (float)matrix4X4.M22, (float)matrix4X4.M23, (float)matrix4X4.M24,
				(float)matrix4X4.M31, (float)matrix4X4.M32, (float)matrix4X4.M33, (float)matrix4X4.M34,
				(float)matrix4X4.M41, (float)matrix4X4.M42, (float)matrix4X4.M43, (float)matrix4X4.M44);
			return t;
		}
		public static BulletSharp.Math.Matrix CastMet(Matrix matrix4X4) {
			var t = new BulletSharp.Math.Matrix(
				matrix4X4.m.M11, matrix4X4.m.M12, matrix4X4.m.M13, matrix4X4.m.M14,
				matrix4X4.m.M21, matrix4X4.m.M22, matrix4X4.m.M23, matrix4X4.m.M24,
				matrix4X4.m.M31, matrix4X4.m.M32, matrix4X4.m.M33, matrix4X4.m.M34,
				matrix4X4.m.M41, matrix4X4.m.M42, matrix4X4.m.M43, matrix4X4.m.M44);
			return t;
		}
		public static RigidBody LocalCreateRigidBody(float mass, Matrix startTransform, CollisionShape shape) {
			//rigidbody is dynamic if and only if mass is non zero, otherwise static
			var isDynamic = mass != 0.0f;
			var localInertia = isDynamic ? shape.CalculateLocalInertia(mass) : BulletSharp.Math.Vector3.Zero;
			var rbInfo = new RigidBodyConstructionInfo(mass, null, shape, localInertia);
			var body = new RigidBody(rbInfo) {
				ContactProcessingThreshold = 0.0f,
				WorldTransform = CastMet(startTransform)
			};
			return body;
		}

		public RigidBody collisionObject;

		private bool _added = false;

		public void ReloadCollission() {
			if (collisionObject is null) {
				StartShape();
			}
			else {
				BuildCollissionObject(collisionObject);
			}
		}

		public void BuildCollissionObject(RigidBody newCol) {
			if (collisionObject != null) {
				if (_added) {
					_added = false;
					((BPhysicsSim)Collider.PhysicsSim.obj)._physicsWorld.RemoveCollisionObject(collisionObject);
					((BPhysicsSim)Collider.PhysicsSim.obj)._physicsWorld.RemoveRigidBody(collisionObject);
				}
				collisionObject = null;
			}
			if (newCol != null) {
				newCol.UserObject = this;
				if (Enabled) {
					_added = true;
					if (NoneStaticBody) {
						((BPhysicsSim)Collider.PhysicsSim.obj)._physicsWorld.AddRigidBody(newCol, (int)Group, (int)Group);
					}
					else {
						((BPhysicsSim)Collider.PhysicsSim.obj)._physicsWorld.AddCollisionObject(newCol, (int)Group, (int)Group);
					}
				}
			}
			else {
				_added = false;
			}
			collisionObject = newCol;
		}

		private Matrix _matrix;

		public void ReloadTrans() {
			if (collisionObject != null) {
				if (Matrix != _matrix) {
					_matrix = Matrix; 
					collisionObject.WorldTransform = CastMet(Matrix);
				}
			}
		}

		event RigidBodyCollider.OverlapCallback Action;

		public void AddOverlapCallBack(RigidBodyCollider.OverlapCallback action) {
			Action += action;
			if(Action != null) {
				((BPhysicsSim)Collider.PhysicsSim.obj).AddPhysicsCallBack(this);
			}
			else {
				((BPhysicsSim)Collider.PhysicsSim.obj).RemovePhysicsCallBack(this);
			}
		}
		public void RemoveOverlapCallBack(RigidBodyCollider.OverlapCallback action) {
			Action -= action;
			if (Action != null) {
				((BPhysicsSim)Collider.PhysicsSim.obj).AddPhysicsCallBack(this);
			}
			else {
				((BPhysicsSim)Collider.PhysicsSim.obj).RemovePhysicsCallBack(this);
			}
		}
		public void Call(Vector3f PositionWorldOnA, Vector3f PositionWorldOnB, Vector3f NormalWorldOnB, double Distance, double Distance1, RigidBodyCollider hit) {
			Action?.Invoke(PositionWorldOnA, PositionWorldOnB, NormalWorldOnB, Distance, Distance1, hit);
		}

		public void Remove() {
			BuildCollissionObject(null);
		}
	}
	public class BulletRigidBodyCollider : ILinkedRigidBodyCollider
	{
		public bool ActiveGet(object obj) {
			return ((BRigidBodyCollider)obj).Enabled;
		}

		public void ActiveSet(object obj, bool val) {
			((BRigidBodyCollider)obj).Enabled = val;
			((BRigidBodyCollider)obj).ReloadCollission();
		}


		public ECollisionFilterGroups GroupGet(object obj) {
			return ((BRigidBodyCollider)obj).Group;
		}

		public void GroupSet(object obj, ECollisionFilterGroups val) {
			((BRigidBodyCollider)obj).Group = val;
			((BRigidBodyCollider)obj).ReloadCollission();
		}

		public ECollisionFilterGroups MaskGet(object obj) {
			return ((BRigidBodyCollider)obj).Mask;
		}

		public void MaskSet(object obj, ECollisionFilterGroups val) {
			((BRigidBodyCollider)obj).Mask = val;
			((BRigidBodyCollider)obj).ReloadCollission();
		}

		public float MassGet(object obj) {
			return ((BRigidBodyCollider)obj).Mass;
		}

		public void MassSet(object obj, float val) {
			((BRigidBodyCollider)obj).Mass = val;
			((BRigidBodyCollider)obj).ReloadCollission();
		}

		public Matrix MatrixGet(object obj) {
			return ((BRigidBodyCollider)obj).Matrix;
		}

		public void MatrixSet(object obj, Matrix val) {
			((BRigidBodyCollider)obj).Matrix = val;
			((BRigidBodyCollider)obj).ReloadTrans();
		}

		public bool NoneStaticBodyGet(object obj) {
			return ((BRigidBodyCollider)obj).NoneStaticBody;
		}

		public void NoneStaticBodySet(object obj, bool val) {
			((BRigidBodyCollider)obj).NoneStaticBody = val;
			((BRigidBodyCollider)obj).ReloadCollission();
		}

		public void Remove(object obj) {
			((BRigidBodyCollider)obj).Remove();
		}

		public void AddOverlapCallback(object obj, RigidBodyCollider.OverlapCallback overlapCallback) {
			((BRigidBodyCollider)obj).AddOverlapCallBack(overlapCallback);       
		}

		public void RemoveOverlapCallback(object obj, RigidBodyCollider.OverlapCallback overlapCallback) {
			((BRigidBodyCollider)obj).RemoveOverlapCallBack(overlapCallback);
		}
	}
}
