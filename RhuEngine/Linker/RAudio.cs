using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using NAudio.CoreAudioApi;
using NAudio.Wave;

using RNumerics;


namespace RhuEngine.Linker
{
	public enum SpeakerMode
	{
		Mono = -1,
		Stereo = 0,
		Surround_31 = 1,
		Surround_51 = 2,
		Surround_71 = 3,
	}

	public interface IRAudio
	{
		public WaveFormat WaveFormat { get; }
		public int BusCount { get; }
		public void AddBus(int atPos = -1);
		public int GetBusChannels(int index);
		public int GetBusIndex(string name);
		public string GetBusName(int index);

		public float GetBusPeakVolumeLeft(int index, int channel);
		public float GetBusPeakVolumeRight(int index, int channel);

		public string GetBusSend(int index);
		public float GetBusVolume(int index);
		public float GetMixRate();
		public double GetOutputLatency();
		public SpeakerMode GetSpeakerMode();
		public double GetTimeSinceLastMix();
		public double GetTimeToNextMix();
		public bool IsBusBypassingEffect(int index);
		public bool IsBusEffectEnabled(int index, int effextIdx);
		public bool IsBusMuted(int index);
		public bool IsBusSolo(int index);
		public void Lock();
		public void MoveBus(int index, int toIndex);
		public void RemoveBus(int index);
		public void RemoveBusEffect(int index, int effect);
		public void SetBusBypassEffects(int index, bool enabled);
		public void SetBusEffectEnabled(int index, int effect_index, bool enabled);
		public void SetBusMuted(int index, bool mute);
		public void SetBusName(int index, string name);
		public void SetBusSend(int index, string name);
		public void SetBusSolo(int index, bool solo);
		public void SetBusVolune(int index, float volume);
		public void SetEnableTaggingUsedAudioSteams(bool enable);
		public void SwapBusEffects(int busIndex, int effectIndex, int byEffectIndex);
		public void UnLock();
		public string[] EngineAudioOutputDevices();
		public string CurrentAudioOutputDevice { get; set; }
		public float PlayBackSpeed { get; set; }
	}

	public enum AudioBus
	{
		Master,
		World,
		Media,
		Voice
	}

	public static class RAudio
	{

		public static IRAudio Inst { get; set; }

		public const int MASTER_BUS_ID = 0;

		public const int WORLD_BUS_ID = 1;

		public const int MEDIA_BUS_ID = 2;

		public const int VOICE_BUS_ID = 3;

		private static Engine _engine;

		public static event Action UpateAudioSystems;

		public static string[] GetAudioInputs() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				return new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).Select(x => x.DeviceFriendlyName).ToArray();
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				return LinuxAudioUtils.GetInputDeviceNames().ToArray();
			}
			else {
				throw new PlatformNotSupportedException();
			}
		}

		public static IWaveIn GetWaveIn(WaveFormat waveFormat, int bufferMilliseconds, string deviceName = "default") {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				var targetIndex = Array.IndexOf(GetAudioInputs(), deviceName);
				return new WaveInEvent {
					WaveFormat = waveFormat,
					BufferMilliseconds = bufferMilliseconds,
					DeviceNumber = targetIndex + 1
				};
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				return new ALSACapture {
					WaveFormat = waveFormat,
					BufferMilliseconds = bufferMilliseconds,
					DeviceName = deviceName
				};
			}
			else {
				throw new PlatformNotSupportedException();
			}
		}

		public static void UpdataAudioSystem() {
			UpateAudioSystems?.Invoke();
		}

		public static void Initialize(Engine engine) {
			_engine = engine;
			if (Inst is null) {
				return;
			}
			Inst.AddBus(1);
			Inst.AddBus(2);
			Inst.AddBus(3);
			Inst.SetBusName(WORLD_BUS_ID, "World");
			Inst.SetBusName(MEDIA_BUS_ID, "Media");
			Inst.SetBusName(VOICE_BUS_ID, "Voice");
			engine.SettingsUpdate += Engine_SettingsUpdate;
			Engine_SettingsUpdate();
		}

		private static void Engine_SettingsUpdate() {
			Inst.SetBusVolune(MASTER_BUS_ID, _engine.MainSettings?.MasterVolumeDB ?? 0);
			Inst.SetBusVolune(WORLD_BUS_ID, _engine.MainSettings?.WorldVolumeDB ?? 0);
			Inst.SetBusVolune(MEDIA_BUS_ID, _engine.MainSettings?.MediaVolumeDB ?? 0);
			Inst.SetBusVolune(VOICE_BUS_ID, _engine.MainSettings?.VoiceVolumeDB ?? 0);
		}
	}
}
