namespace RhuEngine.WorldObjects.ECS
{
	public enum UpdateEnum
	{
		Rendering = -1000,
		Normal = 0,
		Compute = 100,
		Movement = 1000,
		PlayerInput = 10000,
	}
	public class UpdateLevelAttribute : UpdatingComponentAttribute
	{
		public int offset;
		public UpdateLevelAttribute(int value) {
			offset = value;
		}
		public UpdateLevelAttribute(UpdateEnum value) {
			offset = (int)value;
		}
	}
}
