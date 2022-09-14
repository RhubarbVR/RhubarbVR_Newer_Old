using System;
using System.Threading;

using LiteNetLib;

namespace RelayHolePuncher
{
	public interface INetworkingServer
	{
		public void Initialize(int port);
	}
	public sealed class LiteNetLibService
	{
		public HolePunchServer punchServer;

		public RelayServer relayServer;

		public Thread updateThread;

		public void Initialize() {
			punchServer = new HolePunchServer();
			relayServer = new RelayServer();
			punchServer.Initialize(7856);
			relayServer.Initialize(7857);
		}

		public void StartUpdateLoop() {
			var thread = new Thread(() => { while (true) { punchServer.Update(); } });
			var thread2 = new Thread(() => { while (true) { relayServer.Update(); } });
			thread.Start();
			thread2.Start();
			thread.Join();
		}

		public static bool CheckIfValidGUID(string guid) {
			return Guid.TryParse(guid, out _);
		}
	}
}
