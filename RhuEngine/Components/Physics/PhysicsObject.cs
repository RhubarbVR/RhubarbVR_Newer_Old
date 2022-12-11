using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	public abstract class PhysicsObject : Component
	{
		public const float SPECULATIVE_MARGIN = 0.1f;
		public const float COMMON_MAX = 1000000f;
		public const float COMMON_MIN = 0.0000001f;

		public PhysicsSimulation Simulation => World.PhysicsSimulation;

		[Default(RCursorShape.Arrow)]
		public readonly Sync<RCursorShape> CursorShape;
		protected override void OnAttach() {
			base.OnAttach();
			if (Entity.GetFirstComponent<Grabbable>() is not null) {
				CursorShape.Value = RCursorShape.Move;
			}
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
