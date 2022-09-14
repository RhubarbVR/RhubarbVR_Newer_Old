using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using Newtonsoft.Json;

using RhubarbCloudClient.HttpContent;
using RhubarbCloudClient.Model;

namespace RhubarbCloudClient
{
	public sealed partial class RhubarbAPIClient : IDisposable
	{
		public const string RECPATH = "records/";

		public async Task<ServerResponse<CreateRecordResponses>> UploadRecord(Stream data, string ContentType, bool publicData = true, ProgressTracker progress = null) {
			var size = data.Length;
			var req = await CreateRecord(size, publicData, ContentType);
			if (!req.Error) {
				var stream = new ProgressableStreamContent(data, progress);
				var httpResponse = await HttpClient.PutAsync(new Uri(req.Data.TempUploadURL), stream);
				if (!httpResponse.IsSuccessStatusCode) {
					req.Error = true;
					req.MSG = await httpResponse.Content.ReadAsStringAsync();
					stream.ProgressTracker?.ChangeState(ProgressState.Failed);
					return req;
				}
				else {
					stream.ProgressTracker?.ChangeState(ProgressState.Done);
					return req;
				}
			}
			else {
				return req;
			}
		}

		public async Task<ServerResponse<CreateRecordResponses>> CreateRecord(long sizeInBytes, bool publicData, string ContentType) {
			return await SendPostServerResponses<CreateRecordResponses, RCreateRecord>(API_PATH + RECPATH + "CreateRecord", new RCreateRecord {
				ContentType = ContentType,
				Public = publicData,
				SizeInBytes = sizeInBytes,
			});
		}

		public async Task<ServerResponse<RecordResponses[]>> GetRecords() {
			return await SendGetServerResponses<RecordResponses[]>(API_PATH + RECPATH + "GetRecords");
		}

		public async Task<ServerResponse<RecordAccessResponses>> GetRecordAccesses(Guid target) {
			return await SendGetServerResponses<RecordAccessResponses>(API_PATH + RECPATH + target.ToString() + "/GetRecord");
		}

		public async Task<Uri> GetRecordDownloadURL(Guid target) {
			var req = await GetRecordAccesses(target);
			return req.Error ? null : new Uri(req.Data.TempURL);
		}
	}
}
