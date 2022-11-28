using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		private NetManager _relay;

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
					nee.Peer.Send(Serializer.Save(new DataPacked(item, userConnection.index)), DeliveryMethod.ReliableOrdered);
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

			clientListener.PeerConnectedEvent += peer => {
				Console.WriteLine("PeerConnected to Relay server: " + peer.EndPoint + " Tag " + peer.Tag);
				try {
					ProcessConnection(peer, (string)peer.Tag);
				}
				catch (Exception e) {
					Console.WriteLine($"Error with relay connection {e}");
				}
			};

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
					var peer = request.Accept();
					peer.Tag = data;
				}
			};

			clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
				Console.WriteLine("PeerDisconnected from RelayServer: " + disconnectInfo.Reason);
				if (disconnectInfo.AdditionalData.AvailableBytes > 0) {
					Console.WriteLine("Disconnect data RelayServer: " + disconnectInfo.AdditionalData.GetInt());
				}
			};

			_relay = new NetManager(clientListener) {
				IPv6Enabled = IPv6Mode.SeparateSocket//Todo change to dule mode
			};
			_relay.Start(port);
			_relay.MaxConnectAttempts = 15;
			_relay.ChannelsCount = 3;
			_relay.DisconnectTimeout = 1000000;
			_relay.ReuseAddress = true;
			_relay.UpdateTime = 120;//Todo change update speed
			Console.WriteLine($"Started Relay Server on port {port}");
		}

		public void Update() {
			_relay.PollEvents();
		}

		public void Kill() {
			_relay.Stop();
		}

		private void ClientListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) {
			try {
				var data = reader.GetRemainingBytes();
				if (peer.Tag is List<UserConnection> userconections) {
					if (Serializer.TryToRead<ConnectToAnotherUser>(data, out var connectToAnotherUser)) {
						ProcessConnection(peer, connectToAnotherUser.Key);
					}
					else if (Serializer.TryToRead<DataPacked>(data, out var packed)) {
						if (userconections.Count > packed.Id - 1) {
							if (userconections[packed.Id - 1].waitingData is not null) {
								if (deliveryMethod == DeliveryMethod.ReliableOrdered) {
									userconections[packed.Id - 1].waitingData.Add(packed.Data);
								}
							}
							else {
								userconections[packed.Id - 1].otherConnection.Peer.Send(Serializer.Save(new DataPacked(packed.Data, userconections[packed.Id - 1].otherConnection.index)), 2, deliveryMethod);
							}
						}
					}
					else if (Serializer.TryToRead<StreamDataPacked>(data, out var streampacked)) {
						foreach (var item in userconections) {
							try {
								item.otherConnection?.Peer.Send(Serializer.Save(new DataPacked(data, item.otherConnection.index)), 1, deliveryMethod);
							}
							catch (Exception e) {
								Console.WriteLine("Failed to send to user" + e.ToString());
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
									item.otherConnection.Peer.Send(Serializer.Save(new DataPacked(data, item.otherConnection.index)), 0, deliveryMethod);
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
			reader.Recycle();
		}
	}
}
