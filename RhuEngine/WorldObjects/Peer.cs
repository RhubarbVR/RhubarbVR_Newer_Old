
using System;
using System.Collections.Generic;
using LiteNetLib;

using RhuEngine.Linker;

using SharedModels;
using SharedModels.GameSpecific;
using SharedModels.Session;


namespace RhuEngine.WorldObjects
{
	public class RelayPeer
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
			var newpeer = new Peer(NetPeer,user.UserID, (ushort)(peers.Count + 1));
			peers.Add(newpeer);
			NetPeer.Send(Serializer.Save(new ConnectToAnotherUser(user.UserID.ToString())), 2, DeliveryMethod.ReliableSequenced);
			World.ProcessUserConnection(newpeer);
			return newpeer;
		}
		public void OnConnect() {
			RLog.Info("PeerServerConnected");
			peers.Clear();
			//first peer is loading in key
			RLog.Info("Loading First Relay Peer");
			var firstpeer = new Peer(NetPeer,StartingPeerID, 1);
			peers.Add(firstpeer);
			World.ProcessUserConnection(firstpeer);
			
		}
	}

	public class Peer
	{
		public User User { get; set; }
		public Guid UserID { get;private set; }
		public ushort ID { get;private set; }
		public NetPeer NetPeer { get; private set; }

		public int latency = 0;

		public Peer(NetPeer netPeer, Guid userID, ushort id = 0) {
			NetPeer = netPeer;
			ID = id;
			UserID = userID;
		}

		public void Send(byte[] data, DeliveryMethod reliableOrdered) {
			if( ID == 0) {
				NetPeer.Send(data, 0,reliableOrdered);
			}
			else {
				NetPeer.Send(Serializer.Save(new DataPacked(data,ID)),0, reliableOrdered);
			}
		}

		internal void KillRelayConnection() {
			NetPeer = null;
		}

		public void SendAsset(byte[] data, DeliveryMethod reliableOrdered) {
			if (ID == 0) {
				NetPeer.Send(data,3, reliableOrdered);
			}
			else {
				NetPeer.Send(Serializer.Save(new DataPacked(data, ID)),3, reliableOrdered);
			}
		}
	}
}
