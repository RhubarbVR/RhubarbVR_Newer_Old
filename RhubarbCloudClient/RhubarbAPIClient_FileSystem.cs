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
		public const string FILEPATH = "filesystem/";
		public async Task<ServerResponse<bool>> DeleteFile(Guid target) {
			return await SendGetServerResponses<bool>(API_PATH + FILEPATH + "DeleteFile/" + target.ToString());
		}
		public async Task<ServerResponse<bool>> DeleteFolder(Guid target) {
			return await SendGetServerResponses<bool>(API_PATH + FILEPATH + "DeleteFolder/" + target.ToString());
		}

		public async Task<ServerResponse<SyncFile>> GetFile(Guid target) {
			return await SendGetServerResponses<SyncFile>(API_PATH + FILEPATH + "GetFile/" + target.ToString());
		}

		public async Task<ServerResponse<SyncFolder>> GetFolder(Guid target) {
			return await SendGetServerResponses<SyncFolder>(API_PATH + FILEPATH + "GetFolder/" + target.ToString());
		}

		public async Task<ServerResponse<SyncFolder[]>> GetRootFolders() {
			return await SendGetServerResponses<SyncFolder[]>(API_PATH + FILEPATH + "GetRootFolders");
		}

		public async Task<ServerResponse<SyncFolder>> CreateFolder(Guid parrentFolder, string name) {
			return await SendPostServerResponses<SyncFolder, NameData>(API_PATH + FILEPATH + "CreateFolder/" + parrentFolder.ToString(), new NameData { Name = name });
		}

		public async Task<ServerResponse<SyncFile>> CreateFile(Guid parrentFolder, string nameOfFile, Guid thumnail, Guid mainRecordId, IEnumerable<Guid> allOtherRecords) {
			return await SendPostServerResponses<SyncFile, CreateFile>(API_PATH + FILEPATH + "CreateFile/" + parrentFolder.ToString(), new CreateFile {
				Name = nameOfFile,
				AllOtherRecords = allOtherRecords.ToArray(),
				Thumbnail = thumnail,
				MainRecordId = mainRecordId
			});
		}

		public async Task<ServerResponse<bool>> SetFileName(Guid TargetID, string name) {
			return await SendPostServerResponses<bool, NameData>(API_PATH + FILEPATH + "SetFileName/" + TargetID.ToString(), new NameData { Name = name });
		}
		public async Task<ServerResponse<bool>> SetFolderName(Guid TargetID, string name) {
			return await SendPostServerResponses<bool, NameData>(API_PATH + FILEPATH + "SetFolderName/" + TargetID.ToString(), new NameData { Name = name });
		}
	}
}
