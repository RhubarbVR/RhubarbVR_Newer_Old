namespace RhuEngine.WorldObjects
{
	public interface IValueSource<T> : IChangeable, IWorldObject
	{
		T Value { get; set; }
	}
}
