
using System;
using System.Collections.Generic;
using LiteNetLib;
using SharedModels;
using SharedModels.GameSpecific;
using SharedModels.Session;

using StereoKit;

namespace RhuEngine.WorldObjects
{
	public class RelayPeer
	{
		public int latency = 0;
		public NetPeer NetPeer { get; private set; }
		public World World { get; }

		private string StartingPeerID { get; }

		public RelayPeer(NetPeer netPeer, World world, string peerOneID) {
			NetPeer = netPeer;
			World = world;
			StartingPeerID = peerOneID;
		}

		public List<Peer> peers = new();
		public Peer this[ushort id] => peers[id];
		
		public Peer LoadNewPeer(ConnectToUser user) {
			var newpeer = new Peer(NetPeer,user.UserID, (ushort)(peers.Count + 1));
			peers.Add(newpeer);
			NetPeer.Send(Serializer.Save(new ConnectToAnotherUser(user.UserID)), 2, DeliveryMethod.ReliableSequenced);
			World.ProcessUserConnection(newpeer);
			return newpeer;
		}
		public void OnConnect() {
			Log.Info("PeerServerConnected");
			peers.Clear();
			//first peer is loading in key
			Log.Info("Loading First Relay Peer");
			var firstpeer = new Peer(NetPeer,StartingPeerID, 1);
			peers.Add(firstpeer);
			World.ProcessUserConnection(firstpeer);
			
		}
	}

	public class Peer
	{
		public User User { get; set; }
		public string UserID { get;private set; }
		public ushort ID { get;private set; }
		public NetPeer NetPeer { get; private set; }

		public int latency = 0;

		public Peer(NetPeer netPeer,string userID, ushort id = 0) {
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
