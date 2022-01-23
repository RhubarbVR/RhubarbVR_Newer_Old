namespace RhuEngine.WorldObjects.ECS
{
	[UpdatingComponent]
	public interface IUpdatingComponent
	{
		public void Step();
	}
}
