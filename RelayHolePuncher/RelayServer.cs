using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using LiteNetLib;

using SharedModels;
using SharedModels.GameSpecific;

namespace RelayHolePuncher
{
	public sealed class UserConnection
	{
		public ushort index;
		public NetPeer Peer { get; set; }

		public List<byte[]> waitingData = new();

		public UserConnection(NetPeer peer) {
			Peer = peer;
		}

		public UserConnection otherConnection;
	}

	public class RelayServer : INetworkingServer
	{
		public NetManager _relay;

		private readonly Dictionary<string, UserConnection> _waitingConections = new();

		private void ProcessConnection(NetPeer peer, string key) {
			if (peer.Tag is string) {
				peer.Tag = new List<UserConnection>();
			}
			var nee = new UserConnection(peer);
			if (_waitingConections.TryGetValue(key, out var userConnection)) {
				nee.otherConnection = userConnection;
				userConnection.otherConnection = nee;
				foreach (var item in userConnection.waitingData) {
					using var memstream = new MemoryStream();
					using var reader = new BinaryWriter(memstream);
					RelayNetPacked.Serlize(reader, new DataPacked(item, userConnection.index));
					nee.Peer.Send(memstream.ToArray(), DeliveryMethod.ReliableOrdered);
				}
				nee.waitingData = null;
				userConnection.waitingData = null;
				_waitingConections.Remove(key);
			}
			else {
				_waitingConections.Add(key, nee);
			}
			((List<UserConnection>)peer.Tag).Add(nee);
			nee.index = (ushort)(((List<UserConnection>)peer.Tag).Count - 1);
		}

		public void Initialize(int port) {
			var clientListener = new EventBasedNetListener();

			clientListener.NetworkReceiveEvent += ClientListener_NetworkReceiveEvent;

			clientListener.ConnectionRequestEvent += request => {
				if (!request.Data.TryGetString(out var data)) {
					Console.WriteLine("Connection Relay Rejected not string");
					request.Reject();
				}
				else if (!LiteNetLibService.CheckIfValidGUID(data)) {
					Console.WriteLine("Connection Relay Rejected not valid guid");
					request.Reject();
				}
				else {
					Console.WriteLine($"Connection Relay Accept Tag {data}");
					var peer = request.Accept();
					peer.Tag = data;
					Console.WriteLine("PeerConnected to Relay server: " + peer.EndPoint + " Tag " + peer.Tag);
					Task.Run(() => {
						try {
							ProcessConnection(peer, (string)peer.Tag);
						}
						catch (Exception e) {
							Console.WriteLine($"Error with relay connection {e}");
						}
					});
				}
			};

			clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
				Console.WriteLine("PeerDisconnected from RelayServer: " + disconnectInfo.Reason);
				if (disconnectInfo.AdditionalData.AvailableBytes > 0) {
					Console.WriteLine("Disconnect data RelayServer: " + disconnectInfo.AdditionalData.GetInt());
				}
			};

			_relay = new NetManager(clientListener) {
				IPv6Mode = IPv6Mode.DualMode
			};
			_relay.Start(port);
			_relay.MaxConnectAttempts = 64;
			_relay.ChannelsCount = 3;
			_relay.DisconnectTimeout = 60000;
			_relay.ReuseAddress = true;
			_relay.UpdateTime = 25;
			_relay.UnsyncedDeliveryEvent = true;
			_relay.UnsyncedEvents = true;
			_relay.UnsyncedReceiveEvent = true;
			_relay.AutoRecycle = true;
			Console.WriteLine($"Started Relay Server on port {port}");
		}

		public void Kill() {
			_relay.Stop();
		}
		private void ClientListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod) {
			try {
				var data = reader.GetRemainingBytes();
				if (peer.Tag is List<UserConnection> userconections) {
					using var memstream = new MemoryStream();
					using var readere = new BinaryReader(memstream);
					var relayPacked = RelayNetPacked.DeSerlize(readere);
					if (relayPacked is not null) {
						if (relayPacked is ConnectToAnotherUser connectToAnotherUser) {
							ProcessConnection(peer, connectToAnotherUser.Key);
						}
						else if (relayPacked is DataPacked packed) {
							if (userconections.Count > packed.Id - 1) {
								if (userconections[packed.Id - 1].waitingData is not null) {
									if (deliveryMethod == DeliveryMethod.ReliableOrdered) {
										userconections[packed.Id - 1].waitingData.Add(packed.Data);
									}
								}
								else {
									using var memstreame = new MemoryStream();
									using var wtiter = new BinaryWriter(memstreame);
									RelayNetPacked.Serlize(wtiter, new DataPacked(packed.Data, userconections[packed.Id - 1].otherConnection.index));
									userconections[packed.Id - 1].otherConnection.Peer.Send(memstreame.ToArray(), channel, deliveryMethod);
								}
							}
						}
						else if (relayPacked is StreamDataPacked streampacked) {
							foreach (var item in userconections) {
								try {
									using var memstreamr = new MemoryStream();
									using var wtiter = new BinaryWriter(memstreamr);
									RelayNetPacked.Serlize(wtiter, new DataPacked(data, item.otherConnection.index));
									item.otherConnection?.Peer.Send(memstreamr.ToArray(), channel, deliveryMethod);
								}
								catch (Exception e) {
									Console.WriteLine("Failed to send to user" + e.ToString());
								}
							}
						}
					}
					else {
						foreach (var item in userconections) {
							try {
								if (item.waitingData is not null) {
									if (deliveryMethod == DeliveryMethod.ReliableOrdered) {
										item.waitingData.Add(data);
									}
								}
								else {
									using var memstreama = new MemoryStream();
									using var wtiter = new BinaryWriter(memstreama);
									RelayNetPacked.Serlize(wtiter, new DataPacked(data, item.otherConnection.index));
									item.otherConnection.Peer.Send(memstreama.ToArray(), channel, deliveryMethod);
								}
							}
							catch (Exception e) {
								Console.WriteLine("Failed to send to user" + e.ToString());
							}
						}
					}
				}
			}
			catch (Exception ex) {
				Console.WriteLine("error with relay resive error:" + ex.ToString());
			}
		}
	}
}
