using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace RhuEngine
{
	public sealed class AudioConverter : IWaveProvider
	{
		public WaveFormat WaveFormat { get; }

		public IWaveProvider InputStream { get; }

		public AudioConverter(WaveFormat waveFormat, IWaveProvider inputStream) {
			WaveFormat = waveFormat;
			InputStream = inputStream;
		}

		public unsafe int Read(byte[] buffer, int offset, int count) {
			if (WaveFormat.Encoding != InputStream.WaveFormat.Encoding) {
				throw new Exception($"Do not know how to convert From{InputStream.WaveFormat} to{WaveFormat}");
			}
			if (WaveFormat.SampleRate != InputStream.WaveFormat.SampleRate) {
				throw new Exception($"Do not know how to convert From{InputStream.WaveFormat} to{WaveFormat}");
			}
			if (WaveFormat.BitsPerSample != InputStream.WaveFormat.BitsPerSample) {
				throw new Exception($"Do not know how to convert From{InputStream.WaveFormat} to{WaveFormat}");
			}
			if (WaveFormat.Channels != InputStream.WaveFormat.Channels) {
				var amoutBytesPerSample = WaveFormat.BitsPerSample / 8;
				var amountSizeWrite = amoutBytesPerSample * WaveFormat.Channels;
				var amountSizeRead = amoutBytesPerSample * InputStream.WaveFormat.Channels;
				var amoutFrames = count / amountSizeWrite;
				var buffers = new byte[amoutFrames * amountSizeRead];
				var amoutRead = InputStream.Read(buffers, 0, buffers.Length) / amountSizeRead;
				fixed (byte* readBuffer = buffers) {
					fixed (byte* writebuffer = buffer) {
						for (var i = 0; i < amoutRead; i++) {
							var readSample = readBuffer + (i * amountSizeRead);
							var writeSample = writebuffer + (i * amountSizeWrite);
							for (var c = 0; c < WaveFormat.Channels; c++) {
								var writingSample = writeSample + (c * amountSizeWrite);
								for (var d = 0; d < amoutBytesPerSample; d++) {
									writingSample[d] = readSample[d];
								}
							}
						}
					}
				}
				return count;
			}
			else {
				return InputStream.Read(buffer, offset, count);
			}
		}
	}
}
