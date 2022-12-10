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
		private ColliderShape _collider;

		public RigidBodyCollider rigidBody;

		[OnChanged(nameof(RebuildPysics))]
		[Default(ECollisionFilterGroups.StaticFilter)]
		public readonly Sync<ECollisionFilterGroups> Mask;

		[OnChanged(nameof(RebuildPysics))]
		[Default(ECollisionFilterGroups.StaticFilter)]
		public readonly Sync<ECollisionFilterGroups> Group;

		[Default(RCursorShape.Arrow)]
		public readonly Sync<RCursorShape> CursorShape;

		[OnChanged(nameof(RebuildPysics))]
		public readonly Sync<Vector3f> Pos;
		[OnChanged(nameof(RebuildPysics))]
		public readonly Sync<Vector3f> Scale;
		[OnChanged(nameof(RebuildPysics))]
		public readonly Sync<Quaternionf> Rotation;

		protected override void OnAttach() {
			base.OnAttach();
			Scale.Value = Vector3f.One;
			Rotation.Value = Quaternionf.Identity;
			if (Entity.GetFirstComponent<Grabbable>() is not null) {
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
			rigidBody?.Dispose();
			rigidBody = null;
			if (_collider is not null) {
				_collider?.Dispose();
				_collider = null;
			}
			if (Pos.Value == Vector3f.Zero && Rotation.Value == Quaternionf.Identity && Scale.Value == Vector3f.One) {
				_collider = colliderShape;
			}
			else {
				var mainShape = new RCompoundShape();
				mainShape.AddShape(colliderShape, Matrix.TRS(Pos.Value, Rotation.Value, Scale.Value));
				_collider = mainShape;
			}
			rigidBody = _collider.GetCollider(World.PhysicsSim, this);
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
			Enabled.Changed += Enabled_Changed;
			RebuildPysics();
		}

		private void Enabled_Changed(IChangeable obj) {
			if(rigidBody is null) {
				return;
			}
			rigidBody.Enabled = Entity.enabled && Enabled.Value;
		}

		public override void Dispose() {
			if (_collider is not null) {
				_collider?.Dispose();
			}
			_collider = null;
			if (rigidBody is not null) {
				rigidBody?.Dispose();
			}
			rigidBody = null;
			Entity.GlobalTransformChange -= Entity_GlobalTransformChange;
			Entity.enabled.Changed -= Enabled_Changed;
			Enabled.Changed -= Enabled_Changed;
			base.Dispose();
			GC.SuppressFinalize(this);
		}
		private void Entity_GlobalTransformChange(Entity obj, bool isStandaredMove) {
			if (!isStandaredMove) {
				return;
			}
			if (rigidBody is null) {
				return;
			}
			rigidBody.Matrix = Entity.GlobalTrans;
		}

		public ulong LastFrameTouch = 0;
		public bool TouchThisFrame => RUpdateManager.UpdateCount <= LastFrameTouch;

		public uint TouchHanded { get; private set; }
		public Vector3f TouchHitnormal { get; private set; }
		public Vector3f TouchHitpointworld { get; private set; }
		public Handed TouchHandedSide { get; private set; }

		public event Action<uint, Vector3f, Vector3f, Handed> OnTouchPyhsics;

		public void Touch(uint handed, Vector3f hitnormal, Vector3f hitpointworld, Handed handedSide) {
			OnTouchPyhsics?.Invoke(handed, hitnormal, hitpointworld, handedSide);
			Entity.CallOnTouch(handed, hitnormal, hitpointworld, handedSide);
			RUpdateManager.ExecuteOnStartOfFrame(() => {
				LastFrameTouch = RUpdateManager.UpdateCount;
				TouchHanded = handed;
				TouchHitnormal = hitnormal;
				TouchHitpointworld = hitpointworld;
				TouchHandedSide = handedSide;
			});
		}

		public uint LazerV { get; private set; }
		public Vector3f LazerHitnormal { get; private set; }
		public Vector3f LazerHitpointworld { get; private set; }
		public float LazerPressForce { get; private set; }
		public float LazerGripForce { get; private set; }
		public Handed LazerHand { get; private set; }

		public ulong LastFrameLazer = 0;
		public bool LazeredThisFrame => RUpdateManager.UpdateCount <= LastFrameLazer;

		public event Action<uint, Vector3f, Vector3f, float, float, Handed> OnLazerPyhsics;

		public void Lazer(uint v, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForce, Handed hand) {
			OnLazerPyhsics?.Invoke(v, hitnormal, hitpointworld, pressForce, gripForce, hand);
			Entity.CallOnLazer(v, hitnormal, hitpointworld, pressForce, gripForce, hand);
			RUpdateManager.ExecuteOnStartOfFrame(() => {
				LastFrameLazer = RUpdateManager.UpdateCount;
				LazerV = v;
				LazerHitnormal = hitnormal;
				LazerHitpointworld = hitpointworld;
				LazerPressForce = pressForce;
				LazerGripForce = gripForce;
				LazerHand = hand;
			});
		}
	}
}
