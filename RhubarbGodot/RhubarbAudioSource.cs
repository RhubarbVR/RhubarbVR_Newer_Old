using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Godot;

using NAudio.Wave;

using RhubarbVR.Bindings.ComponentLinking;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;

public sealed class RhubarbAudioSource : IDisposable
{
	public RhubarbAudioSource() {
		audio = new AudioStreamGenerator {
			BufferLength = 0.2f,
			MixRate = RAudio.Inst.WaveFormat.SampleRate,
		};
		RAudio.UpateAudioSystems += Update;
	}


	public void SetUpAudio(AudioSourceBase audioSourceBase) {
		audioPlayer = audioSourceBase;
		Link();
	}

	public void Instansiate(AudioStreamPlayback audioStreamPlayback) {
		audioPlayBack = (AudioStreamGeneratorPlayback)audioStreamPlayback;
	}

	public AudioSourceBase audioPlayer;

	private void Link() {
		audioPlayer.AudioStream.LoadChange += AudioStream_LoadChange;
		AudioStream_LoadChange(audioPlayer.AudioStream.Asset);
	}

	private void AudioStream_LoadChange(NAudio.Wave.IWaveProvider obj) {
		if (waveProvider is not null) {
			waveProvider.Dispose();
			waveProvider = null;
		}
		if (obj is null) {
			return;
		}
		waveProvider = new MediaFoundationResampler(obj, RAudio.Inst.WaveFormat);
	}

	public MediaFoundationResampler waveProvider;
	public AudioStreamGenerator audio;
	public AudioStreamGeneratorPlayback audioPlayBack;

	public unsafe void Update() {
		if (waveProvider is null) {
			return;
		}
		if (audioPlayBack is null) {
			return;
		}
		var audioFrames = audioPlayBack.GetFramesAvailable();
		if (audioFrames <= 0) {
			return;
		}
		var bytes = new byte[audioFrames * sizeof(Vector2)];
		try {
			var readAmount = waveProvider.Read(bytes, 0, bytes.Length) / sizeof(Vector2);

			var audioBuffer = new Vector2[readAmount];
			fixed (byte* byteData = bytes) {
				var castedPointer = (Vector2*)byteData;
				for (var i = 0; i < readAmount; i++) {
					audioBuffer[i] = castedPointer[i];
				}
			}
			audioPlayBack.PushBuffer(audioBuffer);
		}
		catch (InvalidComObjectException) { }
	}

	public void Dispose() {
		RAudio.UpateAudioSystems -= Update;
		audio?.Free();
		audio = null;
		waveProvider?.Dispose();
		waveProvider = null;
	}
}
