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

namespace RhuEngine.Components
{
	public abstract class PhysicsShape<T> : PhysicsObject where T : unmanaged, IShape
	{
		[Default(1f)]
		public readonly Sync<float> Mass;

		public readonly Sync<Vector3f> PosOffset;

		protected override void OnLoaded() {
			base.OnLoaded();
			Entity.GlobalTransformChange += Entity_GlobalTransformChange;
			UpdateShape();
		}

		public override void Dispose() {
			IsDestroying = true;
			Entity.GlobalTransformChange -= Entity_GlobalTransformChange;
			RemoveShape();
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		private void Entity_GlobalTransformChange(Entity arg1, bool arg2) {
			if(_lastWorldScale != Entity.GlobalTrans.Scale) {
				UpdateShape();
			}
		}

		public TypedIndex ShapeIndex { get; protected set; }
		public BodyInertia BodyInertiaCache { get; protected set; }
		public abstract void RemoveShape();

		public abstract T CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia);

		public void UpdateShape() {
			RUpdateManager.ExecuteOnStartOfFrame(this, UpdateShapeNow);
		}

		private readonly bool _isChild;//Todo set true when used is sub body

		public float? MassValue => _isChild ? Mass.Value : null;

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
			}
			else {
				Simulation.Simulation.Shapes.GetShape<T>(ShapeIndex.Index) = CreateShape(ref specultive, MassValue, out var bodyInertiaCache);
				_lastWorldScale = Entity.GlobalTrans.Scale;
				BodyInertiaCache = bodyInertiaCache;
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
