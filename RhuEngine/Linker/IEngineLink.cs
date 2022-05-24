using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IEngineLink
	{
		bool ForceLibLoad { get; }
		bool SpawnPlayer { get; }
		bool CanRender { get; }
		bool CanAudio { get; }
		bool CanInput { get; }
		string BackendID { get; }

		void BindEngine(Engine engine);
		void Start();

		void LoadStatics();

	}
}
