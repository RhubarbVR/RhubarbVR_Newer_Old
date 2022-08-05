using System;

using RNumerics;

namespace RhuEngine.WorldObjects
{
	public interface IChangeable
	{
		public event Action<IChangeable> Changed;
	}
}
