using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public static class StaticData
	{
		public static string GetPublicData(Guid target) {
			return $"https://storage.googleapis.com/rhubarbvr_bucket/{target}";
		}
	}
}
