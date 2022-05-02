
using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public interface ILinkable : IWorldObject
	{
		object Object { get; set; }
		void Link(ILinker source);
		void ForceLink(ILinker source);

		bool IsLinkedTo { get; }
		NetPointer LinkedFrom { get; }

		void KillLink();
	}
}
