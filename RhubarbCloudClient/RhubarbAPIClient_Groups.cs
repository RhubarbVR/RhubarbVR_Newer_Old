using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using DataModel.Enums;

using Microsoft.AspNetCore.SignalR.Client;

using Newtonsoft.Json;

using RhubarbCloudClient.Model;

namespace RhubarbCloudClient
{
	public sealed partial class RhubarbAPIClient : IDisposable
	{

		private async Task LoadStartGroups() {
			//var req = await SendGetServerResponses<UserDM[]>(API_PATH + DMSPATH + "DMs");

		}


		public bool IsPartOfGroup(string data) {
			return Guid.TryParse(data, out var group) && IsPartOfGroup(group);
		}
		public bool IsPartOfGroup(Guid groupID) {
			return false;
		}
	}
}
