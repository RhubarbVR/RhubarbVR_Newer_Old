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
	public sealed class NetworkedFile : FileBase
	{
		public NetworkedFile(Guid target, NetApiManager netApiManager, NetworkedFolder parrent, IDrive networkedDrive) {
			_target = target;
			_netApiManager = netApiManager;
			Drive = networkedDrive;
			Parrent = parrent;
			UpdateInfo();
		}
		private readonly NetApiManager _netApiManager;

		private readonly Guid _target;

		public SyncFile syncFile;

		public override string Name
		{
			get => syncFile.Name; set => Task.Run(async () => {
				await _netApiManager.Client.SetFolderName(_target, value);
				await UpdateInfoAsync();
			});
		}

		public override IFolder Parrent { get; }

		public override DateTimeOffset CreationDate => syncFile.CreationDate;

		public override DateTimeOffset LastEdit => syncFile.UpdateDate;

		public override IDrive Drive { get; }

		public override long SizeInBytes => throw new NotImplementedException();

		private void UpdateInfo() {
			Task.Run(UpdateInfoAsync);
		}
		private async Task UpdateInfoAsync() {
			syncFile = (await _netApiManager.Client.GetFile(_target)).Data;
		}

		public override void Open() {

		}
	}
}
