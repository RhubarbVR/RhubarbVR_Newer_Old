using System;

namespace RhuEngine.WorldObjects
{
	public interface IOffsetableElement
	{
		public int Offset { get; }

		public event Action OffsetChanged;

	}
}
