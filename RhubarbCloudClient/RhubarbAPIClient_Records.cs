using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

		public async Task<ServerResponse<CreateRecordResponses>> UploadRecord(Stream data, string ContentType, bool publicDataStaticURL = true, bool publicData = true, ProgressTracker progress = null) {
			var size = data.Length;
			var req = await CreateRecord(size, publicData, publicDataStaticURL, ContentType);
			if (!req.Error) {
				var stream = new ProgressableStreamContent(data, progress);
				stream.Headers.Add("Content-Type", ContentType);
				stream.Headers.Add("x-upload-content-length", size.ToString());
				if (publicDataStaticURL) {
					stream.Headers.Add("X-Goog-Acl", "public-read");
					stream.Headers.Add("Cache-Control", "public, max-age=31540000");

				}
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

		public async Task<ServerResponse<CreateRecordResponses>> UploadRecordGroup(Guid group,Stream data, string ContentType, bool publicDataStaticURL = true, bool publicData = true, ProgressTracker progress = null) {
			var size = data.Length;
			var req = await CreateRecordGroup(group,size, publicData, publicDataStaticURL, ContentType);
			if (!req.Error) {
				var stream = new ProgressableStreamContent(data, progress);
				stream.Headers.Add("Content-Type", ContentType);
				stream.Headers.Add("x-upload-content-length", size.ToString());
				if (publicDataStaticURL) {
					stream.Headers.Add("X-Goog-Acl", "public-read");
					stream.Headers.Add("Cache-Control", "public, max-age=31540000");

				}
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

		public async Task<ServerResponse<CreateRecordResponses>> CreateRecordGroup(Guid group,long sizeInBytes, bool publicData, bool publicDataStaticURL, string ContentType) {
			var create = await SendPostServerResponses<CreateRecordResponses, RCreateRecord>(API_PATH + RECPATH + "CreateRecordGroup/" + group.ToString(), new RCreateRecord {
				ContentType = ContentType,
				Public = publicData,
				PublicStaticURL= publicDataStaticURL,
				SizeInBytes = sizeInBytes,
			});
			await UpdateGroupRecords(group);
			return create;
		}

		public async Task<ServerResponse<CreateRecordResponses>> CreateRecord(long sizeInBytes, bool publicData, bool publicDataStaticURL, string ContentType) {
			var create = await SendPostServerResponses<CreateRecordResponses, RCreateRecord>(API_PATH + RECPATH + "CreateRecord", new RCreateRecord {
				ContentType = ContentType,
				Public = publicData,
				PublicStaticURL = publicDataStaticURL,
				SizeInBytes = sizeInBytes,
			});
			await UpdateRecords();
			return create;
		}

		public readonly Dictionary<Guid,List<RecordResponses>> GroupRecordResponses = new();

		public readonly List<RecordResponses> RecordResponses = new();

		public event Action RecordResponsesUpdate;
		public event Action<Guid> RecordResponsesUpdateGroup;

		public async Task LoadStartDataGroups() {
			await UpdateRecords();
		}


		public async Task<ServerResponse<RecordResponses[]>> GetRecords() {
			return await SendGetServerResponses<RecordResponses[]>(API_PATH + RECPATH + "GetRecords");
		}

		public async Task<ServerResponse<RecordResponses[]>> GetRecordsGroup(Guid group) {
			return await SendGetServerResponses<RecordResponses[]>(API_PATH + RECPATH + "GetRecordsGroup/" + group.ToString());
		}

		public async Task UpdateRecords() {
			var data = await GetRecords();
			if (data.Error) {
				return;
			}
			RecordResponses.Clear();
			RecordResponses.AddRange(data.Data);
			RecordResponsesUpdate?.Invoke();
		}

		public async Task UpdateGroupRecords(Guid group) {
			var data = await GetRecordsGroup(group);
			if (data.Error) {
				return;
			}
			if(GroupRecordResponses.TryGetValue(group,out var ldata)) {
				ldata.Clear();
				ldata.AddRange(data.Data);
				RecordResponsesUpdateGroup?.Invoke(group);
			}
			else {
				GroupRecordResponses.Add(group, data.Data.ToList());
				RecordResponsesUpdateGroup?.Invoke(group);
			}
		}

		public async Task<ServerResponse<RecordAccessResponses>> GetRecordAccesses(Guid target) {
			return await SendGetServerResponses<RecordAccessResponses>(API_PATH + RECPATH + target.ToString() + "/GetRecord");
		}

		public async Task<ServerResponse<SyncFile[]>> GetRecordFiles(Guid target) {
			return await SendGetServerResponses<SyncFile[]>(API_PATH + RECPATH + "GetRecordFiles/" + target.ToString());
		}

		public async Task<Uri> GetRecordDownloadURL(Guid target) {
			var req = await GetRecordAccesses(target);
			return req.Error ? null : new Uri(req.Data.TempURL);
		}
	}
}
