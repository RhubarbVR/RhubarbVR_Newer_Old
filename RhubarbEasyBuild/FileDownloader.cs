using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RhubarbEasyBuild
{
	public sealed class FileDownloader
	{
		private readonly string _url;
		private readonly string _fullPathWhereToSave;
		private readonly SemaphoreSlim _semaphore = new(0);

		public FileDownloader(string url, string fullPathWhereToSave) {
			if (string.IsNullOrEmpty(url)) {
				throw new ArgumentNullException(nameof(url));
			}

			if (string.IsNullOrEmpty(fullPathWhereToSave)) {
				throw new ArgumentNullException(nameof(fullPathWhereToSave));
			}

			_url = url;
			_fullPathWhereToSave = fullPathWhereToSave;
		}

		public async Task<bool> StartDownload() {
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(_fullPathWhereToSave));

				if (File.Exists(_fullPathWhereToSave)) {
					File.Delete(_fullPathWhereToSave);
				}

				using (var client = new HttpClient()) {
					using var response = await client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead);
					response.EnsureSuccessStatusCode();

					using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
					using Stream streamToWriteTo = File.Open(_fullPathWhereToSave, FileMode.Create);
					var buffer = new byte[81920];
					int bytesRead;
					long totalBytesRead = 0;

					while (totalBytesRead != response.Content.Headers.ContentLength.Value) {
						bytesRead = await streamToReadFrom.ReadAsync(buffer);
						await streamToWriteTo.WriteAsync(buffer.AsMemory(0, bytesRead));
						totalBytesRead += bytesRead;
						Console.Write($"\r     -->    {(double)totalBytesRead / response.Content.Headers.ContentLength.Value * 100:F2}%");
					}
				}

				Console.WriteLine(Environment.NewLine + "Download finished!");
				return true;
			}
			catch (Exception e) {
				Console.WriteLine("Was not able to download file!");
				Console.Write(e);
				return false;
			}
			finally {
				_semaphore.Dispose();
			}
		}
	}
}