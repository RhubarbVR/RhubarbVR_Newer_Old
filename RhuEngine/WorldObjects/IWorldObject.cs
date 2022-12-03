using System;

using RhuEngine.Datatypes;
namespace RhuEngine.WorldObjects
{

	/// <summary>
	/// Helper class for WorldObjects
	/// </summary>
	public static class WorldObjectHelper
	{
		/// <summary>
		/// Checks if the parent or parent of parent of the IWorldObject is removed. 
		/// </summary>
		/// <param name="worldObject">The world object to check.</param>
		/// <returns>True if any of the parent are removed, else false.</returns>
		public static bool IsParentRemoved(this IWorldObject worldObject) {
			// Needs to check if both the world object and its parents aren't removed.
			return (worldObject.Parent?.IsRemoved ?? false) || (worldObject.Parent?.IsParentRemoved() ?? false);
		}
	}

	/// <summary>
	/// Used to define the level of editing that can be done on an object.
	/// </summary>
	public enum EditLevel : byte
	{
		None,
		Open,
		CreaterOnly,
		HostOnly,
		CreaterAndHost,
	}
	/// <summary>
	/// A WorldObject is an object that is part of the networked world.
	/// It can be interacted with, edited and viewed.
	/// </summary>
	public interface IWorldObject : IDisposable
	{

		/// <summary>
		/// Gets the name of the object.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="RhuEngine.WorldObjects.IWorldObject"/> is removed.
		/// </summary>
		/// <value><c>true</c> if is removed; otherwise, <c>false</c>.</value>
		public bool IsRemoved { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="RhuEngine.WorldObjects.IWorldObject"/> is persisted.
		/// </summary>
		/// <value><c>true</c> if this instance is persisted; otherwise, <c>false</c>.</value>
		public bool Persistence { get; }

		/// <summary>
		/// Gets or sets the pointer.
		/// </summary>
		/// <value>The pointer.</value>
		public NetPointer Pointer
		{
			get; set;
		}

		/// <summary>
		/// Gets the parent of this object.
		/// </summary>
		/// <value>The parent.</value>
		public IWorldObject Parent { get; }
		/// <summary>
		/// Gets the world the object is part of.
		/// </summary>
		/// <value>The world.</value>
		World World { get; }

	}
}
