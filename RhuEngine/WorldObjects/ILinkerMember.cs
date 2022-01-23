namespace RhuEngine.WorldObjects
{
	public interface ILinkerMember<T> : IValueSource<T>, ILinkable
	{
		void Link(ILinker source);
		void ForceLink(ILinker source);

	}
}
