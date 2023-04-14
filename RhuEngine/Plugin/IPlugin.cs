using System;

namespace RhuEngine.Plugin
{
	public enum InitializationLevel
	{
		BeforeStart = 0,
		BeforeDiscordManager = BeforeStart,
		AfterDiscordManager = 1,
		BeforeWindowManager = AfterDiscordManager,
		AfterWindowManager = 2,
		BeforeFileManager = AfterWindowManager,
		AfterFileManager = 3,
		BeforeLocalisationManager = AfterFileManager,
		AfterLocalisationManager = 4,
		BeforeInputManager = AfterLocalisationManager,
		AfterInputManager = 5,
		BeforeNetApiManager = AfterInputManager,
		AfterNetApiManager = 6,
		BeforeAssetManager = AfterNetApiManager,
		AfterAssetManager = 7,
		BeforeWasmManager = AfterAssetManager,
		AfterWasmManager = 8,
		BeforeWorldManager = AfterWasmManager,
		AfterWorldManager = 9,
	}

	public interface IPlugin : IDisposable
	{
		public void Initialization(InitializationLevel initializationLevel, Engine engine);

		public void Step();

		public void RenderStep();
	}
}
