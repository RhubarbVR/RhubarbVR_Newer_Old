using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

using LiteNetLib;

namespace RelayHolePuncher
{
	public sealed class HolePunchServer : INetworkingServer, INatPunchListener
	{
		private readonly ConcurrentDictionary<string, WaitPeer> _waitingPeers = new();
		public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token) {
			if (_waitingPeers.TryGetValue(token, out var wpeer)) {
				Console.WriteLine("Wait peer found, sending introduction...");

				//found in list - introduce client and host to eachother
				Console.WriteLine(
					"host - i({0}) e({1})\nclient - i({2}) e({3})",
					wpeer.InternalAddr,
					wpeer.ExternalAddr,
					localEndPoint,
					remoteEndPoint);

				_puncher.NatPunchModule.NatIntroduce(
					wpeer.InternalAddr, // host internal
					wpeer.ExternalAddr, // host external
					localEndPoint, // client internal
					remoteEndPoint, // client external
					token // request token
					);
				_waitingPeers.Remove(token, out _);
			}
			else {
				Console.WriteLine("Wait peer created. i({0}) e({1})", localEndPoint, remoteEndPoint);
				_waitingPeers[token] = new WaitPeer(localEndPoint, remoteEndPoint);
			}
		}

		public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token) {
			//Ignore we are server
		}

		public NetManager _puncher;
		public void Initialize(int port) {
			var clientListener = new EventBasedNetListener();

			clientListener.PeerConnectedEvent += peer => Console.WriteLine("HolePunch PeerConnected to server: " + peer.EndPoint);

			clientListener.ConnectionRequestEvent += request => request.Accept();

			clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
				Console.WriteLine("HolePunch PeerDisconnected from server: " + disconnectInfo.Reason);
				if (disconnectInfo.AdditionalData.AvailableBytes > 0) {
					Console.WriteLine("HolePunch Disconnect data: " + disconnectInfo.AdditionalData.GetInt());
				}
			};

			_puncher = new NetManager(clientListener) {
				IPv6Enabled = IPv6Mode.DualMode,
				NatPunchEnabled = true
			};
			Console.WriteLine($"Started HolePunchServer on port {port}");
			_puncher.Start(port);
			_puncher.MaxConnectAttempts = 64;
			_puncher.DisconnectTimeout = 60000;
			_puncher.ReuseAddress = true;
			_puncher.UpdateTime = 25;
			_puncher.UnsyncedDeliveryEvent = true;
			_puncher.UnsyncedEvents = true;
			_puncher.UnsyncedReceiveEvent = true;
			_puncher.NatPunchModule.Init(this);
		}

		public void Kill() {
			_puncher.Stop();
		}

	}
}
