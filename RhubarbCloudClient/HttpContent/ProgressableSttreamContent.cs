using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RhubarbCloudClient.HttpContent
{
	public sealed class ProgressableStreamContent : System.Net.Http.HttpContent
	{
		private const int DEFAULT_BUFFER_SIZE = 4096;
		private readonly Stream _content;
		private readonly int _bufferSize;
		private bool _contentConsumed;
		public ProgressTracker ProgressTracker { get; private set; }

		public ProgressableStreamContent(Stream content, ProgressTracker progressTracke = null) : this(content, DEFAULT_BUFFER_SIZE, progressTracke) { }

		public ProgressableStreamContent(Stream content, int buffersize, ProgressTracker progressTracke = null) {
			if (content is null) {
				throw new ArgumentException(nameof(content));
			}
			if (buffersize <= 0) {
				throw new ArgumentOutOfRangeException(nameof(buffersize));
			}

			_content = content;
			_bufferSize = buffersize;
			ProgressTracker = progressTracke ?? new ProgressTracker();
		}

		private void PrepareContent() {
			if (_contentConsumed) {
				_content.Position = _content.CanSeek ? 0 : throw new InvalidOperationException("net_http_content_stream_already_read");
				_contentConsumed = true;
			}
		}

		protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) {
			Contract.Assert(stream != null);
			PrepareContent();
			return Task.Run(() => {
				var buffer = new byte[_bufferSize];
				var size = _content.Length;
				var uploaded = 0;
				ProgressTracker.ChangeState(ProgressState.PendingUpload);
				using (_content) {
					while (true) {
						var length = _content.Read(buffer, 0, buffer.Length);
						if (length <= 0) {
							break;
						}

						ProgressTracker.Bytes = uploaded += length;
						stream.Write(buffer, 0, length);
						ProgressTracker.ChangeState(ProgressState.Uploading);
					}
				}

				ProgressTracker.ChangeState(ProgressState.PendingResponse);
			});
		}

		protected override bool TryComputeLength(out long length) {
			length = _content.Length;
			return true;
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				_content?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
