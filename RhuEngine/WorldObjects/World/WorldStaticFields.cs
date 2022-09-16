using RhuEngine.Datatypes;

namespace RhuEngine.WorldObjects
{
	public sealed partial class World
	{
		public bool IsRemoved => false;

		public NetPointer Pointer
		{
			get => new();
			set {}
		}

		public IWorldObject Parent => null;

		World IWorldObject.World => this;

		public bool Persistence => true;

		public EditLevel EditLevel => EditLevel.None;

		public string Name => "World";
	}
}
