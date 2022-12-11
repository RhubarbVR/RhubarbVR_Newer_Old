﻿using RhuEngine.WorldObjects;
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

namespace RhuEngine.Components
{
	public abstract class PhysicsShape<T> : PhysicsObject where T : unmanaged, IShape
	{
		[Default(1f)]
		public readonly Sync<float> Mass;

		public readonly Sync<Vector3f> PosOffset;

		public TypedIndex shapeIndex;

		public abstract void RemoveShape();

		public abstract T CreateShape(ref float speculativeMargin, float? mass, out BodyInertia inertia);

		public void UpdateShape() {
			RUpdateManager.ExecuteOnStartOfFrame(this, UpdateShapeNow);
		}

		private void UpdateShapeNow() {
			var specultive = SPECULATIVE_MARGIN;
			if (shapeIndex == default) {
				var shape = CreateShape(ref specultive, null, out _);
				shapeIndex = Simulation.Simulation.Shapes.Add(in shape);
			}
			else {
				Simulation.Simulation.Shapes.GetShape<T>(shapeIndex.Index) = CreateShape(ref specultive, null, out _);
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