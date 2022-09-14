using System;

using RhuEngine.Datatypes;
namespace RhuEngine.WorldObjects
{
	public static class WorldObjectHelper
	{
		public static bool IsParentRemoved(this IWorldObject worldObject) {
			// Needs to check if both the world object and its parents aren't removed.
			return (worldObject.Parent?.IsRemoved ?? false) || (worldObject.Parent?.IsParentRemoved() ?? false);
		}
	}

	public enum EditLevel : byte
	{
		None,
		Open,
		CreaterOnly,
		HostOnly,
		CreaterAndHost,
	}

	public interface IWorldObject : IDisposable
	{
		public string Name { get; }
		public bool IsRemoved { get; }
		public bool Persistence { get; }

		public NetPointer Pointer
		{
			get; set;
		}
		public IWorldObject Parent { get; }
		World World { get; }

	}
}
