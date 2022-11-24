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
	public sealed class NetworkedDrive : IDrive
	{
		private readonly RhubarbAPIClient.FolderCache _item;

		public NetworkedDrive(RhubarbAPIClient.FolderCache item, Engine engine) {
			_item = item;
			Engine = engine;
		}

		public bool IsUserFolder => _item.ID == Engine.netApiManager.Client.User?.Id;

		public Engine Engine { get; }

		public string Path => string.Empty;

		public string Name { get => _item.Name; set => _item.Name = value; }

		public long UsedBytes => IsUserFolder ? Engine.netApiManager.Client.User.UsedBytes : -1;

		public long TotalBytes => IsUserFolder ? Engine.netApiManager.Client.User.TotalBytes : -1;

		public IFolder Root => new NetworkedFolder(_item, this);
	}
}
