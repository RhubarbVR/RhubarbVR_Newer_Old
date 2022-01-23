using RhuEngine.WorldObjects;

using StereoKit;

namespace RhuEngine.Components
{
	public interface ILocomotionModule : ISyncObject
	{
		public void ProcessMovement(Vec3 movementVector, float rotation, UserRoot userRoot, bool sprint);
	}
}
