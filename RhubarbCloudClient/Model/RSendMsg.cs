using System;
using System.Collections.Generic;
using System.Text;

using DataModel.Enums;

namespace RhubarbCloudClient.Model
{
	public sealed class RSendMsg
	{
		public MessageType MessageType { get; set; }
		public string Data { get; set; }
	}
}
