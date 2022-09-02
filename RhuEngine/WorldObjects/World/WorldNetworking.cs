using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LiteNetLib;

using Newtonsoft.Json;

using RhuEngine.AssetSystem.RequestStructs;
using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Linker;
using RhuEngine.Managers;

using SharedModels;
using SharedModels.GameSpecific;


namespace RhuEngine.WorldObjects
{
	public partial class World : IWorldObject
	{

		public bool IsNetworked { get; internal set; } = false;
		public bool IsLoadingNet { get; internal set; } = true;
		public bool WaitingForWorldStartState { get; internal set; } = true;

		public ushort MasterUser { get; set; } = 1;

		public List<RelayPeer> relayServers = new();
		public List<Peer> peers = new();

		private NetManager _netManager;
		private readonly EventBasedNatPunchListener _natPunchListener = new();
		private readonly EventBasedNetListener _clientListener = new();
		public void StartNetworking(bool newWorld) {
			IsNetworked = true;
			if (newWorld) {
				IsDeserializing = false;
				AddLocalUser();
				ConnectedToSession(false);
				WaitingForWorldStartState = false;
			}
			else {
				LocalUserID = 0;
				IsLoadingNet = true;
				IsDeserializing = true;
				ConnectedToSession(true);
			}
		}

		internal void SessionInfoChanged() {
			worldManager.OnWorldUpdateTaskBar?.Invoke();
			if (MasterUser != LocalUserID) {
				return;
			}
			if (!IsNetworked) {
				return;
			}
			//ToDO: SessionRequest
			//var sessionInfo = new SessionInfo {
			//	ActiveUsers = Users.Select(user => (((User)user)?.isPresent?.Value??false) && (((User)user).IsConnected || ((User)user).IsLocalUser)).Where((val) => val).Count(),
			//	Admins = Admins.Select((x) => Guid.Parse(((SyncRef<User>)x).Target?.userID.Value)).ToArray(),
			//	AssociatedGroup = Guid.Parse(AssociatedGroup.Value),
			//	IsHidden = IsHidden.Value,
			//	MaxUsers = MaxUserCount.Value,
			//	ThumNail = ThumNail.Value,
			//	SessionTags = SessionTags,
			//	SessionAccessLevel = AccessLevel.Value,
			//	SessionName = SessionName.Value,
			//	NormalizedSessionName = SessionName.Value.Normalize(),
			//	SessionId = Guid.Parse(SessionID.Value)
			//};
			//var sessionConnection = new SessionCreation {
			//	SessionInfo = sessionInfo,
			//	UserConnectionInfo = null,
			//	ForceJoin = Array.Empty<Guid>()
			//};
			//Engine.netApiManager.SendDataToSocked(new SessionRequest { RequestType = RequestType.UpdateSession, RequestData = JsonConvert.SerializeObject(sessionConnection), ID = sessionInfo.SessionId });
		}

		private void ConnectedToSession(bool joiningSession) {
			Task.Run(async () => {
				IsLoadingNet= true;
				LoadNatManager();
				try {
					var Pings = new Dictionary<string, int>();
					//ToDO: SessionRequest
					//var servers = await Engine.netApiManager.GetRelayHoleServers();
					//foreach (var item in servers) {
					//	var pingSender = new Ping();
					//	var e = pingSender.Send(new Uri(item.IP).Host, 500);
					//	if ((e?.Status ?? IPStatus.Unknown) == IPStatus.Success) {
					//		Pings.Add(item.IP, (int)e.RoundtripTime);
					//	}
					//}
					////TODO: add support for changeing on connection info 
					//var userConnection = new UserConnectionInfo {
					//	ConnectionType = ConnectionType.HolePunch,
					//	ServerPingLevels = Pings,
					//	Data = null
					//};
					//if (!joiningSession) {
					//	SessionID.Value = Guid.NewGuid().ToString();
					//	var sessionInfo = new SessionInfo {
					//		ActiveUsers = 1,
					//		Admins = Admins.Select((x)=>Guid.Parse(((SyncRef<User>)x).Target?.userID.Value)).ToArray(),
					//		AssociatedGroup = Guid.Parse(AssociatedGroup.Value),
					//		IsHidden = IsHidden.Value,
					//		MaxUsers = MaxUserCount.Value,
					//		ThumNail = ThumNail.Value,
					//		SessionTags = SessionTags,
					//		SessionAccessLevel = AccessLevel.Value,
					//		SessionName = SessionName.Value,
					//		NormalizedSessionName = SessionName.Value.Normalize(),
					//		SessionId = Guid.Parse(SessionID.Value)
					//	};
					//	var sessionConnection = new SessionCreation {
					//		SessionInfo = sessionInfo,
					//		UserConnectionInfo = userConnection,
					//		ForceJoin = Array.Empty<Guid>()
					//	};
					//	Engine.netApiManager.SendDataToSocked(new SessionRequest { ID = Guid.Parse(SessionID.Value), RequestData = JsonConvert.SerializeObject(sessionConnection), RequestType = RequestType.CreateSession });

					//}
					//else {
					//	var sessionConnection = new JoinSession {
					//		SessionID = Guid.Parse(SessionID.Value),
					//		UserConnectionInfo = userConnection,
					//	};
					//	Engine.netApiManager.SendDataToSocked(new SessionRequest { ID = Guid.Parse(SessionID.Value), RequestData = JsonConvert.SerializeObject(sessionConnection), RequestType = RequestType.JoinSession });
					//}
					IsLoadingNet = false;
				}
				catch (Exception ex) {
					LoadMsg = "Failed to Connected To Session" + ex.Message;
				}
			});
		}

#if DEBUG
		private string KEY => $"MyNameISJeffryFromMilksnake{worldManager.Engine.version.Major}{worldManager.Engine.version.Minor}";
#else
		private string KEY => $"MyNameISJeffryFromRhubarbVR{worldManager.Engine.version.Major}{worldManager.Engine.version.Minor}";
#endif

		public ConcurrentDictionary<string, bool> NatIntroductionSuccessIsGood = new();
		public ConcurrentDictionary<string, NetPeer> NatConnection = new();
		public ConcurrentDictionary<string, Guid> NatUserIDS = new();

		private void FindNewMaster() {
			for (var i = 0; i < Users.Count; i++) {
				var user = Users[i];
				if (user.IsConnected || user.IsLocalUser) {
					MasterUser = (ushort)(i + 1);
					break;
				}
			}
		}

		private void PeerDisconect(Peer peer) {
			FindNewMaster();
		}

		private void LoadNatManager() {
			_natPunchListener.NatIntroductionSuccess += (point, addrType, token) => {
				RLog.Info($"NatIntroductionSuccess {point}  {addrType}  {token}");
				NatIntroductionSuccessIsGood[token] = true;
				var peer = _netManager.Connect(point, token + '~' + KEY);
				peer.Tag = NatUserIDS[token];
				NatConnection.TryAdd(token, peer);
			};

			_natPunchListener.NatIntroductionRequest += (point, addrType, token) => {
				RLog.Info($"NatIntroductionRequest {point}  {addrType}  {token}");
				NatIntroductionSuccessIsGood.TryAdd(token, false);
			};

			_clientListener.PeerConnectedEvent += PeerConnected;

			_clientListener.ConnectionRequestEvent += request => {
				try {
					var key = request.Data.GetString();
					RLog.Info($"ConnectionRequestEvent Key: {key}");
					if (key.Contains('~')) {
						var e = key.Split('~');
						if (e[1] == KEY) {
							RLog.Info($"ConnectionRequestEvent Accepted");
							var peer = request.Accept();
							peer.Tag = NatUserIDS[e[0]];
							NatIntroductionSuccessIsGood.TryAdd(e[0], true);
							NatConnection.TryAdd(e[0], peer);
						}
						else {
							RLog.Info($"ConnectionRequestEvent Reject Key invalied");
							request.Reject();
						}
					}
					else {
						request.AcceptIfKey(KEY);
					}
				}
				catch {
					request.Reject();
				}
			};

			_clientListener.NetworkReceiveEvent += ClientListener_NetworkReceiveEvent;
			_clientListener.NetworkLatencyUpdateEvent += ClientListener_NetworkLatencyUpdateEvent;

			_clientListener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
				if (peer.Tag is Peer rpeer) {
					PeerDisconect(rpeer);
				}
				else if (peer.Tag is RelayPeer repeer) {
					relayServers.Remove(repeer);
					foreach (var item in repeer.peers) {
						PeerDisconect(item);
					}
				}
				Console.WriteLine($"PeerDisconnected: " + disconnectInfo.Reason);
				if (disconnectInfo.AdditionalData.AvailableBytes > 0) {
					Console.WriteLine("Disconnect data: " + disconnectInfo.AdditionalData.GetInt());
				}
			};

			_netManager = new NetManager(_clientListener) {
				IPv6Enabled = IPv6Mode.SeparateSocket,
				NatPunchEnabled = true
			};
			_netManager.NatPunchModule.Init(_natPunchListener);
			_netManager.Start();
			_netManager.EnableStatistics = true;
			_netManager.MaxConnectAttempts = 15;
			_netManager.DisconnectTimeout = 10000;
			_netManager.UpdateTime = 10;
			_netManager.ChannelsCount = 3;
			_netManager.UpdateTime = 120;
			//0 is main
			//1 is syncStreams
			//2 is assetPackeds
		}

		private void ClientListener_NetworkLatencyUpdateEvent(NetPeer peer, int latency) {
			if (peer.Tag is Peer rupeer) {
				rupeer.latency = latency;
			}
			else if (peer.Tag is RelayPeer repeer) {
				repeer.latency = latency;
			}
		}

		public NetStatistics NetStatistics => _netManager?.Statistics;

		private void ProcessPackedData(DataNodeGroup dataGroup, DeliveryMethod deliveryMethod, Peer peer) {
			if (WaitingForWorldStartState) {
				try {
					var worldData = dataGroup.GetValue("WorldData");
					if (worldData == null) {
						throw new Exception();
					}
					Task.Run(() => {
						try {
							LoadMsg = "World state Found";
							// Wait for everyone
							//while (ActiveConnections.Count > 0) {
							//	Thread.Sleep(1000);
							//}
							_worldObjects.Clear();
							var deserializer = new SyncObjectDeserializerObject(false);
							Deserialize((DataNodeGroup)worldData, deserializer);
							LocalUserID = (ushort)(Users.Count + 1);
							foreach (var item in deserializer.onLoaded) {
								item?.Invoke();
							}
							RLog.Info(LoadMsg = "World state loaded");
							foreach (var peer1 in _netManager.ConnectedPeerList) {
								if (peer1.Tag is Peer contpeer) {
									LoadUserIn(contpeer);
								}
							}
							IsDeserializing = false;
							WaitingForWorldStartState = false;
							AddLocalUser();
							FindNewMaster();
						}
						catch (Exception ex) {
							RLog.Err("Failed to load world state" + ex);
							LoadMsg = "Failed to load world state" + ex;
						}
					});
				}
				catch { }
			}
			else {
				try {
					var target = (DataNode<NetPointer>)dataGroup.GetValue("Pointer");
					if (target == null) {
						throw new Exception();
					}
					try {
						_networkedObjects[target.Value].Received(peer, dataGroup.GetValue("Data"));
					}
					catch (Exception ex) {
#if DEBUG
						if (deliveryMethod == DeliveryMethod.ReliableOrdered && peer.User is not null) {
							RLog.Err($"Failed to Process NetData target:{target.Value.HexString()} Error:{ex}");
						}
#endif
					}
				}
				catch { }
			}
		}

		private void ClientListener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) {
			if (IsDisposed) {
				return;
			}
			try {
				var data = reader.GetRemainingBytes();
				if (peer.Tag is Peer) {
					var tag = peer.Tag as Peer;
					if (Serializer.TryToRead<BlockStore>(data, out var keyValuePairs)) {
						ProcessPackedData((DataNodeGroup)new DataReader(keyValuePairs).Data, deliveryMethod, tag);
					}
					else if (Serializer.TryToRead<IAssetRequest>(data, out var assetRequest)) {
						AssetResponses(assetRequest, tag, deliveryMethod);
					}
					else if (Serializer.TryToRead<StreamDataPacked>(data, out var streamDataPacked)) {
						ProcessPackedData((DataNodeGroup)new DataReader(streamDataPacked.Data).Data, deliveryMethod, tag);
					}
					else {
						throw new Exception("Uknown Data from User");
					}
				}
				else if (peer.Tag is RelayPeer) {
					var tag = peer.Tag as RelayPeer;
					if (Serializer.TryToRead<DataPacked>(data, out var packed)) {
						if (Serializer.TryToRead<BlockStore>(packed.Data, out var keyValuePairs)) {
							ProcessPackedData((DataNodeGroup)new DataReader(keyValuePairs).Data, deliveryMethod, tag[packed.Id]);
						}
						else if (Serializer.TryToRead<IAssetRequest>(packed.Data, out var assetRequest)) {
							AssetResponses(assetRequest, tag[packed.Id], deliveryMethod);
						}
						else if (Serializer.TryToRead<StreamDataPacked>(packed.Data, out var streamDataPacked)) {
							ProcessPackedData((DataNodeGroup)new DataReader(streamDataPacked.Data).Data, deliveryMethod, tag[packed.Id]);
						}
						else {
							throw new Exception("Uknown Data from relay");
						}
					}
					else if (Serializer.TryToRead<OtherUserLeft>(data, out var otherUserLeft)) {
						var rpeer = tag.peers[otherUserLeft.id];
						tag.peers.Remove(rpeer);
						rpeer.KillRelayConnection();
						PeerDisconect(rpeer);
					}
					else if (Serializer.TryToRead<DataNodeGroup>(data, out _)) {
						throw new Exception("Got a datanode group not a packed");
					}
					else {
						throw new Exception("data packed could not be read");
					}
				}
				else if (peer.Tag is string or null) {
					//Still loading peer
				}
				else {
					RLog.Err("Peer is not known");
				}
			}
			catch (Exception ex) {
				RLog.Err($"Failed to proccess packed Error {ex}");
			}
			reader.Recycle();
		}

		public void ProcessUserConnection(Peer peer) {
			RLog.Info("User connected");
			peers.Add(peer);
			if (IsLoading) {
				return;
			}
			if (LocalUserID == MasterUser) {
				RLog.Info("Sending initial world state to a new user");
				var dataGroup = new DataNodeGroup();
				dataGroup.SetValue("WorldData", Serialize(new SyncObjectSerializerObject(true)));
				peer.Send(new DataSaver(dataGroup).SaveStore(), DeliveryMethod.ReliableOrdered);
			}
			LoadUserIn(peer);
			FindNewMaster();
		}

		private void PeerConnected(NetPeer peer) {
			RLog.Info("Peer connected");
			if (peer.EndPoint.Address.ToString().Contains("127.0.0.1") && peer.Tag is not RelayPeer) {  // this is to make debuging essayer
				return;
			}
			if (peer.Tag is RelayPeer relayPeer) {
				RLog.Info("Peer was relay server");
				relayServers.Add(relayPeer);
				relayPeer.OnConnect();
			}
			else if (peer.Tag is Guid @string) {
				RLog.Info("Normal Peer Loaded");
				var newpeer = new Peer(peer, @string);
				peer.Tag = newpeer;
				ProcessUserConnection(newpeer);
			}
			else {
				RLog.Err("Peer had no tag noidea what to do");
			}
		}
		//ToDO: SessionRequest

		//public List<ConnectToUser> ActiveConnections = new();

		//public void ConnectToUser(ConnectToUser user) {
		//	Task.Run(() => {
		//		lock (ActiveConnections) {
		//			ActiveConnections.Add(user);
		//		}
		//		if (user is null) {
		//			return;
		//		}
		//		RLog.Info("Connecting to user " + user.ConnectionType.ToString() + " Token:" + user.Data + " UserID:" + user.UserID);
		//		LoadMsg = "Connecting to user";
		//		switch (user.ConnectionType) {
		//			case ConnectionType.Direct:
		//				LoadMsg = "Direct Connected to User";
		//				var idUri = new Uri(user.Data);
		//				var dpeer = _netManager.Connect(idUri.Host, idUri.Port, KEY);
		//				dpeer.Tag = user.UserID;
		//				lock (ActiveConnections) {
		//					ActiveConnections.Remove(user);
		//				}
		//				break;
		//			case ConnectionType.HolePunch:
		//				try {
		//					LoadMsg = "Trying to HolePunch to User";
		//					var peerCount = _netManager.ConnectedPeersCount;
		//					RLog.Info("Server: " + worldManager.Engine.netApiManager.BaseAddress.Host);
		//					NatUserIDS.TryAdd(user.Data, user.UserID);
		//					var eidUri = new Uri(user.Server);
		//					_netManager.NatPunchModule.SendNatIntroduceRequest(eidUri.Host,eidUri.Port, user.Data);
		//					for (var i = 0; i < 60; i++) {
		//						if (NatIntroductionSuccessIsGood.TryGetValue(user.Data, out var evalue) && evalue) {
		//							if (NatConnection.TryGetValue(user.Data, out var peer)) {
		//								if ((peer?.ConnectionState ?? ConnectionState.Disconnected) == ConnectionState.Connected) {
		//									break;
		//								}
		//							}
		//						}
		//						LoadMsg = $"HolePuch Try{(uint)(i / 10)}";
		//						//Like this so i can add update Msgs
		//						Thread.Sleep(100);
		//					}
		//					if (NatIntroductionSuccessIsGood.TryGetValue(user.Data, out var value) && value) {
		//						if (NatConnection.TryGetValue(user.Data, out var peer)) {
		//							if ((peer?.ConnectionState ?? ConnectionState.Disconnected) != ConnectionState.Connected) {
		//								try {
		//									if (peer is not null) {
		//										peer.Disconnect();
		//									}
		//								}
		//								catch {
		//									return;
		//								}
		//								RLog.Info(LoadMsg = "Failed To Hole Punch now using relay");
		//								RelayConnect(user);
		//								lock (ActiveConnections) {
		//									ActiveConnections.Remove(user);
		//								}
		//							}
		//							else {
		//								RLog.Info(LoadMsg = "HolePunch succeeded");
		//								lock (ActiveConnections) {
		//									ActiveConnections.Remove(user);
		//								}
		//							}
		//						}
		//						else {
		//							RLog.Info(LoadMsg = "Failed To Hole Punch now using relay");
		//							RelayConnect(user);
		//							lock (ActiveConnections) {
		//								ActiveConnections.Remove(user);
		//							}
		//						}
		//					}
		//					else {
		//						RLog.Info(LoadMsg = "Failed To Hole Punch now using relay");
		//						RelayConnect(user);
		//						lock (ActiveConnections) {
		//							ActiveConnections.Remove(user);
		//						}
		//					}
		//					NatIntroductionSuccessIsGood.TryRemove(user.Data, out _);
		//					NatConnection.TryRemove(user.Data, out _);
		//					NatUserIDS.TryRemove(user.Data, out _);
		//				}
		//				catch (Exception e) {
		//					RLog.Err($"Excerption when trying to hole punch {e}");
		//					return;
		//				}
		//				break;
		//			case ConnectionType.Relay:
		//				LoadMsg = "Relay connecting to user";
		//				NatUserIDS.TryAdd(user.Data, user.UserID);
		//				RelayConnect(user);
		//				lock (ActiveConnections) {
		//					ActiveConnections.Remove(user);
		//				}
		//				break;
		//			default:
		//				break;
		//		}
		//		LoadMsg = "Waiting for world state";
		//	});
		//}

		//private void RelayConnect(ConnectToUser user) {
		//	RLog.Info("Relay Connect Client");
		//	try {
		//		var eidUri = new Uri(user.Server);
		//		var peer = _netManager.Connect(eidUri.Host, eidUri.Port + 1, user.Data);
		//		if (peer.Tag is not null) {
		//			RLog.Info(LoadMsg = "Adding another Relay Client");
		//			var relay = peer.Tag as RelayPeer;
		//			relay.LoadNewPeer(user);
		//		}
		//		else {
		//			RLog.Info(LoadMsg = "Start New Relay Server");
		//			var relay = new RelayPeer(peer, this, user.UserID);
		//			peer.Tag = relay;
		//		}
		//	}
		//	catch (Exception e) {
		//		RLog.Err(LoadMsg = "Relay Connect Error:" + e.ToString());
		//		throw new Exception("Failed to use relay");
		//	}
		//}

		public void BroadcastDataToAll(IWorldObject target, IDataNode data, DeliveryMethod deliveryMethod) {
			if (target.IsRemoved) {
				return;
			}
			if (_netManager is null) {
				return;
			}
			if (IsLoading) {
				return;
			}
			if (target.Pointer.GetOwnerID() == 0) {
				//LocalValue
				return;
			}
			var netData = new DataNodeGroup();
			netData.SetValue("Data", data);
			netData.SetValue("Pointer", new DataNode<NetPointer>(target.Pointer));
			_netManager.SendToAll(new DataSaver(netData).SaveStore(), 0, deliveryMethod);
		}

		public void BroadcastDataToAllStream(IWorldObject target, IDataNode data, DeliveryMethod deliveryMethod) {
			if (target.IsRemoved) {
				return;
			}
			if (_netManager is null) {
				return;
			}
			if (IsLoading) {
				return;
			}
			if (target.Pointer.GetOwnerID() == 0) {
				//LocalValue
				return;
			}
			var netData = new DataNodeGroup();
			netData.SetValue("Data", data);
			netData.SetValue("Pointer", new DataNode<NetPointer>(target.Pointer));
			_netManager.SendToAll(Serializer.Save(new StreamDataPacked(new DataSaver(netData).SaveStore())), 1, deliveryMethod);
		}


		public void AddLocalUser() {
			LoadMsg = "Adding LocalUser";
			var Olduser = GetUserFromID(Engine.netApiManager.Client.User?.Id??new Guid());
			if (Olduser is null) {
				LoadMsg = "Building new LocalUser";
				ItemIndex = 176;
				LocalUserID = (ushort)(Users.Count + 1);
				RLog.Info($"Built local User with id{LocalUserID}");
				var user = Users.Add();
				user.userID.Value = (worldManager.Engine.netApiManager.Client.User?.Id ?? new Guid()).ToString();
				user.Platform.Value = Environment.OSVersion.Platform;
				user.PlatformVersion.Value = Environment.OSVersion.Version.ToString();
				user.BackendID.Value = Engine.EngineLink.BackendID;
			}
			else {
				LoadMsg = "using old LocalUser";
				LocalUserID = (ushort)(Users.IndexOf(Olduser) + 1);
				RLog.Info($"using old LocalUser with id{LocalUserID}");
				var e = from objec in _worldObjects
						where objec.Key.GetOwnerID() == LocalUserID
						orderby objec.Key.id descending
						select objec.Key;
				ItemIndex = e.First().ItemIndex() + 10;
				RLog.Info($"Last index is {ItemIndex}");

			}
		}
	}
}
