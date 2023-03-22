using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using NAudio.Wave;

using RhuEngine.Linker;

namespace RhubarbVR.Bindings
{
	public sealed class GodotAudioMicLink : IWaveInWithBufferSize
	{
		public GodotAudio godotAudio;

		public GodotAudioMicLink(GodotAudio godotAudio) {
			this.godotAudio = godotAudio;
			SetUpMic();
		}

		private AudioStreamPlayer _streamPlayer;

		public const string MIC_BUS = "Mic";

		private AudioEffectCapture _audioEffectCapture;

		public void SetUpMic() {
			AudioServer.AddBus(1);
			AudioServer.SetBusName(1, MIC_BUS);
			AudioServer.SetBusMute(1, true);
			_audioEffectCapture = new() {
				BufferLength = 0.5f
			};
			AudioServer.AddBusEffect(1, _audioEffectCapture);
			_streamPlayer = new() {
				Stream = new AudioStreamMicrophone(),
				Playing = false,
				Bus = MIC_BUS
			};
			EngineRunnerHelpers._.AddChild(_streamPlayer);
		}

		public WaveFormat WaveFormat
		{
			get => WaveFormat.CreateIeeeFloatWaveFormat((int)AudioServer.GetMixRate(), 1);
			set { }
		}

		public int BufferSizeMilliseconds { get; set; }

		public event EventHandler<WaveInEventArgs> DataAvailable;
		public event EventHandler<StoppedEventArgs> RecordingStopped;

		public void Dispose() {
			StopRecording();
		}

		public void StartRecording() {
			_streamPlayer.Playing = true;
		}

		public void StopRecording() {
			_streamPlayer.Playing = false;
			_ranLastFrame = false;
			RecordingStopped?.Invoke(this, new StoppedEventArgs());
		}

		private bool _ranLastFrame = false;

		public int FrameAmount => (int)(AudioServer.GetMixRate() * (BufferSizeMilliseconds / 1000f));

		public unsafe void Update() {
			if (!_streamPlayer.Playing) {
				_ranLastFrame = false;
				return;
			}
			if (!_ranLastFrame) {
				_audioEffectCapture.ClearBuffer();
			}
			_ranLastFrame = true;
			var audioFrames = _audioEffectCapture.GetFramesAvailable();
			if (audioFrames <= FrameAmount) {
				return;
			}
			var audioBuffer = _audioEffectCapture.GetBuffer(FrameAmount);
			var bytes = new byte[audioBuffer.Length * sizeof(float) / 2];
			fixed (byte* byteData = bytes) {
				var castedPointer = (float*)byteData;
				for (var i = 0; i < audioBuffer.Length / 2; i++) {
					castedPointer[i] = (audioBuffer[i * 2].X + audioBuffer[i * 2].Y + audioBuffer[(i * 2) + 1].X + audioBuffer[(i * 2) + 1].Y) / 4;
				}
			}
			DataAvailable?.Invoke(this, new WaveInEventArgs(bytes, bytes.Length));
		}
	}

	public sealed class GodotAudio : IRAudio
	{
		public GodotAudio() {
			_godotAudioMicLink = new GodotAudioMicLink(this);
		}

		public int BusCount => AudioServer.BusCount;

		public string CurrentAudioInputDevice { get => AudioServer.InputDevice; set => AudioServer.InputDevice = value; }
		public string CurrentAudioOutputDevice { get => AudioServer.OutputDevice; set => AudioServer.OutputDevice = value; }
		public float PlayBackSpeed { get => AudioServer.PlaybackSpeedScale; set => AudioServer.PlaybackSpeedScale = value; }

		private readonly GodotAudioMicLink _godotAudioMicLink;
		public IWaveInWithBufferSize EngineInputAudio => _godotAudioMicLink;

		public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat((int)AudioServer.GetMixRate(), 2);

		public void AddBus(int atPos = -1) {
			AudioServer.AddBus(atPos);
		}

		public string[] EngineAudioInputDevices() {
			return AudioServer.GetInputDeviceList();
		}

		public string[] EngineAudioOutputDevices() {
			return AudioServer.GetOutputDeviceList();
		}

		public int GetBusChannels(int index) {
			return AudioServer.GetBusChannels(index);
		}

		public int GetBusIndex(string name) {
			return AudioServer.GetBusIndex(name);
		}

		public string GetBusName(int index) {
			return AudioServer.GetBusName(index);
		}

		public float GetBusPeakVolumeLeft(int index, int channel) {
			return AudioServer.GetBusPeakVolumeLeftDb(index, channel);
		}

		public float GetBusPeakVolumeRight(int index, int channel) {
			return AudioServer.GetBusPeakVolumeRightDb(index, channel);
		}

		public string GetBusSend(int index) {
			return AudioServer.GetBusSend(index);
		}

		public float GetBusVolume(int index) {
			return AudioServer.GetBusVolumeDb(index);
		}

		public float GetMixRate() {
			return AudioServer.GetMixRate();
		}

		public double GetOutputLatency() {
			return AudioServer.GetOutputLatency();
		}

		public SpeakerMode GetSpeakerMode() {
			return AudioServer.GetSpeakerMode() switch {
				AudioServer.SpeakerMode.ModeStereo => SpeakerMode.Stereo,
				AudioServer.SpeakerMode.Surround31 => SpeakerMode.Surround_31,
				AudioServer.SpeakerMode.Surround51 => SpeakerMode.Surround_51,
				AudioServer.SpeakerMode.Surround71 => SpeakerMode.Surround_71,
				_ => SpeakerMode.Mono,
			};
		}

		public double GetTimeSinceLastMix() {
			return AudioServer.GetTimeSinceLastMix();
		}

		public double GetTimeToNextMix() {
			return AudioServer.GetTimeToNextMix();
		}

		public bool IsBusBypassingEffect(int index) {
			return AudioServer.IsBusBypassingEffects(index);
		}

		public bool IsBusEffectEnabled(int index, int effextIdx) {
			return AudioServer.IsBusEffectEnabled(index, effextIdx);
		}

		public bool IsBusMuted(int index) {
			return AudioServer.IsBusMute(index);
		}

		public bool IsBusSolo(int index) {
			return AudioServer.IsBusSolo(index);
		}

		public void Lock() {
			AudioServer.Lock();
		}

		public void MoveBus(int index, int toIndex) {
			AudioServer.MoveBus(index, toIndex);
		}

		public void RemoveBus(int index) {
			AudioServer.RemoveBus(index);
		}

		public void RemoveBusEffect(int index, int effect) {
			AudioServer.RemoveBusEffect(index, effect);
		}

		public void SetBusBypassEffects(int index, bool enabled) {
			AudioServer.SetBusBypassEffects(index, enabled);
		}

		public void SetBusEffectEnabled(int index, int effect_index, bool enabled) {
			AudioServer.SetBusEffectEnabled(index, effect_index, enabled);
		}

		public void SetBusMuted(int index, bool mute) {
			AudioServer.SetBusMute(index, mute);
		}

		public void SetBusName(int index, string name) {
			AudioServer.SetBusName(index, name);
		}

		public void SetBusSend(int index, string name) {
			AudioServer.SetBusSend(index, name);
		}

		public void SetBusSolo(int index, bool solo) {
			AudioServer.SetBusSolo(index, solo);
		}

		public void SetBusVolune(int index, float volume) {
			AudioServer.SetBusVolumeDb(index, volume);
		}

		public void SetEnableTaggingUsedAudioSteams(bool enable) {
			AudioServer.SetEnableTaggingUsedAudioStreams(enable);
		}

		public void SwapBusEffects(int busIndex, int effectIndex, int byEffectIndex) {
			AudioServer.SwapBusEffects(busIndex, effectIndex, byEffectIndex);
		}

		public void UnLock() {
			AudioServer.Unlock();
		}

		public void Update() {
			_godotAudioMicLink.Update();
		}
	}
}
