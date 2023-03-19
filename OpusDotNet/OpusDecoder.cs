using System;

namespace OpusDotNet
{
	/// <summary>
	/// Provides audio decoding with Opus.
	/// </summary>
	public sealed class OpusDecoder : IDisposable
	{
		private readonly SafeDecoderHandle _handle;
		// Number of samples in the frame size, per channel.
		private readonly int _samples;
		private readonly int _pcmLength;

		/// <summary>
		/// Initializes a new <see cref="OpusDecoder"/> instance, with 48000 Hz sample rate and 2 channels.
		/// </summary>
		public OpusDecoder() : this(60, 48000, 2) {
		}

		/// <summary>
		/// Initializes a new <see cref="OpusDecoder"/> instance, with the specified frame size, 48000 Hz sample rate and 2 channels.
		/// </summary>
		/// <param name="frameSize">The frame size used when encoding, 2.5, 5, 10, 20, 40 or 60 ms.</param>
		[Obsolete("This constructor was used for the old decode method and is deprecated, please use the new decode method instead.")]
		public OpusDecoder(double frameSize) : this(frameSize, 48000, 2) {
		}

		/// <summary>
		/// Initializes a new <see cref="OpusDecoder"/> instance, with the specified sample rate and channels.
		/// </summary>
		/// <param name="sampleRate">The sample rate to decode to, 48000, 24000, 16000, 12000 or 8000 Hz.</param>
		/// <param name="channels">The channels to decode to, mono or stereo.</param>
		public OpusDecoder(int sampleRate, int channels) : this(60, sampleRate, channels) {
		}


		private OpusDecoder(double frameSize, int sampleRate, int channels) {
			if (!NativeLib.Load()) {
				throw new Exception("Failed to load opus lib");
			}
			switch (frameSize) {
				case 2.5:
				case 5:
				case 10:
				case 20:
				case 40:
				case 60:
					break;
				default:
					throw new ArgumentException("Value must be one of the following: 2.5, 5, 10, 20, 40 or 60.", nameof(frameSize));
			}

			switch (sampleRate) {
				case 8000:
				case 12000:
				case 16000:
				case 24000:
				case 48000:
					break;
				default:
					throw new ArgumentException("Value must be one of the following: 8000, 12000, 16000, 24000 or 48000.", nameof(sampleRate));
			}

			if (channels < 1 || channels > 2) {
				throw new ArgumentOutOfRangeException(nameof(channels), "Value must be between 1 and 2.");
			}

			SampleRate = sampleRate;
			Channels = channels;

			_samples = API.GetSampleCount(frameSize, sampleRate);
			_pcmLength = API.GetPCMLength(_samples, channels);

			_handle = API.opus_decoder_create(sampleRate, channels, out var error);
			API.ThrowIfError(error);
		}

		/// <summary>
		/// Gets the sample rate, 48000, 24000, 16000, 12000 or 8000 Hz.
		/// </summary>
		public int SampleRate { get; }

		/// <summary>
		/// Gets the channels, mono or stereo.
		/// </summary>
		public int Channels { get; }

		/// <summary>
		/// Decodes an Opus packet or any FEC (forward error correction) data.
		/// </summary>
		/// <param name="opusBytes">The Opus packet, or null to indicate packet loss.</param>
		/// <param name="opusLength">The maximum number of bytes to read from <paramref name="opusBytes"/>, or -1 to indicate packet loss.</param>
		/// <param name="pcmBytes">The buffer that the decoded audio will be stored in.</param>
		/// <param name="samples">The maximum number of bytes to write to <paramref name="pcmBytes"/>.
		/// When using FEC (forward error correction) this must be a valid frame size that matches the duration of the missing audio.</param>
		/// <returns>The number of bytes written to <paramref name="pcmBytes"/>.</returns>
		public int DecodeFloat(byte[] opusBytes, int opusLength, byte[] pcmBytes, int samples) {
			if (!NativeLib.Load()) {
				throw new Exception("Failed to load opus lib");
			}
			if (opusLength < 0 && opusBytes != null) {
				throw new ArgumentOutOfRangeException(nameof(opusLength), $"Value cannot be negative when {nameof(opusBytes)} is not null.");
			}

			if (opusBytes != null && opusBytes.Length < opusLength) {
				throw new ArgumentOutOfRangeException(nameof(opusLength), $"Value cannot be greater than the length of {nameof(opusBytes)}.");
			}

			if (pcmBytes == null) {
				throw new ArgumentNullException(nameof(pcmBytes));
			}

			ThrowIfDisposed();

			int result;

			if (opusBytes != null) {
				result = API.opus_decode_float(_handle, opusBytes, opusLength, pcmBytes, samples, 0);
			}
			else {
				// If forward error correction is enabled, this will indicate a packet loss.
				result = API.opus_decode_float(_handle, null, 0, pcmBytes, samples, 1);
			}

			API.ThrowIfError(result);
			return result;
		}


		/// <summary>
		/// Decodes an Opus packet or any FEC (forward error correction) data.
		/// </summary>
		/// <param name="opusBytes">The Opus packet, or null to indicate packet loss.</param>
		/// <param name="opusLength">The maximum number of bytes to read from <paramref name="opusBytes"/>, or -1 to indicate packet loss.</param>
		/// <param name="pcmBytes">The buffer that the decoded audio will be stored in.</param>
		/// <param name="pcmLength">The maximum number of bytes to write to <paramref name="pcmBytes"/>.
		/// When using FEC (forward error correction) this must be a valid frame size that matches the duration of the missing audio.</param>
		/// <returns>The number of bytes written to <paramref name="pcmBytes"/>.</returns>
		public unsafe int Decode(byte[] opusBytes, int opusLength, byte[] pcmBytes, int pcmLength) {
			if (!NativeLib.Load()) {
				throw new Exception("Failed to load opus lib");
			}
			if (opusLength < 0 && opusBytes != null) {
				throw new ArgumentOutOfRangeException(nameof(opusLength), $"Value cannot be negative when {nameof(opusBytes)} is not null.");
			}

			if (opusBytes != null && opusBytes.Length < opusLength) {
				throw new ArgumentOutOfRangeException(nameof(opusLength), $"Value cannot be greater than the length of {nameof(opusBytes)}.");
			}

			if (pcmBytes == null) {
				throw new ArgumentNullException(nameof(pcmBytes));
			}

			if (pcmLength < 0) {
				throw new ArgumentOutOfRangeException(nameof(pcmLength), "Value cannot be negative.");
			}

			if (pcmBytes.Length < pcmLength) {
				throw new ArgumentOutOfRangeException(nameof(pcmLength), $"Value cannot be greater than the length of {nameof(pcmBytes)}.");
			}

			var frameSize = API.GetFrameSize(pcmLength, SampleRate, Channels);

			if (opusBytes == null) {
				switch (frameSize) {
					case 2.5:
					case 5:
					case 10:
					case 20:
					case 40:
					case 60:
						break;
					default:
						throw new ArgumentException("When using FEC the frame size must be one of the following: 2.5, 5, 10, 20, 40 or 60.", nameof(pcmLength));
				}
			}

			ThrowIfDisposed();

			var samples = API.GetSampleCount(frameSize, SampleRate);


			int result;
			fixed (byte* input = opusBytes)
			fixed (byte* output = pcmBytes) {
				var inputPtr = (IntPtr)input;
				var outputPtr = (IntPtr)output;

				if (opusBytes != null) {
					result = API.opus_decode(_handle, inputPtr, opusLength, outputPtr, samples, 0);
				}
				else {
					// If forward error correction is enabled, this will indicate a packet loss.
					result = API.opus_decode(_handle, IntPtr.Zero, 0, outputPtr, samples, 1);
				}
			}

			API.ThrowIfError(result);
			return API.GetPCMLength(result, Channels);
		}

		/// <summary>
		/// Releases all resources used by the current instance.
		/// </summary>
		public void Dispose() {
			_handle?.Dispose();
		}

		private void ThrowIfDisposed() {
			if (_handle.IsClosed) {
				throw new ObjectDisposedException(GetType().FullName);
			}
		}
	}
}
