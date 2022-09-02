using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RhubarbCloudClient;

using RhuEngine.Linker;


namespace RhuEngine.Managers
{
	public class NetApiManager : IManager
	{
		public RhubarbAPIClient Client { get; private set; }

		public NetApiManager(string path) {
			Client = new RhubarbAPIClient(RhubarbAPIClient.BaseUri,path);
		}

		public void Init(Engine engine) {
		}

		public void Step() {
		}

		public void RenderStep() {
		}

		public void Dispose() {
		}
	}
}
