using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "User" })]
	public class NoClipLocomotion : LocomotionModule
	{
		[Default(1f)]
		public Sync<float> MovementSpeed;
		[Default(30f)]
		public Sync<float> RotationSpeed;
		[Default(2f)]
		public Sync<float> MovementSpeedMultiplier;
		[Default(80f)]
		public Sync<float> RotationSpeedMultiplier;
		[Default(true)]
		public Sync<bool> AllowMultiplier;

		public override void OnAttach() {
			base.OnAttach();
			locmotionName.Value = "No Clip";
		}

		public override void ProcessMovement() {
			
		}
	}
}
