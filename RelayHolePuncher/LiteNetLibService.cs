using System;
using System.Threading;
using System.Threading.Tasks;

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
			relayServer.Initialize(7857
				);
		}

		public async Task StartUpdateLoop() {
			while (punchServer._puncher.IsRunning && relayServer._relay.IsRunning) {
				punchServer._puncher.PollEvents();
				punchServer._puncher.NatPunchModule.PollEvents();
				relayServer._relay.PollEvents();
				await Task.Delay(10);
			}
		}

		public static bool CheckIfValidGUID(string guid) {
			return Guid.TryParse(guid, out _);
		}
	}
}
