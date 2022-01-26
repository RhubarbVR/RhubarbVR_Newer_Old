using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Managers;

using StereoKit;

namespace RhuEngine.AssetSystem.AssetProtocals
{
	public class HttpHttpsProtocol : IAssetProtocol
	{
		public string[] Schemes => new string[] {"http","https"};

		public AssetManager Manager;

		public HttpHttpsProtocol(AssetManager assetManager) {
			Manager = assetManager;
		}

		public async Task<byte[]> ProccessAsset(Uri uri) {
			Log.Info("Loading asset URL:" + uri);
			using var client = new HttpClient();
			Log.Info("Client");
			using var response = await client.GetAsync(uri);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				var bytearray = await response.Content.ReadAsByteArrayAsync();
				return bytearray;
			}
			else {
				return null;
			}
		}

		public void UploadAsset(Uri uri, byte[] data) {
			throw new NotImplementedException();
		}
	}
}
