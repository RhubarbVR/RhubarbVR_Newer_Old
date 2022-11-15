using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RNumerics
{
	public sealed class ConcatenatedStream : Stream
	{
		readonly Queue<Stream> _streams;
		public ConcatenatedStream() {
			_streams = new Queue<Stream>();
		}

		public ConcatenatedStream(IEnumerable<Stream> streams) {
			_streams = new Queue<Stream>(streams);
		}

		public void Enqueue(Stream stream) {
			_streams.Enqueue(stream);
		}

		public void Dequeue() {
			_streams.Dequeue();
		}

		public override bool CanRead => true;

		public override int Read(byte[] buffer, int offset, int count) {
			var totalBytesRead = 0;

			while (count > 0 && _streams.Count > 0) {
				var bytesRead = _streams.Peek().Read(buffer, offset, count);
				if (bytesRead == 0) {
					_streams.Dequeue().Dispose();
					continue;
				}

				totalBytesRead += bytesRead;
				offset += bytesRead;
				count -= bytesRead;
			}

			return totalBytesRead;
		}

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override void Flush() {
			throw new NotImplementedException();
		}

		public override long Length => throw new NotImplementedException();

		public override long Position
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotImplementedException();
		}

		public override void SetLength(long value) {
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotImplementedException();
		}
	}
}
