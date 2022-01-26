using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.AssetSystem
{
	public interface IAssetProtocol
	{
		public string[] Schemes { get; }

		public Task<byte[]> ProccessAsset(Uri uri);

		public void UploadAsset(Uri uri,byte[] data);
	}
}
