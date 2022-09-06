namespace RhuEngine.WorldObjects
{
	public interface IValueSource<T> : IChangeable, ISyncObject
	{
		T Value { get; set; }
	}
}
