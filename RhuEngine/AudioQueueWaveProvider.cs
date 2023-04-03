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

		public WaveFormat WaveFormat { get; set; } = new WaveFormat(48000, 16, 1);

		public int DropExtra { get; set; } = 3;

		public void Enqueue(byte[] data) {
			if (Count > DropExtra) {
				_audioQueue.Clear();
			}
			_audioQueue.Enqueue(data);
		}

		public void Clear() {
			_audioQueue.Clear();
		}

		public int Count => _audioQueue.Count;

		public bool IsEmpty => _audioQueue.IsEmpty;

		public int lastReadAmount = 0;

		public int Read(byte[] buffer, int offset, int count) {
			var totalAmountRead = 0;
			if (_audioQueue.Count == 0) {
				Array.Clear(buffer, offset, count);
				return Count;
			}
			while (totalAmountRead < count && _audioQueue.TryPeek(out var pcmData)) {
				var amountRead = Math.Min(count - totalAmountRead, pcmData.Length - lastReadAmount);
				Buffer.BlockCopy(pcmData, lastReadAmount, buffer, offset + totalAmountRead, amountRead);
				if (amountRead == pcmData.Length) {
					_audioQueue.TryDequeue(out _);
					lastReadAmount = 0;
				}
				else {
					lastReadAmount = pcmData.Length - amountRead;
				}
				totalAmountRead += amountRead;
			}

			if (totalAmountRead < count) {
				Array.Clear(buffer, offset + totalAmountRead, count - totalAmountRead);
			}

			return totalAmountRead;
		}
	}
}
