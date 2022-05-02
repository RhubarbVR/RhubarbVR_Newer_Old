using System;
using System.Runtime.Serialization;

using RhuEngine.Managers;

namespace RhuEngine
{
	[Serializable]
	internal class ConnectToServerError : Exception
	{
		public ConnectToServerError(NetApiManager apiManager) {
			apiManager.UpdateCheckForInternetConnection();
		}

		public ConnectToServerError(string message) : base(message) {
		}

		public ConnectToServerError(string message, Exception innerException) : base(message, innerException) {
		}

		protected ConnectToServerError(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}
}