using System;
using System.Collections.Generic;
using System.Text;

namespace RhubarbCloudClient.Model
{
	public class ServerResponse<T>
	{
		public T Data { get; set; }
		public bool Error { get; set; }
		public string MSG { get; set; }
		public ServerResponse(string error) {
			MSG = error;
			Error = true;
		}
		public ServerResponse(T data) {
			Data = data;
		}

		public ServerResponse() { }
	}
}
