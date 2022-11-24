using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;
using RhubarbCloudClient.Model;

using RhuEngine.Linker;
using RhuEngine.Managers;

namespace RhuEngine
{
	public sealed class NetworkedFile : FileBase
	{
		private readonly RhubarbAPIClient.FileCache _item;
		private readonly NetworkedDrive _networkedDrive;

		public NetworkedFile(RhubarbAPIClient.FileCache item, NetworkedDrive networkedDrive) {
			_item = item;
			_networkedDrive = networkedDrive;
		}
		public override string Name { get => _item.Name; set => _item.Name = value; }

		public override IFolder Parrent => _item.ParrentFolder is null ? null : (IFolder)new NetworkedFolder(_item.ParrentFolder, _networkedDrive);

		public override DateTimeOffset CreationDate => _item.CreationDate;

		public override DateTimeOffset LastEdit => _item.UpdateData;

		public override long SizeInBytes => 0;

		public override IDrive Drive => _networkedDrive;

		public override RTexture2D Texture => _networkedDrive.Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.File);

		public override void Open() {

		}
	}
}
