using System;

namespace RhuEngine.Managers
{
	public interface IManager : IDisposable
	{
		public void Init(Engine engine);

		public void Step();
		public void RenderStep();
	}
}
