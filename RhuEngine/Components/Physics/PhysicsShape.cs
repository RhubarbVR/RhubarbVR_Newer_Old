using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using BepuPhysics;
using RhuEngine.Physics;
using System.Runtime.InteropServices;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using BepuUtilities;
using System.Numerics;
using static RNumerics.RoundRectGenerator;
using System.Runtime.CompilerServices;
using DiscordRPC;
using System.Reflection.Metadata;

namespace RhuEngine.Components
{
	public abstract class PhysicsShape<T> : PhysicsObject where T : unmanaged, IShape
	{
		[Default(1f)]
		[OnChanged(nameof(UpdateShape))]
		public readonly Sync<float> Mass;

		[OnChanged(nameof(PosUpdate))]
		public readonly Sync<Vector3f> PosOffset;

		[Default(true)]
		[OnChanged(nameof(UpdateShape))]
		public readonly Sync<bool> HasStaticBody;

		protected override void OnLoaded() {
			base.OnLoaded();
			Entity.GlobalTransformChange += Entity_GlobalTransformChange;
			Entity.EnabledChanged += AwakeningFilterUpdate;
			Enabled.Changed += EnabledChanged;
			UpdateShape();
		}

		private void EnabledChanged(IChangeable changeable) {
			AwakeningFilterUpdate();
		}

		public override void Dispose() {
			IsDestroying = true;
			Entity.GlobalTransformChange -= Entity_GlobalTransformChange;
			RemoveBody();
			RemoveShape();
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		private void Entity_GlobalTransformChange(Entity arg1, bool arg2) {
			if (_lastWorldScale != Entity.GlobalTrans.Scale) {
				UpdateShape();
			}
			PosUpdate();
		}

		public TypedIndex ShapeIndex { get; protected set; }
		public BodyInertia BodyInertiaCache { get; protected set; }
		private StaticHandle _staticHandle;

		public abstract void RemoveShape();

		public abstract T CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia);

		public void UpdateShape() {
			RUpdateManager.ExecuteOnEndOfFrame(this, UpdateShapeNow);
		}

		private readonly bool _isChild;//Todo set true when used is sub body

		public float? MassValue => _isChild ? MassAfterScale : null;

		private Vector3f _lastWorldScale;

		private void UpdateShapeNow() {
			if (IsDestroying) {
				return;
			}
			var specultive = SPECULATIVE_MARGIN;
			if (ShapeIndex == default) {
				var shape = CreateShape(ref specultive, MassValue, out var bodyInertiaCache);
				_lastWorldScale = Entity.GlobalTrans.Scale;
				ShapeIndex = Simulation.Simulation.Shapes.Add(in shape);
				BodyInertiaCache = bodyInertiaCache;
				LoadedShape();
			}
			else {
				Simulation.Simulation.Shapes.GetShape<T>(ShapeIndex.Index) = CreateShape(ref specultive, MassValue, out var bodyInertiaCache);
				_lastWorldScale = Entity.GlobalTrans.Scale;
				BodyInertiaCache = bodyInertiaCache;
			}
		}

		protected void PosUpdate() {
			if (_staticHandle != default) {
				var filter = new StaticChangeAwakeningFilter(Simulation, IsEnabled, Group);
				Simulation.Simulation.Statics.GetDescription(_staticHandle, out var description);
				description.Pose = GetRigidPose();
				Simulation.Simulation.Statics.ApplyDescription(_staticHandle, in description, ref filter);
			}
		}
		
		protected override void MaskUpdate() {
			AwakeningFilterUpdate();
		}

		private void AwakeningFilterUpdate() {
			if (_staticHandle != default) {
				var filter = new StaticChangeAwakeningFilter(Simulation, IsEnabled, Group);
				Simulation.Simulation.Statics.GetDescription(_staticHandle, out var description);
				Simulation.Simulation.Statics.ApplyDescription(_staticHandle, in description, ref filter);
			}
		}

		private void UpdatedShape() {
			AwakeningFilterUpdate();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private RigidPose GetRigidPose() {
			var globalTrans = RNumerics.Matrix.T(PosOffset.Value) * Entity.GlobalTrans;
			var rigidPose = new RigidPose(MathUtil.Clamp(globalTrans.Translation.Clean, -1E+08f, 1E+08f), (Quaternion)globalTrans.Rotation.Clean);
			return rigidPose;
		}

		private void LoadedShape() {
			if (HasStaticBody && _staticHandle == default) {
				var rigidPose = GetRigidPose();
				var filter = new StaticChangeAwakeningFilter(Simulation, IsEnabled, Group);
				var description = new StaticDescription(rigidPose.Position, rigidPose.Orientation, ShapeIndex);
				var handle = Simulation.Simulation.Statics.Add(in description, ref filter);
				Simulation.RegisterPhysicsObject(handle, this);
				_staticHandle = handle;
			}
			PosUpdate();
		}

		public void RemoveBody() {
			if(_staticHandle != default) {
				Simulation.UnRegisterPhysicsObject(_staticHandle);
				_staticHandle = default;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplyGlobalScaleValues(ref Vector3f size) {
			size *= Entity.GlobalTrans.Scale;
			size = size.Clean;
			size = MathUtil.Abs(in size);
			size = MathUtil.Clamp(in size, new Vector3f(COMMON_MIN), new Vector3f(COMMON_MAX));
		}

		public float MassAfterScale
		{
			get {
				var scale = Entity.GlobalTrans.Scale;
				var scaleAve = Math.Abs((scale.x + scale.y + scale.z) / 3) * Mass.Value;
				return Math.Clamp((float.IsNaN(scaleAve) || float.IsInfinity(scaleAve)) ? 0 : scaleAve, COMMON_MIN, COMMON_MAX);
			}
		}

	}
}
