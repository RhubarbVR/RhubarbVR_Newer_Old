using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RhuEngine;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;

namespace RhuEngine.Commads
{
	public class DebugInfo : Command
	{
		public override string HelpMsg => "Game Debug Info";

		public override void RunCommand() {
			if(args.Length == 1) {
				Console.WriteLine("Need arg of EngineStatistics,FocusedWorld,FocusedWorldUsers and FocusedWorldNetStatistics");
				return;
			}
			switch (args[1].ToLower()) {
				case "enginestatistics":
					Console.WriteLine(@$"=====---- EngineStatistics ----=====
Is Online {Manager.Engine.netApiManager.Client.IsOnline}
Server Ping {Manager.Engine.netApiManager.Client.Ping}
Is Login {Manager.Engine.netApiManager.Client.IsLogin}
Username {Manager.Engine.netApiManager.Client.User?.UserName ?? "Null"}
UserID {Manager.Engine.netApiManager.Client.User?.Id ?? new Guid()}

worldManager stepTime {Manager.Engine.worldManager.TotalStepTime * 1000f:f3}ms
FPS {1 / RTime.Elapsedf:f3}
Worlds Open {Manager.Engine.worldManager.worlds.Count}
Main Mic {Manager.Engine.MainMic ?? "System Default"}
LastFocusChange {Manager.Engine.worldManager.FocusedWorld?.LastFocusChange}
=====-----------------------=====");
					break;
				case "focusedworldusers":
					Console.WriteLine(@$"=====    FocusedWorldUsers    =====
{GetUserList()}
=====-----------------------=====");
					break;
				case "focusedworldnetstatistics":
					Console.WriteLine(@$"=====FocusedWorldNetStatistics=====
{((Manager.Engine.worldManager.FocusedWorld?.NetStatistics is not null) ?
$@"BytesReceived {Manager.Engine.worldManager.FocusedWorld?.NetStatistics?.BytesReceived.ToString()}
BytesSent {Manager.Engine.worldManager.FocusedWorld?.NetStatistics?.BytesSent}
PacketLoss {Manager.Engine.worldManager.FocusedWorld?.NetStatistics?.PacketLoss}
PacketLossPercent {Manager.Engine.worldManager.FocusedWorld?.NetStatistics?.PacketLossPercent}
PacketsReceived {Manager.Engine.worldManager.FocusedWorld?.NetStatistics?.PacketsReceived}
PacketsSent {Manager.Engine.worldManager.FocusedWorld?.NetStatistics?.PacketsSent}" : "No NetStatistics for this world")}
=====-----------------------=====");
					break;
				default:
					Console.WriteLine(@$"=====---- FocusedWorld ----=====
UserID {Manager.Engine.worldManager.FocusedWorld.GetLocalUser()?.userID.Value ?? "Null"}
IsLoading { Manager.Engine.worldManager.FocusedWorld?.IsLoading}
IsLoadingNet { Manager.Engine.worldManager.FocusedWorld?.IsLoadingNet}
WaitingForState { Manager.Engine.worldManager.FocusedWorld?.WaitingForWorldStartState}
IsDeserializing { Manager.Engine.worldManager.FocusedWorld?.IsDeserializing}
World Name { Manager.Engine.worldManager.FocusedWorld?.WorldName.Value ?? "Null"}
Session Name { Manager.Engine.worldManager.FocusedWorld?.SessionName.Value ?? "Null"}
MasterUserID { Manager.Engine.worldManager.FocusedWorld?.MasterUser}
UserID { Manager.Engine.worldManager.FocusedWorld?.LocalUserID}
UserCount { Manager.Engine.worldManager.FocusedWorld?.Users.Count}
Updating Entities { Manager.Engine.worldManager.FocusedWorld?.UpdatingEntityCount}
Entities { Manager.Engine.worldManager.FocusedWorld?.EntityCount}
Networkeds { Manager.Engine.worldManager.FocusedWorld?.NetworkedObjectsCount}
WorldObjects { Manager.Engine.worldManager.FocusedWorld?.WorldObjectsCount}
RenderComponents { Manager.Engine.worldManager.FocusedWorld?.RenderingComponentsCount}
GlobalStepables { Manager.Engine.worldManager.FocusedWorld?.GlobalStepableCount}
stepTime { (Manager.Engine.worldManager.FocusedWorld?.stepTime * 1000f).Value:f3}ms
===== -----------------------=====");
					break;
			}
		}
		private string GetUserList() {
			var returnstring = "";
			var currentUserID = 0;
			if (Manager.Engine.worldManager.FocusedWorld != null) {
				//Todo: make forLoop
				foreach (var item in Manager.Engine.worldManager.FocusedWorld.Users.Cast<User>()) {
					returnstring += $"User: {currentUserID + 1} UserRef: {item.Pointer} UserName: {item.UserName} PeerLoaded: {item.CurrentPeer != null} UserID: {item.userID.Value} IsLocal: {Manager.Engine.worldManager.FocusedWorld?.GetLocalUser() == item} SyncStreamsCount: {item.syncStreams.Count} isPresent: {item.isPresent.Value} isConnected: {item.IsConnected} peerID: {item.CurrentPeer?.ID.ToString() ?? "null"}  latency{item.CurrentPeer?.latency ?? -1}\n";
					currentUserID++;
				}
			}
			else {
				returnstring = "Not in session\n";
			}
			return returnstring;
		}
	}
}
