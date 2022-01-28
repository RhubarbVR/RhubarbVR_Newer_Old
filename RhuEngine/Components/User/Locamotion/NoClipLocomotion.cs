using RhuEngine.Utils;
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

		private float _rotationX;
		private float _rotationY;

		//Make rotation head based
		public void ProcessMovement(Vec3 movementVector, Vec3 rotation, UserRoot userRoot, bool sprint) {
			_rotationX += rotation.x * ((sprint & AllowSprint.Value) ? SprintRotationSpeed.Value : RotationSpeed.Value);
			_rotationY += rotation.y * ((sprint & AllowSprint.Value) ? SprintRotationSpeed.Value : RotationSpeed.Value);
			_rotationX = MathR.Clamp(_rotationX, -90, 90);
			
			var moveVec = Quat.FromAngles(_rotationX, _rotationY, 180f) * movementVector;
			
			userRoot.Entity.position.Value += (moveVec * ((sprint & AllowSprint.Value) ? SprintMovementSpeed.Value : MovementSpeed.Value));
			userRoot.Entity.rotation.Value = Quat.FromAngles(new Vec3(_rotationX,_rotationY,0));
		}
	}
}
