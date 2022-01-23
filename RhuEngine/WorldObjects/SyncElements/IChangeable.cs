using System;

namespace RhuEngine.WorldObjects
{
	public interface IChangeable
	{
		public event Action<IChangeable> Changed;
	}
}
