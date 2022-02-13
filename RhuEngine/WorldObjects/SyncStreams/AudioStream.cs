using System;

using RhuEngine.DataStructure;
using RhuEngine.WorldObjects.ECS;
using StereoKit;
using System.Collections.Generic;
using System.Diagnostics;

namespace RhuEngine.WorldObjects
{
	public abstract class AudioStream : SyncStream, INetworkedObject, IAssetProvider<Sound>, IGlobalStepable
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

		[Default(AudioFrameTime.ms2_5)]
		public Sync<AudioFrameTime> frameSize;

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

		public int SampleCount => (int)(48000 / 1000 * TimeInMs);

		public event Action<Sound> OnAssetLoaded;

		public Sound Value { get; private set; }

		public void Load(Sound data) {
			Value = data;
			Loaded = data != null;
			OnAssetLoaded?.Invoke(data);
		}

		public bool Loaded { get; private set; } = false;

		public override void Dispose() {
			Load(null);
			base.Dispose();
		}
		private Sound _input;

		private Sound _output;

		private bool _loadedDevice = false;
		
		public override void OnLoaded() {
			_output = Sound.CreateStream(1f);
			Load(_output);
		}

		public void LoadInput(string deviceName = null) {
			if (deviceName is null) {
				deviceName = Engine.MainMic;
				Engine.MicChanged += LoadInput;
			}
			if (!Microphone.Start(deviceName)) {
				Log.Err($"Failed to load Mic {deviceName ?? "System Default"}");
				return;
			}
			_input = Microphone.Sound;
			Load(_input);
			_output = null;
			_loadedDevice = true;
		}

		public virtual byte[] SendAudioSamples(float[] audio) {
			throw new NotImplementedException();
		}

		public virtual float[] ProssesAudioSamples(byte[] data) {
			throw new NotImplementedException();
		}


		public Queue<byte[]> audioData = new(3);

		public override void Received(Peer sender, IDataNode data) {
			if (data is DataNode<byte[]> dataNode) {
				audioData.Enqueue(dataNode);
			}
		}

		private bool ShouldSendAudioPacked(float[] samples) {
			return true;
		}

		public void Step() {
			if (NoSync) {
				return;
			}
			if (!_loadedDevice) {
				if(_output is null) {
					return;
				}
				if (_output.TotalSamples - _output.CursorSamples >= SampleCount) {
					if (audioData.Count > 0) {
						_output.WriteSamples(ProssesAudioSamples(audioData.Dequeue()));
					}
					else {
						_output.WriteSamples(new float[SampleCount]);
					}
				}
			}
			else {
				if (_input is null) {
					return;
				}
				if (_input.UnreadSamples >= SampleCount) { 
					var audioPacked = new float[SampleCount];
					_input.ReadSamples(ref audioPacked);
					if (ShouldSendAudioPacked(audioPacked)) {
						World.BroadcastDataToAllStream(this, new DataNode<byte[]>(SendAudioSamples(audioPacked)), LiteNetLib.DeliveryMethod.ReliableUnordered);
					}
				}
			}
		}
	}
}
