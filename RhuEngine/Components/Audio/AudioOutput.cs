using System;
using System.Runtime.InteropServices;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using NAudio.Wave;
using System.IO;

namespace RhuEngine.Components
{
	public class RawAudioOutput : SyncObject, IAssetProvider<IWaveProvider>
	{
		public event Action<IWaveProvider> OnAssetLoaded;

		public IWaveProvider Value { get; private set; }

		public void Load(IWaveProvider data) {
			Value = data;
			Loaded = data != null;
			OnAssetLoaded?.Invoke(data);
		}

		public bool Loaded { get; private set; } = false;

		public override void Dispose() {
			Load(null);
			base.Dispose();
		}

		public BufferedWaveProvider audio;


		protected override void OnLoaded() {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			audio = new BufferedWaveProvider(new WaveFormat(44100, 32, 1));
			Load(audio);
		}

		public void WriteAudio(byte[] data) {
			if(audio is null) {
				return;
			}
			audio.AddSamples(data,0,data.Length);
		}

	}
}
