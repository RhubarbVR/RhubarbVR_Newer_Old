using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Godot;

using NAudio.Wave;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;

public sealed class RhubarbAudioSource : IDisposable
{
	public AudioStreamGenerator Audio;
	private AudioStreamGeneratorPlayback _audioPlayBack;
	private AudioSourceBase _audioPlayer;
	private IWaveProvider _waveProvider;

	public RhubarbAudioSource() {
		Audio = new AudioStreamGenerator {
			BufferLength = 0.2f,
			MixRate = RAudio.Inst.WaveFormat.SampleRate,
		};
		RAudio.UpateAudioSystems += Update;
	}

	public void SetUpAudio(AudioSourceBase audioSourceBase) {
		if (audioSourceBase == null) {
			throw new ArgumentNullException(nameof(audioSourceBase));
		}

		_audioPlayer = audioSourceBase;
		Link();
	}

	public void Instansiate(AudioStreamPlayback audioStreamPlayback) {
		_audioPlayBack = (AudioStreamGeneratorPlayback)audioStreamPlayback;
	}

	private void Link() {
		_audioPlayer.AudioStream.LoadChange += AudioStream_LoadChange;
		AudioStream_LoadChange(_audioPlayer.AudioStream.Asset);
	}

	private void AudioStream_LoadChange(IWaveProvider obj) {
		if (obj == null) {
			return;
		}

		if (RAudio.Inst.WaveFormat.Equals(obj.WaveFormat)) {
			_waveProvider = obj;
			return;
		}
		try {
			// Create new resampler
			_waveProvider = new AudioConverter(RAudio.Inst.WaveFormat, obj);
		}
		catch (Exception ex) {
			// Log error and return if resampler creation fails
			GD.PrintErr($"Error creating MediaFoundationResampler: {ex.Message}");
			return;
		}
	}

	private unsafe void ReadAudio() {
		if (_audioPlayBack is null) {
			return;
		}
		if (_waveProvider is null) {
			_audioPlayBack.PushBuffer(new Vector2[_audioPlayBack.GetFramesAvailable()]);
			return;
		}
		var audioFrames = _audioPlayBack.GetFramesAvailable();
		if (audioFrames <= 0) {
			return;
		}

		var bytes = new byte[audioFrames * sizeof(Vector2)];
		try {
			var readAmount = _waveProvider.Read(bytes, 0, bytes.Length) / sizeof(Vector2);

			var audioBuffer = new Vector2[readAmount];
			fixed (byte* byteData = bytes) {
				var castedPointer = (Vector2*)byteData;
				for (var i = 0; i < readAmount; i++) {
					audioBuffer[i] = castedPointer[i];
				}
			}
			_audioPlayBack.PushBuffer(audioBuffer);
		}
		catch (Exception ex) {
			// Log error and return if audio reading fails
			GD.PrintErr($"Error reading audio: {ex.Message}");
			return;
		}
	}

	public void Dispose() {
		RAudio.UpateAudioSystems -= Update;
		Audio?.Free();
		Audio = null;
		_waveProvider = null;
	}

	private void Update() {
		ReadAudio();
	}
}