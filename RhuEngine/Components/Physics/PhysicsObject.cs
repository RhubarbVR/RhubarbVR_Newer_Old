using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using RhuEngine.Physics;
using System.Runtime.CompilerServices;

namespace RhuEngine.Components
{
	[Flags]
	public enum EPhysicsMask : ushort
	{
		None = 0,
		Layer1 = 1,
		UI = Layer1,
		Layer2 = 2,
		Player = Layer2,
		Layer3 = 4,
		Floor = Layer3,
		Layer4 = 8,
		WorldObjects = Layer4,
		Layer5 = 16,
		Layer6 = 32,
		Layer7 = 64,
		Layer8 = 128,
		Layer9 = 256,
		Layer10 = 512,
		Layer11 = 1024,
		Layer12 = 2048,
		Layer13 = 4096,
		Layer14 = 8192,
		Layer15 = 16384,
		Normal = Layer1 | Layer2 | Layer3 | Layer4 | Layer5,
		All = Layer1 | Layer2 | Layer3 | Layer4 | Layer5 | Layer6 | Layer7 | Layer8 | Layer9 | Layer10 | Layer11 | Layer12 | Layer13 | Layer14 | Layer15,
	}

	public static class EPhysicsMaskHelper {
		[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool BasicCheck(this EPhysicsMask a, EPhysicsMask b) {
			return (a & b) != EPhysicsMask.None;
		}
	}

	public abstract class PhysicsObject : Component
	{
		public const float SPECULATIVE_MARGIN = 0.1f;
		public const float COMMON_MAX = 1000000f;
		public const float COMMON_MIN = 0.0000001f;

		public PhysicsSimulation Simulation => World.PhysicsSimulation;

		[Default(true)]
		public readonly Sync<bool> CollisionEnabled;

		[Default(EPhysicsMask.WorldObjects)]
		[OnChanged(nameof(MaskUpdate))]
		public readonly Sync<EPhysicsMask> Group;

		[Default(EPhysicsMask.WorldObjects)]
		[OnChanged(nameof(MaskUpdate))]
		public readonly Sync<EPhysicsMask> Mask;

		[Default(RCursorShape.Arrow)]
		public readonly Sync<RCursorShape> CursorShape;

		protected virtual void MaskUpdate() {

		}
		
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

		public virtual void Touch(uint handed, Vector3f hitnormal, Vector3f hitpointworld, Handed handedSide) {
			OnTouchPyhsics?.Invoke(handed, hitnormal, hitpointworld, handedSide);
			Entity.CallOnTouch(handed, hitnormal, hitpointworld, handedSide);
			RUpdateManager.ExecuteOnStartOfUpdate(() => {
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

		public virtual void Lazer(uint v, Vector3f hitnormal, Vector3f hitpointworld, float pressForce, float gripForce, Handed hand) {
			OnLazerPyhsics?.Invoke(v, hitnormal, hitpointworld, pressForce, gripForce, hand);
			Entity.CallOnLazer(v, hitnormal, hitpointworld, pressForce, gripForce, hand);
			RUpdateManager.ExecuteOnStartOfUpdate(() => {
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
