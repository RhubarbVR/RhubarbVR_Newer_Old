using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LiteNetLib;

using Newtonsoft.Json;

using RhuEngine.AssetSystem.RequestStructs;
using RhuEngine.DataStructure;
using RhuEngine.Datatypes;
using RhuEngine.Managers;

using SharedModels;

using StereoKit;

namespace RhuEngine.WorldObjects
{
	public partial class World : IWorldObject
	{

		private ClientWebSocket _client;
		public bool IsNetworked { get; private set; } = false;
		public bool IsLoadingNet { get; private set; } = true;
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
				StartWebSocketClient(false);
				IsLoadingNet = false;
				WaitingForWorldStartState = false;
			}
			else {
				LocalUserID = 0;
				IsLoadingNet = true;
				IsDeserializing = true;
				StartWebSocketClient(true);
			}
		}

		private void SessionNameChanged() {
			if (LocalUserID != 1) {
				return;
			}
			try {
				_client?.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("SetSessionName^&^" + SessionName.Value)), WebSocketMessageType.Text, true, CancellationToken.None);
			}
			catch { }
		}

		private void StartWebSocketClient(bool joiningSession) {
			try {
				Log.Info("Starting WebSocket Client");
				_client = new ClientWebSocket();
				var cernts = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();
				foreach (var item in NetApiManager.LetsEncrypt) {
					cernts.Add(new System.Security.Cryptography.X509Certificates.X509Certificate(item));
				}
				_client.Options.ClientCertificates = cernts;
				var dist = new Uri(worldManager.Engine.netApiManager.BaseAddress, "/ws/session");
				var uri = new UriBuilder(dist) {
					//Check if Android so it can be insecure 
					Scheme = ((dist.Scheme == Uri.UriSchemeHttps) && !RuntimeInformation.FrameworkDescription.StartsWith("Mono ")) ? "wss" : "ws",
					Port = RuntimeInformation.FrameworkDescription.StartsWith("Mono ") ? 80 : dist.Port
				};
				//this disgusts me i needed the auth cookie if android
				if (RuntimeInformation.FrameworkDescription.StartsWith("Mono ")) {
					worldManager.Engine.netApiManager.Cookies.SetCookies(uri.Uri, worldManager.Engine.netApiManager.Cookies.GetCookieHeader(worldManager.Engine.netApiManager.BaseAddress));
				}
				_client.Options.Cookies = worldManager.Engine.netApiManager.Cookies;
				//Its for the quest users aaaaaaa
				Log.Info($"Using {uri.Uri}");
				var connection = _client.ConnectAsync(uri.Uri, CancellationToken.None);
				connection.Wait();
				Log.Info($"WebSocket Client {connection.Status}");
				Task.Run(() => {
					if (joiningSession) {
						WaitingForWorldStartState = true;
						_client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("JoinSession^&^" + JsonConvert.SerializeObject(new UserSessionInfo { ConnectionType = ConnectionType.HolePunch }) + "^&^" + SessionID.Value)), WebSocketMessageType.Text, true, CancellationToken.None);
					}
					else {
						_client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("CreateSession^&^" + JsonConvert.SerializeObject(new UserSessionInfo { ConnectionType = ConnectionType.HolePunch }))), WebSocketMessageType.Text, true, CancellationToken.None);
					}
					var WaitingForSessionID = true;
					while (_client.State == WebSocketState.Open) {
						var buffer = new ArraySegment<byte>(new byte[1024]);
						var task = _client.ReceiveAsync(buffer, CancellationToken.None);
						task.Wait();
						if (WaitingForSessionID) {
							if (joiningSession) {
								var data = Encoding.UTF8.GetString(buffer.Array);
								if (data.Remove(5) == "Added") {
									IsDeserializing = true;
									IsLoadingNet = false;
									WaitingForSessionID = false;
								}
								else {
									Log.Err(data);
									Dispose();
								}
							}
							else {
								SessionID.Value = Encoding.UTF8.GetString(buffer.Array);
								Log.Info(SessionID.Value);
								WaitingForSessionID = false;
							}
						}
						else {
							ConnectToUser(JsonConvert.DeserializeObject<ConnectToUser>(Encoding.UTF8.GetString(buffer.Array)));
						}
					}
				});
				try {
					LoadNatManager();
				}
				catch (Exception ex) {
					Log.Err($"Faild to start NatManager {ex}");
					Dispose();
				}
			}
			catch (Exception e) {
				Log.Err($"Faild to start WebSocket Client {e}");
				Dispose();
			}
		}

#if DEBUG
		private string KEY => $"MyNameISJeffryFromMilksnake{worldManager.Engine.version.Major}{worldManager.Engine.version.Minor}";
#else
		private string KEY => $"MyNameISJeffryFromRhubarbVR{worldManager.Engine.version.Major}{worldManager.Engine.version.Minor}";
#endif

		public ConcurrentDictionary<string, bool> NatIntroductionSuccessIsGood = new();
		public ConcurrentDictionary<string, NetPeer> NatConnection = new();
		public ConcurrentDictionary<string, string> NatUserIDS = new();

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
				Log.Info($"NatIntroductionSuccess {point}  {addrType}  {token}");
				NatIntroductionSuccessIsGood[token] = true;
				var peer = _netManager.Connect(point, token + '~' + KEY);
				peer.Tag = NatUserIDS[token];
				NatConnection.TryAdd(token, peer);
			};

			_natPunchListener.NatIntroductionRequest += (point, addrType, token) => {
				Log.Info($"NatIntroductionRequest {point}  {addrType}  {token}");
				NatIntroductionSuccessIsGood.TryAdd(token, false);
			};

			_clientListener.PeerConnectedEvent += PeerConnected;

			_clientListener.ConnectionRequestEvent += request => {
				try {
					var key = request.Data.GetString();
					Log.Info($"ConnectionRequestEvent Key: {key}");
					if (key.Contains('~')) {
						var e = key.Split('~');
						if (e[1] == KEY) {
							Log.Info($"ConnectionRequestEvent Accepted");
							var peer = request.Accept();
							peer.Tag = NatUserIDS[e[0]];
							NatIntroductionSuccessIsGood.TryAdd(e[0], true);
						}
						else {
							Log.Info($"ConnectionRequestEvent Reject Key invalied");
							request.Reject();
						}
					}
					else {
						request.AcceptIfKey(KEY);
					}
				} catch{
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
					try {
						WaitingForWorldStartState = false;
						_worldObjects.Clear();
						var deserializer = new SyncObjectDeserializerObject(false);
						Deserialize((DataNodeGroup)worldData, deserializer);
						LocalUserID = (ushort)(Users.Count + 1);
						foreach (var item in deserializer.onLoaded) {
							item?.Invoke();
						}
						Log.Info("World state loaded");
						foreach (var peer1 in _netManager.ConnectedPeerList) {
							if (peer1.Tag is Peer contpeer) {
								LoadUserIn(contpeer);
							}
						}
						IsDeserializing = false;
						AddLocalUser();
						FindNewMaster();
					}
					catch (Exception ex) {
						Log.Err("Failed to load world state" + ex);
					}
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
							Log.Err($"Failed to Process NetData target:{target.Value.HexString()} Error:{ex}");
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
					if (Serializer.TryToRead<Dictionary<string, IDataNode>>(data, out var keyValuePairs)) {
						ProcessPackedData(new DataNodeGroup(keyValuePairs), deliveryMethod, tag);
					}
					else if (Serializer.TryToRead<IAssetRequest>(data, out var assetRequest)) {
						AssetResponses(assetRequest, tag, deliveryMethod);
					}
					else if (Serializer.TryToRead<StreamDataPacked>(data, out var streamDataPacked)) {
						ProcessPackedData(new DataNodeGroup(streamDataPacked.Data), deliveryMethod, tag);
					}
					else {
						throw new Exception("Uknown Data from User");
					}
				}
				else if (peer.Tag is RelayPeer) {
					var tag = peer.Tag as RelayPeer;
					if (Serializer.TryToRead<DataPacked>(data, out var packed)) {
						if (Serializer.TryToRead<Dictionary<string, IDataNode>>(packed.Data, out var keyValuePairs)) {
							ProcessPackedData(new DataNodeGroup(keyValuePairs), deliveryMethod, tag[packed.Id]);
						}
						else if (Serializer.TryToRead<IAssetRequest>(packed.Data, out var assetRequest)) {
							AssetResponses(assetRequest, tag[packed.Id], deliveryMethod);
						}
						else if (Serializer.TryToRead<StreamDataPacked>(packed.Data, out var streamDataPacked)) {
							ProcessPackedData(new DataNodeGroup(streamDataPacked.Data), deliveryMethod, tag[packed.Id]);
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
					Log.Err("Peer is not known");
				}
			}
			catch (Exception ex) {
				Log.Err($"Failed to proccess packed Error {ex}");
			}
			reader.Recycle();
		}

		public void ProcessUserConnection(Peer peer) {
			Log.Info("User connected");
			peers.Add(peer);
			if (IsLoading) {
				return;
			}
			if (LocalUserID == MasterUser) {
				Log.Info("Sending initial world state to a new user");
				var dataGroup = new DataNodeGroup();
				dataGroup.SetValue("WorldData", Serialize(new SyncObjectSerializerObject(true)));
				peer.Send(dataGroup.GetByteArray(), DeliveryMethod.ReliableOrdered);
			}
			LoadUserIn(peer);
			FindNewMaster();
		}

		private void PeerConnected(NetPeer peer) {
			Log.Info("Peer connected");
			if (peer.EndPoint.Address.ToString().Contains("127.0.0.1") && peer.Tag is not RelayPeer) {  // this is to make debuging essayer
				return;
			}
			if (peer.Tag is RelayPeer relayPeer) {
				Log.Info("Peer was relay server");
				relayServers.Add(relayPeer);
				relayPeer.OnConnect();
			}
			else if (peer.Tag is string @string) {
				Log.Info("Normal Peer Loaded");
				var newpeer = new Peer(peer, @string);
				peer.Tag = newpeer;
				ProcessUserConnection(newpeer);
			}
			else {
				Log.Err("Peer had no tag noidea what to do");
			}
		}

		private void ConnectToUser(ConnectToUser user) {
			if (user is null) {
				return;
			}
			Log.Info("Connecting to user " + user.ConnectionType.ToString() + " Token:" + user.Data + " UserID:" + user.UserID);
			switch (user.ConnectionType) {
				case ConnectionType.Direct:
					var idUri = new Uri(user.Data);
					var dpeer = _netManager.Connect(idUri.Host, idUri.Port, KEY);
					dpeer.Tag = user.UserID;
					break;
				case ConnectionType.HolePunch:
					Task.Run(() => {
						try {
							var peerCount = _netManager.ConnectedPeersCount;
							Log.Info("Server: " + worldManager.Engine.netApiManager.BaseAddress.Host);
							NatUserIDS.TryAdd(user.Data, user.UserID);
							_netManager.NatPunchModule.SendNatIntroduceRequest(worldManager.Engine.netApiManager.BaseAddress.Host, 7856, user.Data);
							for (var i = 0; i < 6; i++) {
								//Like this so i can add update Msgs
								Thread.Sleep(1000);
							}
							if (NatIntroductionSuccessIsGood.TryGetValue(user.Data, out var value) && value) {
								if (NatConnection.TryGetValue(user.Data, out var peer)) {
									if ((peer?.ConnectionState ?? ConnectionState.Disconnected) != ConnectionState.Connected) {
										try {
											if (peer is not null) {
												peer.Disconnect();
											}
										}
										catch { }
										Log.Info("Failed To Hole Punch now using relay");
										RelayConnect(user);
									}
									else {
										Log.Info("HolePunch succeeded");
									}
								}
								else {
									Log.Info("Failed To Hole Punch now using relay");
									RelayConnect(user);
								}
							}
							else {
								Log.Info("Failed To Hole Punch now using relay");
								RelayConnect(user);
							}
							NatIntroductionSuccessIsGood.TryRemove(user.Data, out _);
							NatConnection.TryRemove(user.Data, out _);
							NatUserIDS.TryRemove(user.Data, out _);
						}
						catch (Exception e) {
							Log.Err($"Excerption when trying to hole punch {e}");
						}
					});
					break;
				case ConnectionType.Relay:
					NatUserIDS.TryAdd(user.Data, user.UserID);
					RelayConnect(user);
					break;
				default:
					break;
			}
		}

		private void RelayConnect(ConnectToUser user) {
			Log.Info("Relay Connect Client");
			try {
				var peer = _netManager.Connect(worldManager.Engine.netApiManager.BaseAddress.Host, 7857, user.Data);
				if (peer.Tag is not null) {
					Log.Info("Adding another Relay Client");
					var relay = peer.Tag as RelayPeer;
					relay.LoadNewPeer(user);
				}
				else {
					Log.Info("Start New Relay Server");
					var relay = new RelayPeer(peer, this, user.UserID);
					peer.Tag = relay;
				}
			}
			catch (Exception e) {
				Log.Err("Relay Connect Error:" + e.ToString());
			}
		}

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
			_netManager.SendToAll(netData.GetByteArray(), 0, deliveryMethod);
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
			_netManager.SendToAll(Serializer.Save(new StreamDataPacked(netData.GetByteArray())), 1, deliveryMethod);
		}


		public void AddLocalUser() {
			var Olduser = GetUserFromID(Engine.netApiManager.User?.Id ?? "Null");
			if (Olduser is null) {
				ItemIndex = 176;
				LocalUserID = (ushort)(Users.Count + 1);
				Log.Info($"Loaded local User with id{LocalUserID}");
				var user = Users.Add();
				user.userID.Value = worldManager.Engine.netApiManager.User?.Id ?? "null";
			}
			else {
				LocalUserID = (ushort)(Users.IndexOf(Olduser) + 1);
				var e = from objec in _worldObjects
						where objec.Key.GetOwnerID() == LocalUserID
						orderby objec.Key.id descending
						select objec.Key;
				ItemIndex = e.First().ItemIndex() + 10;
			}
		}
	}
}
