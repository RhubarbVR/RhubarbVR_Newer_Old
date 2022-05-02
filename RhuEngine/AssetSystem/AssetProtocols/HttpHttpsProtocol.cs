using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Managers;
using RhuEngine.Linker;
using System.IO;

namespace RhuEngine.AssetSystem.AssetProtocals
{
	public class HttpHttpsProtocol : IAssetProtocol
	{
		public string[] Schemes => new string[] {"http","https"};

		public AssetManager Manager;

		public HttpHttpsProtocol(AssetManager assetManager) {
			Manager = assetManager;
		}

		public async Task<byte[]> ProccessAsset(Uri uri, Action<float> ProgressUpdate = null) {
			RLog.Info("Loading asset URL:" + uri);
			var HttpClientHandler = new HttpClientHandler {
				AllowAutoRedirect = true,
			};
			using var client = new HttpClient(HttpClientHandler);
			HttpClientHandler.ServerCertificateCustomValidationCallback = NetApiManager.ValidateRemoteCertificate;
			using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode();
			var totalBytes = response.Content.Headers.ContentLength;
			using var contentStream = await response.Content.ReadAsStreamAsync();
			RLog.Info($"Downloading asset with {totalBytes} bytes");
			return await ProcessContentStream(totalBytes, contentStream, ProgressUpdate);
		}

		private async Task<byte[]> ProcessContentStream(long? totalDownloadSize, Stream contentStream, Action<float> ProgressChanged) {
			var totalBytesRead = 0L;
			var readCount = 0L;
			var buffer = new byte[8192];
			using var mem = new MemoryStream((int)(totalDownloadSize ?? 0));
			var isMoreToRead = true;
				do {
					var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead == 0) {
						isMoreToRead = false;
						TriggerProgressChanged(totalDownloadSize, totalBytesRead, ProgressChanged);
						continue;
					}

					await mem.WriteAsync(buffer, 0, bytesRead);

					totalBytesRead += bytesRead;
					readCount += 1;

					if (readCount % 100 == 0) {
					TriggerProgressChanged(totalDownloadSize, totalBytesRead, ProgressChanged);
				}
			}
			while (isMoreToRead);
			return mem.ToArray();
		}

		private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead,Action<float> ProgressChanged) {
			if (ProgressChanged == null) {
				return;
			}

			double? progressPercentage = null;
			if (totalDownloadSize.HasValue) {
				progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);
			}

			ProgressChanged((float)progressPercentage);
			;
		}



		public void UploadAsset(Uri uri, byte[] data) {
			throw new NotImplementedException();
		}
	}
}
