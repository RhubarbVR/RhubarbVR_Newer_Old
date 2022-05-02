using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.AssetSystem
{
	public interface IAssetProtocol
	{
		public string[] Schemes { get; }

		public Task<byte[]> ProccessAsset(Uri uri, Action<float> ProgressUpdate = null);

		public void UploadAsset(Uri uri,byte[] data);
	}
}
