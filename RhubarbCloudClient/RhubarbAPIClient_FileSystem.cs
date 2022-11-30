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

		public sealed class FolderCache
		{
			private readonly RhubarbAPIClient _rhubarbAPIClient;
			public Guid? ParrentFolderID { get; private set; }

			public Guid ID { get; private set; }

			public readonly List<Guid> FolderIDs = new();
			public readonly List<Guid> FileIDs = new();


			private string _name;
			private DateTimeOffset _creationDate;
			private DateTimeOffset _updateDate = DateTimeOffset.MinValue;
			public FolderCache ParrentFolder => _rhubarbAPIClient.TryToGetFolder(ParrentFolderID ?? Guid.Empty, out var folder) ? folder : null;

			public DateTimeOffset CreationDate => _creationDate;
			public DateTimeOffset UpdateData => _updateDate;

			public string Name
			{
				get => _name;
				set {
					_name = value;
					Task.Run(async () => {
						await _rhubarbAPIClient.ReqSetFolderName(ID, _name);
						await Refresh();
					});
				}
			}

			public IEnumerable<FolderCache> Folders() {
				foreach (var item in FolderIDs) {
					if (_rhubarbAPIClient.TryToGetFolder(item, out var folderCache)) {
						yield return folderCache;
					}
				}
			}

			public IEnumerable<FileCache> Files() {
				foreach (var item in FileIDs) {
					if (_rhubarbAPIClient.TryToGetFile(item, out var folderCache)) {
						yield return folderCache;
					}
				}
			}

			public FolderCache(SyncFolder returnData, RhubarbAPIClient rhubarbAPIClient, bool hasSub = true) {
				_rhubarbAPIClient = rhubarbAPIClient;
				LoadInData(returnData, hasSub);
			}

			public void LoadInData(SyncFolder returnData, bool hasSub = true) {
				if (returnData.UpdateDate < _updateDate) {
					return;
				}
				ID = returnData.Id;
				_name = returnData.Name;
				_creationDate = returnData.CreationDate;
				_updateDate = returnData.UpdateDate;
				ParrentFolderID = returnData.ParrentFolderId;
				if (hasSub) {
					FolderIDs.Clear();
					foreach (var item in returnData.Folders) {
						FolderIDs.Add(item.Id);
						lock (_rhubarbAPIClient.folders) {
							if (_rhubarbAPIClient.folders.TryGetValue(item.Id, out var folderCache)) {
								folderCache.LoadInData(item, false);
							}
							else {
								folderCache = new FolderCache(item, _rhubarbAPIClient, false);
								_rhubarbAPIClient.folders.Add(item.Id, folderCache);
							}
						}
					}
					FileIDs.Clear();
					foreach (var item in returnData.Files) {
						FileIDs.Add(item.Id);
						lock (_rhubarbAPIClient.files) {
							if (_rhubarbAPIClient.files.TryGetValue(item.Id, out var fileCache)) {
								fileCache.LoadInData(item);
							}
							else {
								fileCache = new FileCache(item, _rhubarbAPIClient);
								_rhubarbAPIClient.files.Add(item.Id, fileCache);
							}
						}
					}
				}

			}

			public async Task Refresh() {
				var data = await _rhubarbAPIClient.ReqGetFolder(ID);
				if (!data.Error) {
					LoadInData(data.Data);
				}
				else {
					Console.WriteLine("Failed to refresh Folder:" + ID.ToString());
				}
			}
		}

		public sealed class FileCache
		{
			private readonly RhubarbAPIClient _rhubarbAPIClient;

			public Guid ID { get; private set; }
			public Guid? ParrentFolderID { get; private set; }

			private string _name;
			private DateTimeOffset _creationDate;
			private DateTimeOffset _updateDate = DateTimeOffset.MinValue;


			public DateTimeOffset CreationDate => _creationDate;
			public DateTimeOffset UpdateData => _updateDate;

			public Guid Thumbnail { get; private set; }
			public Guid MainRecordId { get; private set; }

			public string Type { get; private set; }

			public string Name
			{
				get => _name;
				set {
					_name = value;
					Task.Run(async () => {
						await _rhubarbAPIClient.ReqSetFileName(ID, _name);
						await Refresh();
					});
				}
			}

			public FolderCache ParrentFolder => _rhubarbAPIClient.TryToGetFolder(ParrentFolderID ?? Guid.Empty, out var folder) ? folder : null;

			public FileCache(SyncFile returnData, RhubarbAPIClient rhubarbAPIClient) {
				_rhubarbAPIClient = rhubarbAPIClient;
				LoadInData(returnData);
			}

			public void LoadInData(SyncFile returnData) {
				if (returnData.UpdateDate <= _updateDate) {
					return;
				}
				ID = returnData.Id;
				_name = returnData.Name;
				_creationDate = returnData.CreationDate;
				_updateDate = returnData.UpdateDate;
				ParrentFolderID = returnData.ParrentFolderId;
				Thumbnail = returnData.Thumbnail;
				MainRecordId = returnData.MainRecordId;
				Type = returnData.Type;
			}

			public async Task Refresh() {
				var data = await _rhubarbAPIClient.ReqGetFile(ID);
				if (!data.Error) {
					LoadInData(data.Data);
				}
				else {
					Console.WriteLine("Failed to refresh File:" + ID.ToString());
				}
			}

		}

		public readonly Dictionary<Guid, FolderCache> folders = new();

		public readonly Dictionary<Guid, FileCache> files = new();

		public bool TryToGetFolder(Guid folderId, out FolderCache folderCache) {
			lock (folders) {
				return folders.TryGetValue(folderId, out folderCache);
			}
		}

		public bool TryToGetFile(Guid folderId, out FileCache fileCache) {
			lock (files) {
				return files.TryGetValue(folderId, out fileCache);
			}
		}


		public async Task<FolderCache> GetFolder(Guid folderId) {
			lock (folders) {
				if (folders.TryGetValue(folderId, out var folderCache)) {
					return folderCache;
				}
			}
			var returnData = await ReqGetFolder(folderId);
			FolderCache newCach = null;
			lock (folders) {
				if (folders.TryGetValue(folderId, out var folderCache)) { //Check If another req with through
					return folderCache;
				}
				if (!returnData.Error) {
					newCach = new FolderCache(returnData.Data, this);
					folders.Add(folderId, newCach);
				}
			}
			return newCach;
		}

		public async Task<FileCache> GetFile(Guid fileId) {
			lock (files) {
				if (files.TryGetValue(fileId, out var fileCache)) {
					return fileCache;
				}
			}
			var returnData = await ReqGetFile(fileId);
			FileCache newCach = null;
			lock (files) {
				if (files.TryGetValue(fileId, out var fileCache)) { //Check If another req with through
					return fileCache;
				}
				if (!returnData.Error) {
					newCach = new FileCache(returnData.Data, this);
					files.Add(fileId, newCach);
				}
			}
			return newCach;
		}

		public async Task<ServerResponse<bool>> ReqDeleteFile(Guid target) {
			return await SendGetServerResponses<bool>(API_PATH + FILEPATH + "DeleteFile/" + target.ToString());
		}
		public async Task<ServerResponse<bool>> ReqDeleteFolder(Guid target) {
			return await SendGetServerResponses<bool>(API_PATH + FILEPATH + "DeleteFolder/" + target.ToString());
		}

		public async Task<ServerResponse<SyncFile>> ReqGetFile(Guid target) {
			return await SendGetServerResponses<SyncFile>(API_PATH + FILEPATH + "GetFile/" + target.ToString());
		}

		public async Task<ServerResponse<SyncFolder>> ReqGetFolder(Guid target) {
			return await SendGetServerResponses<SyncFolder>(API_PATH + FILEPATH + "GetFolder/" + target.ToString());
		}

		public async Task<ServerResponse<SyncFolder[]>> ReqGetRootFolders() {
			return await SendGetServerResponses<SyncFolder[]>(API_PATH + FILEPATH + "GetRootFolders");
		}

		public FolderCache[] RootFolders = Array.Empty<FolderCache>();

		public async Task<FolderCache[]> GetRootFolders() {
			var data = await ReqGetRootFolders();
			lock (RootFolders) {
				if (data.Error || data.Data is null) {
					RootFolders = Array.Empty<FolderCache>();
					return RootFolders;
				}
				var dataRe = new List<FolderCache>();
				foreach (var item in data.Data) {
					lock (folders) {
						if (folders.TryGetValue(item.Id, out var fdata)) {
							fdata.LoadInData(item);
							dataRe.Add(fdata);
						}
						else {
							var newCach = new FolderCache(item, this);
							folders.Add(item.Id, newCach);
							dataRe.Add(newCach);
						}
					}
				}
				RootFolders = dataRe.ToArray();
			}
			return RootFolders;
		}

		public async Task<ServerResponse<SyncFolder>> ReqCreateFolder(Guid parrentFolder, string name) {
			return await SendPostServerResponses<SyncFolder, NameData>(API_PATH + FILEPATH + "CreateFolder/" + parrentFolder.ToString(), new NameData { Name = name });
		}

		public async Task<ServerResponse<SyncFile>> ReqCreateFile(Guid parrentFolder, string nameOfFile, string type, Guid thumnail, Guid mainRecordId, IEnumerable<Guid> allOtherRecords) {
			return await SendPostServerResponses<SyncFile, CreateFile>(API_PATH + FILEPATH + "CreateFile/" + parrentFolder.ToString(), new CreateFile {
				Name = nameOfFile,
				AllOtherRecords = allOtherRecords.ToArray(),
				Thumbnail = thumnail,
				MainRecordId = mainRecordId,
				Type = type,
			});
		}

		public async Task<ServerResponse<bool>> ReqSetFileName(Guid TargetID, string name) {
			return await SendPostServerResponses<bool, NameData>(API_PATH + FILEPATH + "SetFileName/" + TargetID.ToString(), new NameData { Name = name });
		}
		public async Task<ServerResponse<bool>> ReqSetFolderName(Guid TargetID, string name) {
			return await SendPostServerResponses<bool, NameData>(API_PATH + FILEPATH + "SetFolderName/" + TargetID.ToString(), new NameData { Name = name });
		}
	}
}
