using System;

using RhuEngine.DataStructure;
using RhuEngine.WorldObjects.ECS;

using OpusDotNet;

using System.Collections.Generic;
using RhuEngine.Linker;

namespace RhuEngine.WorldObjects
{
	public class OpusStream : AudioStream
	{
		[OnChanged(nameof(LoadOpus))]
		[Default(Application.VoIP)]
		public readonly Sync<Application> typeOfStream;

		[OnChanged(nameof(LoadOpus))]
		[Default(Bandwidth.MediumBand)]
		public readonly Sync<Bandwidth> MaxBandwidth;

		[OnChanged(nameof(LoadOpus))]
		[Default(true)]
		public readonly Sync<bool> DTX;
		[OnChanged(nameof(UpdateFrameSize))]

		[Default(64000)]
		public readonly Sync<int> BitRate;
		
		private OpusDecoder _decoder;
		private OpusEncoder _encoder;
		
		private int _packedSize = 134;

		public override void UpdateFrameSize() {
			base.UpdateFrameSize();
			_packedSize = (BitRate.Value / 8 / TimeScale) + 1;
		}

		public override bool IsRunning => (_encoder is not null) && (_decoder is not null);

		private void LoadOpus() {
			if (_encoder is not null) {
				_encoder.Dispose();
				_encoder = null;
			}
			if (_decoder is not null) {
				_decoder.Dispose();
				_decoder = null;
			}
			try {
				_encoder = new OpusEncoder(typeOfStream.Value, 48000, 1) {
					VBR = true,
					DTX = DTX,
					MaxBandwidth = MaxBandwidth
				};
				_decoder = new OpusDecoder(48000, 1);
			}
			catch (Exception ex) {
				RLog.Err($"Exception when loading Opus {ex}");
			}
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			LoadOpus();
		}

		public override byte[] SendAudioSamples(float[] audio) {
			var outpack = new byte[_packedSize];
			var amount = _encoder.Encode(audio, SampleCount, outpack, outpack.Length);
			Array.Resize(ref outpack, amount);
			return outpack;
		}

		public override float[] ProssesAudioSamples(byte[] data) {
			var audio = new float[SampleCount];
			_decoder.Decode(data, data?.Length??0, audio, audio.Length);
			return audio;
		}
	}
}
