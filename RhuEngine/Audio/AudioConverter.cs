using System;
using NAudio.Wave;

namespace RhuEngine
{
	public sealed class AudioConverter : IWaveProvider
	{
		public WaveFormat WaveFormat { get; }

		public IWaveProvider InputStream { get; }

		public bool IgnoreSampleRateChanges { get; }

		private byte[] _readBuffer = Array.Empty<byte>();
		private float[] _convertBuffer = Array.Empty<float>();
		private float[] _workBuffer = Array.Empty<float>();

		public AudioConverter(WaveFormat waveFormat, IWaveProvider inputStream, bool ignoreSampleRateChanges) {
			IgnoreSampleRateChanges = ignoreSampleRateChanges;
			WaveFormat = waveFormat;
			InputStream = inputStream;
		}

		public unsafe int Read(byte[] buffer, int offset, int count) {
			if (WaveFormat.Equals(InputStream.WaveFormat)) {
				return InputStream.Read(buffer, offset, count);
			}
			if (!IgnoreSampleRateChanges) {
				if (WaveFormat.SampleRate != InputStream.WaveFormat.SampleRate) {
					throw new Exception("Input and output WaveFormats must have the same SampleRate.");
				}
			}
			var bytesPerSampleWrite = WaveFormat.BitsPerSample / 8 * WaveFormat.Channels;
			var bytesPerSampleRead = InputStream.WaveFormat.BitsPerSample / 8 * InputStream.WaveFormat.Channels;
			var requiredReadBufferLength = count / bytesPerSampleWrite * bytesPerSampleRead;

			// Resize the read buffer if necessary
			if (_readBuffer == null || _readBuffer.Length < requiredReadBufferLength) {
				Array.Resize(ref _readBuffer, requiredReadBufferLength);
			}

			var bytesRead = InputStream.Read(_readBuffer, 0, requiredReadBufferLength);
			var floatDataSize = ConvertToFloatArray(ref _convertBuffer, _readBuffer, InputStream.WaveFormat, bytesRead);
			var amoutReturned = ConvertChannels(floatDataSize, ref _convertBuffer, ref _workBuffer, InputStream.WaveFormat.Channels, WaveFormat.Channels);
			var convertedBytesLength = ConvertToByteArray(ref _readBuffer, _workBuffer, amoutReturned, WaveFormat);
			Buffer.BlockCopy(_readBuffer, 0, buffer, offset, convertedBytesLength);
			return convertedBytesLength;
		}

		public static int ConvertToByteArray(ref byte[] bytes, float[] floats, int amoutReturn, WaveFormat waveFormat) {
			return waveFormat.Encoding == WaveFormatEncoding.IeeeFloat
				? FloatArrayToByteArray(ref bytes, floats, amoutReturn)
				: FloatArrayToByteArray(ref bytes, floats, amoutReturn, waveFormat.BitsPerSample);
		}

		public static int ConvertToFloatArray(ref float[] floats, byte[] bytes, WaveFormat waveFormat, int bytesRead) {
			return waveFormat.Encoding == WaveFormatEncoding.IeeeFloat
				? ByteArrayToFloatArray(ref floats, bytesRead, bytes)
				: ByteArrayToFloatArray(ref floats, bytesRead, bytes, waveFormat.BitsPerSample);
		}


		public static int FloatArrayToByteArray(ref byte[] bytes, float[] floatArray, int amoutReturn) {
			var amount = amoutReturn * sizeof(float);
			if (amount > bytes.Length) {
				Array.Resize(ref bytes, amount);
			}
			Buffer.BlockCopy(floatArray, 0, bytes, 0, amount);
			return amount;
		}

		public static int ByteArrayToFloatArray(ref float[] floats, int bytesRead, byte[] byteArray) {
			var length = bytesRead / sizeof(float);
			if (length > floats.Length) {
				Array.Resize(ref floats, length);
			}
			Buffer.BlockCopy(byteArray, 0, floats, 0, bytesRead);
			return length;
		}

		public static int ByteArrayToFloatArray(ref float[] floatArray, int bytesRead, byte[] byteArray, int bitsPerSample) {
			if (bitsPerSample is not 16 and not 32) {
				throw new ArgumentException("Bits per sample must be 16 or 32.");
			}

			var bytesPerSample = bitsPerSample / 8;
			var floatArrayLength = bytesRead / bytesPerSample;
			if (floatArrayLength > floatArray.Length) {
				Array.Resize(ref floatArray, floatArrayLength);
			}

			for (var i = 0; i < floatArrayLength; i++) {
				if (bitsPerSample == 16) {
					var sample = BitConverter.ToInt16(byteArray, i * bytesPerSample);
					floatArray[i] = sample / 32768.0f; // Normalize to the range [-1, 1]
				}
				else if (bitsPerSample == 32) {
					var sample = BitConverter.ToInt32(byteArray, i * bytesPerSample);
					floatArray[i] = sample / 2147483648.0f; // Normalize to the range [-1, 1]
				}
			}

			return floatArrayLength;
		}

		public static int FloatArrayToByteArray(ref byte[] byteArray, float[] floatArray, int amoutReturn, int bitsPerSample) {
			if (bitsPerSample is not 16 and not 32) {
				throw new ArgumentException("Bits per sample must be 16 or 32.");
			}

			var bytesPerSample = bitsPerSample / 8;
			var byteArrayLength = amoutReturn * bytesPerSample;
			if (byteArray.Length < byteArrayLength) {
				Array.Resize(ref byteArray, byteArrayLength);
			}

			for (var i = 0; i < amoutReturn; i++) {
				if (bitsPerSample == 16) {
					var sample = (short)(floatArray[i] * 32768); // De-normalize from the range [-1, 1]
					BitConverter.GetBytes(sample).CopyTo(byteArray, i * bytesPerSample);
				}
				else if (bitsPerSample == 32) {
					var sample = (int)(floatArray[i] * 2147483648); // De-normalize from the range [-1, 1]
					BitConverter.GetBytes(sample).CopyTo(byteArray, i * bytesPerSample);
				}
			}

			return byteArrayLength;
		}



		public static int ConvertChannels(int inputAudioSize, ref float[] inputAudio, ref float[] outputAudio, int inputChannels, int outputChannels) {
			var inputFrameCount = inputAudioSize / inputChannels;
			var outputFrameCount = inputFrameCount;
			var returnSize = outputFrameCount * outputChannels;
			if (returnSize > outputAudio.Length) {
				Array.Resize(ref outputAudio, returnSize);
			}

			for (var i = 0; i < inputFrameCount; i++) {
				for (var j = 0; j < outputChannels; j++) {
					if (inputChannels == 1) // Mono to multi-channel
					{
						outputAudio[(i * outputChannels) + j] = inputAudio[i];
					}
					else if (inputChannels == 2 && outputChannels == 1) // Stereo to mono
					{
						outputAudio[i] = (inputAudio[i * inputChannels] + inputAudio[(i * inputChannels) + 1]) / 2.0f;
					}
					else if (outputChannels == 1) // Multi-channel to mono
					{
						var sum = 0.0f;
						for (var k = 0; k < inputChannels; k++) {
							sum += inputAudio[(i * inputChannels) + k];
						}
						outputAudio[i] = sum / inputChannels;
					}
					else // Multi-channel to another multi-channel format
					{
						var nearestInputChannel = (int)Math.Round((float)j / outputChannels * inputChannels);
						var idx = i * outputChannels;
						outputAudio[idx + j] = inputAudio[(i * inputChannels) + nearestInputChannel];
					}
				}
			}

			return returnSize;
		}
	}
}


