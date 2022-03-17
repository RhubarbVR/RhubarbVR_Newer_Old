using System;

using RhuEngine.DataStructure;
using RhuEngine.WorldObjects.ECS;
using StereoKit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

		[OnChanged(nameof(UpdateFrameSize))]
		[Default(AudioFrameTime.ms40)]
		public Sync<AudioFrameTime> frameSize;

		public virtual void UpdateFrameSize() {
			_output = Sound.CreateStream(TimeInMs * 0.003f);
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

		public virtual bool IsRunning => true;
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
			UpdateFrameSize();
			Load(_output);
		}

		public void LoadInput(string deviceName = null) {
			if (RuntimeInformation.FrameworkDescription.StartsWith("Mono ")) {
				Log.Err("Android And mic is buggy");
				return;
			}
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

		private float[] _samples = new float[0];

		private const int MAX_QUEUE_SIZE = 2;

		private readonly Queue<byte[]> _samplesQueue = new(MAX_QUEUE_SIZE);

		private float[] _currentData = new float[1];

		private long _currsorPos = 0;
		private long _startPos = 0;

		//Todo: make read a whole array
		private float ReadSample() {
			_currsorPos++;
			if ((_startPos + _currentData.LongLength) > _currsorPos) {
				return _currentData[_currsorPos - _startPos];
			}
			else {
				_startPos = _currsorPos + 1;
				if(_samplesQueue.Count > MAX_QUEUE_SIZE) {
					for (var i = 0; i < MAX_QUEUE_SIZE - _samplesQueue.Count; i++) {
						_samplesQueue.Dequeue(); // Drop Packed
					}
				}
				_currentData = _samplesQueue.Count > 0 ? ProssesAudioSamples(_samplesQueue.Dequeue()) : ProssesAudioSamples(null);
				return _currentData[0];
			}
		}

		public override void Received(Peer sender, IDataNode data) {
			if (data is DataNode<byte[]> dataNode) {
				_samplesQueue.Enqueue(dataNode);
			}
		}

		private bool ShouldSendAudioPacked(float[] samples) {
			var sum = 0f;
			for (var i = 0; i < samples.Length; i++) {
				sum += samples[i];
			}
			var average = sum / samples.Length;


			return true;
		}

		public void Step() {
			if (NoSync) {
				return;
			}
			if (!IsRunning) {
				return;
			}
			if (!_loadedDevice) {
				if(_output is null) {
					return;
				}
				var count = Math.Max(0, (int)(0.1f * 48000) - (_output.TotalSamples - _output.CursorSamples));
				if (_samples.Length < count) {
					_samples = new float[count];
				}
				for (var i = 0; i < count; i++) {
					_samples[i] = ReadSample();
				}
				_output.WriteSamples(_samples, count);
			}
			else {
				if (_input is null) {
					return;
				}
				if (_input.UnreadSamples >= SampleCount) { 
					var audioPacked = new float[SampleCount];
					_input.ReadSamples(ref audioPacked);
					if (ShouldSendAudioPacked(audioPacked)) {
						World.BroadcastDataToAllStream(this, new DataNode<byte[]>(SendAudioSamples(audioPacked)), LiteNetLib.DeliveryMethod.Unreliable);
					}
				}
			}
		}
	}
}
