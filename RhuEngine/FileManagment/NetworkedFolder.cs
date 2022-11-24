using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;
using RhubarbCloudClient.Model;

using RhuEngine.Managers;

namespace RhuEngine
{
	public sealed class NetworkedFolder : FolderBase
	{
		private readonly RhubarbAPIClient.FolderCache _item;
		private readonly NetworkedDrive _networkedDrive;

		public NetworkedFolder(RhubarbAPIClient.FolderCache item, NetworkedDrive networkedDrive) {
			_item = item;
			_networkedDrive = networkedDrive;
		}

		public override string Name { get => _item.Name; set => _item.Name = value; }

		public override IFolder Parrent => _item.ParrentFolder is null ? null : (IFolder)new NetworkedFolder(_item.ParrentFolder, _networkedDrive);

		public override DateTimeOffset CreationDate => _item.CreationDate;

		public override DateTimeOffset LastEdit => _item.UpdateData;

		public override IDrive Drive => _networkedDrive;

		public override IFile[] Files => _item.Files().Select(x => new NetworkedFile(x, _networkedDrive)).ToArray();

		public override IFolder[] Folders => _item.Folders().Select(x => new NetworkedFolder(x, _networkedDrive)).ToArray();

		public override Task Refresh() {
			return _item.Refresh();
		}
	}
}
