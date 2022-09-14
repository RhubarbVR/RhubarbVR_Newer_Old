using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Managers;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.AssetSystem.AssetProtocals
{
	public class FtpFtpsProtocol : IAssetProtocol
	{
		public string[] Schemes => new string[] {"ftp","ftps"};

		public AssetManager Manager;

		public FtpFtpsProtocol(AssetManager assetManager) {
			Manager = assetManager;
		}

		public async Task<byte[]> ProccessAsset(Uri uri, Action<float> ProgressUpdate = null) {
			RLog.Info("Loading asset URL:" + uri);
			var HttpClientHandler = new HttpClientHandler {
				AllowAutoRedirect = true,
			};
			var request = (FtpWebRequest)WebRequest.Create(uri);
			request.Method = WebRequestMethods.Ftp.DownloadFile;
			request.Credentials = new NetworkCredential("anonymous", "rhubarbUser@RhubarbVR.net");
			RLog.Info("Client");
			var response = (FtpWebResponse)request.GetResponse();
			if (response.StatusCode == FtpStatusCode.CommandOK) {
				var bytearray = new byte[response.ContentLength];
				await response.GetResponseStream().ReadAsync(bytearray,0,bytearray.Length);
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
