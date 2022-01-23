
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface ILinkable : IWorldObject
	{
		bool IsLinked { get; }
		NetPointer LinkedFrom { get; }

		void KillLink();
	}
}
