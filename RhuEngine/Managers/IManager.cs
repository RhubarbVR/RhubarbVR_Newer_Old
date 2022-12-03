using System;

namespace RhuEngine.Managers
{
	/// <summary>
	/// Interface for Managers
	/// </summary>
	public interface IManager : IDisposable
	{
		/// <summary>
		/// Initialize the manager
		/// </summary>
		/// <param name="engine">
		/// Engine<see cref="Engine"/>
		/// </param>
		public void Init(Engine engine);
		/// <summary>
		/// Does game logix related activities
		/// </summary>
		public void Step();
		/// <summary>
		/// Does rendering related activities
		/// </summary>
		public void RenderStep();
	}
}
