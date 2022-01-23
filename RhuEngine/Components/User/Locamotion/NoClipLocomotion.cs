using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "User" })]
	public class NoClipLocomotion : Component, ILocomotionModule
	{
		[Default(1f)]
		public Sync<float> MovementSpeed;
		[Default(30f)]
		public Sync<float> RotationSpeed;
		[Default(2f)]
		public Sync<float> SprintMovementSpeed;
		[Default(80f)]
		public Sync<float> SprintRotationSpeed;
		[Default(true)]
		public Sync<bool> AllowSprint;

		public void ProcessMovement(Vec3 movementVector, float rotation, UserRoot userRoot, bool sprint) {
			userRoot.Entity.position.Value = userRoot.Entity.position.Value + (movementVector * ((sprint & AllowSprint.Value) ? SprintMovementSpeed.Value : MovementSpeed.Value));
			userRoot.Entity.rotation.Value = userRoot.Entity.rotation.Value * Quat.FromAngles(0, rotation * ((sprint & AllowSprint.Value) ? SprintRotationSpeed.Value : RotationSpeed.Value), 0);
		}
	}
}
