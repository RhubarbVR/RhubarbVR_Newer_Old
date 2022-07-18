using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels.UserSession
{
	public enum RequestType
	{
		StatusUpdate,
		CreateSession,
		JoinSession,
		UpdateSession,
		LeaveSession,
		SessionError,
		ConnectToUser,
		SessionID,
		LoadStartingStatus,
	}
	public class SessionRequest
	{
		public RequestType RequestType { get; set; }
		public string RequestData { get; set; }
		public Guid ID { get; set; }
	}
}
