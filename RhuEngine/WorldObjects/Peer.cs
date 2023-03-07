
using System;
using System.Collections.Generic;
using System.IO;

using DiscordRPC;

using LiteNetLib;

using RhubarbCloudClient.Model;

using RhuEngine.DataStructure;
using RhuEngine.Linker;

using SharedModels;
using SharedModels.GameSpecific;

using static RhuEngine.WorldObjects.World;

namespace RhuEngine.WorldObjects
{
	public sealed class RelayPeer
	{
		public int latency = 0;
		public NetPeer NetPeer { get; private set; }
		public World World { get; }

		private Guid StartingPeerID { get; }

		public RelayPeer(NetPeer netPeer, World world, Guid peerOneID) {
			NetPeer = netPeer;
			World = world;
			StartingPeerID = peerOneID;
		}

		public List<Peer> peers = new();
		public Peer this[ushort id] => peers[id];
		public Peer LoadNewPeer(ConnectToUser user) {
			var newpeer = new Peer(NetPeer, user.UserID, (ushort)(peers.Count + 1));
			peers.Add(newpeer);
			using var memstream = new MemoryStream();
			using var reader = new BinaryWriter(memstream);
			RelayNetPacked.Serlize(reader, new ConnectToAnotherUser(user.UserID.ToString()));
			NetPeer.Send(memstream.ToArray(), 2, DeliveryMethod.ReliableSequenced);
			World.ProcessUserConnection(newpeer);
			return newpeer;
		}
		public void OnConnect() {
			RLog.Info("PeerServerConnected");
			peers.Clear();
			//first peer is loading in key
			RLog.Info("Loading First Relay Peer");
			var firstpeer = new Peer(NetPeer, StartingPeerID, 1);
			peers.Add(firstpeer);
			World.ProcessUserConnection(firstpeer);

		}
	}

	public sealed class Peer
	{
		private User _user;

		public User User
		{
			get {
				if (_user is null) {
					RLog.Err("User Not loaded");
				}
				return _user;
			}
			set => _user = value;
		}

		public Guid UserID { get; private set; }
		public ushort ID { get; private set; }
		public NetPeer NetPeer { get; private set; }

		public bool IsRelay => ID != 0;

		public int latency = 0;

		public Peer(NetPeer netPeer, Guid userID, ushort id = 0) {
			NetPeer = netPeer;
			ID = id;
			UserID = userID;
		}

		public void Send(byte[] data, DeliveryMethod reliableOrdered) {
			if (ID == 0) {
				NetPeer.Send(data, 0, reliableOrdered);
			}
			else {
				using var memstream = new MemoryStream();
				using var reader = new BinaryWriter(memstream);
				RelayNetPacked.Serlize(reader, new DataPacked(data, ID));
				NetPeer.Send(memstream.ToArray(), 0, reliableOrdered);
			}
		}

		internal void KillRelayConnection() {
			NetPeer = null;
		}

		public void SendAsset(byte[] data, DeliveryMethod reliableOrdered) {
			if (ID == 0) {
				NetPeer.Send(data, 2, reliableOrdered);
			}
			else {
				using var memstream = new MemoryStream();
				using var reader = new BinaryWriter(memstream);
				RelayNetPacked.Serlize(reader, new DataPacked(data, ID));
				NetPeer.Send(memstream.ToArray(), 2, reliableOrdered);
			}
		}
	}
}
