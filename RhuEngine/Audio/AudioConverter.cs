using System;

using NAudio.Wave;

namespace RhuEngine
{
	public sealed class AudioConverter : IWaveProvider
	{
		public WaveFormat WaveFormat { get; }

		public IWaveProvider InputStream { get; }

		public bool IgnoreSampleRateChanges { get; }

		private byte[] _readBuffer;

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
				_readBuffer = new byte[requiredReadBufferLength];
			}

			var bytesRead = InputStream.Read(_readBuffer, 0, requiredReadBufferLength);
			var floatData = ConvertToFloatArray(_readBuffer, InputStream.WaveFormat, bytesRead);
			floatData = ConvertChannels(floatData, InputStream.WaveFormat.Channels, WaveFormat.Channels);
			var convertedBytes = ConvertToByteArray(floatData, WaveFormat);
			Buffer.BlockCopy(convertedBytes, 0, buffer, offset, convertedBytes.Length);
			return convertedBytes.Length;
		}

		public static byte[] ConvertToByteArray(float[] floats, WaveFormat waveFormat) {
			return waveFormat.Encoding == WaveFormatEncoding.IeeeFloat
				? FloatArrayToByteArray(floats)
				: FloatArrayToByteArray(floats, waveFormat.BitsPerSample);
		}

		public static float[] ConvertToFloatArray(byte[] bytes, WaveFormat waveFormat, int bytesRead) {
			return waveFormat.Encoding == WaveFormatEncoding.IeeeFloat
				? ByteArrayToFloatArray(bytesRead, bytes)
				: ByteArrayToFloatArray(bytesRead, bytes, waveFormat.BitsPerSample);
		}


		public static byte[] FloatArrayToByteArray(float[] floatArray) {
			var bytes = new byte[floatArray.Length * sizeof(float)];
			Buffer.BlockCopy(floatArray, 0, bytes, 0, bytes.Length);
			return bytes;
		}

		public static float[] ByteArrayToFloatArray(int bytesRead, byte[] byteArray) {
			var floats = new float[bytesRead / sizeof(float)];
			Buffer.BlockCopy(byteArray, 0, floats, 0, bytesRead);
			return floats;
		}

		public static float[] ByteArrayToFloatArray(int bytesRead, byte[] byteArray, int bitsPerSample) {
			if (bitsPerSample is not 16 and not 32) {
				throw new ArgumentException("Bits per sample must be 16 or 32.");
			}

			var bytesPerSample = bitsPerSample / 8;
			var floatArrayLength = bytesRead / bytesPerSample;
			var floatArray = new float[floatArrayLength];

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

			return floatArray;
		}

		public static byte[] FloatArrayToByteArray(float[] floatArray, int bitsPerSample) {
			if (bitsPerSample is not 16 and not 32) {
				throw new ArgumentException("Bits per sample must be 16 or 32.");
			}

			var bytesPerSample = bitsPerSample / 8;
			var byteArrayLength = floatArray.Length * bytesPerSample;
			var byteArray = new byte[byteArrayLength];

			for (var i = 0; i < floatArray.Length; i++) {
				if (bitsPerSample == 16) {
					var sample = (short)(floatArray[i] * 32768); // De-normalize from the range [-1, 1]
					BitConverter.GetBytes(sample).CopyTo(byteArray, i * bytesPerSample);
				}
				else if (bitsPerSample == 32) {
					var sample = (int)(floatArray[i] * 2147483648); // De-normalize from the range [-1, 1]
					BitConverter.GetBytes(sample).CopyTo(byteArray, i * bytesPerSample);
				}
			}

			return byteArray;
		}



		public static float[] ConvertChannels(float[] inputAudio, int inputChannels, int outputChannels) {
			var inputFrameCount = inputAudio.Length / inputChannels;
			var outputFrameCount = inputFrameCount;
			var outputAudio = new float[outputFrameCount * outputChannels];

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

			return outputAudio;
		}
	}
}


