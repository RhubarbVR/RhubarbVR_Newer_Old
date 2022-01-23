namespace RhuEngine.WorldObjects
{
	public interface ILinker : IWorldObject
	{
		void SetLinkLocation(ILinkable val);

		void RemoveLinkLocation();
	}
}
