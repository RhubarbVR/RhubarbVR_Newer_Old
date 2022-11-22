using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient.Model;

using RhuEngine.Managers;

namespace RhuEngine
{
	public sealed class NetworkedFolder : FolderBase
	{
		public NetworkedFolder(Guid target, NetApiManager netApiManager, NetworkedFolder parrent, IDrive networkedDrive) {
			_target = target;
			_netApiManager = netApiManager;
			Drive = networkedDrive;
			Parrent = parrent;
			UpdateInfo();
		}
		private readonly NetApiManager _netApiManager;

		private readonly Guid _target;

		public SyncFolder syncFolder;

		public override string Name
		{
			get => syncFolder.Name; set => Task.Run(async () => {
				await _netApiManager.Client.SetFolderName(_target, value);
				await UpdateInfoAsync();
			});
		}

		public override IFolder Parrent { get; }

		public override DateTimeOffset CreationDate => syncFolder.CreationDate;

		public override DateTimeOffset LastEdit => syncFolder.UpdateDate;

		public override IDrive Drive { get; }

		public override IFile[] Files => syncFolder.Files.Select(x => new NetworkedFile(x.Id, _netApiManager, this, Drive)).ToArray();

		public override IFolder[] Folders => syncFolder.Folders.Select(x => new NetworkedFolder(x.Id, _netApiManager, this, Drive)).ToArray();

		private void UpdateInfo() {
			Task.Run(UpdateInfoAsync);
		}
		private async Task UpdateInfoAsync() {
			var data = await _netApiManager.Client.GetFolder(_target);
			syncFolder = data.Data;
		}

		public override Task Refresh() {
			return UpdateInfoAsync();
		}
	}
}
