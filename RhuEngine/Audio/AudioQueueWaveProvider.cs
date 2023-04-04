using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

using RhuEngine.Linker;

namespace RhuEngine
{
	public sealed class AudioQueueWaveProvider : IWaveProvider
	{
		private readonly ConcurrentQueue<byte[]> _audioQueue = new();

		public WaveFormat WaveFormat { get; }

		public int DropExtra { get; set; } = 4;

		public void Enqueue(byte[] data) {
			if (DropExtra <= 0) {
				if (Count > DropExtra) {
					_audioQueue.Clear();
				}
			}
			_audioQueue.Enqueue(data);
		}

		public void Clear() {
			_audioQueue.Clear();
		}

		public int Count => _audioQueue.Count;

		public bool IsEmpty => _audioQueue.IsEmpty;

		public int lastReadAmount = 0;

		public AudioQueueWaveProvider(WaveFormat waveFormat) {
			WaveFormat = waveFormat;
		}

		public int Read(byte[] buffer, int offset, int count) {
			var totalAmountRead = 0;

			while (totalAmountRead < count && _audioQueue.TryDequeue(out var pcmData)) {
				var amountRead = Math.Min(count - totalAmountRead, pcmData.Length);
				Buffer.BlockCopy(pcmData, 0, buffer, offset + totalAmountRead, amountRead);
				totalAmountRead += amountRead;
			}

			if (totalAmountRead < count) {
				Array.Clear(buffer, offset + totalAmountRead, count - totalAmountRead);
			}

			return totalAmountRead;
		}
	}
}
