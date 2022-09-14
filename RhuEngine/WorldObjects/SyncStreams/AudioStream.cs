using System;

using RhuEngine.DataStructure;
using RhuEngine.WorldObjects.ECS;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RhuEngine.Linker;
using NAudio.Wave;

namespace RhuEngine.WorldObjects
{
	public abstract class AudioStream : SyncStream, INetworkedObject, IAssetProvider<IWaveProvider>, IGlobalStepable
	{
		public enum AudioFrameTime
		{
			ms2_5,
			ms5,
			ms10,
			ms20,
			ms40,
			ms60
		}

		[OnChanged(nameof(UpdateFrameSize))]
		[Default(AudioFrameTime.ms40)]
		public readonly Sync<AudioFrameTime> frameSize;

		public virtual void UpdateFrameSize() {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
		}

		public float TimeInMs
		{
			get {
				return frameSize.Value switch {
					AudioFrameTime.ms2_5 => 2.5f,
					AudioFrameTime.ms5 => 5,
					AudioFrameTime.ms20 => 20,
					AudioFrameTime.ms40 => 40,
					AudioFrameTime.ms60 => 60,
					_ => 10,
				};
			}
		}

		public int TimeScale
		{
			get {
				return frameSize.Value switch {
					AudioFrameTime.ms2_5 => 4000,
					AudioFrameTime.ms5 => 200,
					AudioFrameTime.ms20 => 50,
					AudioFrameTime.ms40 => 25,
					AudioFrameTime.ms60 => 16,
					_ => 10,
				};
			}
		}

		public int SampleCount => (int)(48000 / 1000 * TimeInMs);

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

		protected override void OnLoaded() {
			UpdateFrameSize();
		}

		public void LoadInput(string deviceName = null) {
			if (!Engine.EngineLink.CanAudio) {
				return;
			}
			if (deviceName is null) {
				deviceName = Engine.MainMic;
				Engine.MicChanged += LoadInput;
			}
			//if (!RMicrophone.Start(deviceName,out var rMicReader)) {
			//	RLog.Err($"Failed to load Mic {deviceName ?? "System Default"}");
			//	return;
			//}
			//_input = rMicReader;
			//Load(_input.SoundClip);
			//_output = null;
			//_loadedDevice = true;
		}

		public override void Received(Peer sender, IDataNode data) {
		
		}

		public void Step() {
		}
	}
}
