using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Assimp;

using DataModel.Enums;

using NYoutubeDL.Options;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Components.UI;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed partial class DebugerProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Warning;

		public override string ProgramNameLocName => "Programs.Debuger.Name";
		private RichTextLabel _richText;
		private ScrollContainer _scroll;
		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var Window = AddWindow();
			var box = Window.Entity.AddChild("Box").AttachComponent<BoxContainer>();
			box.Vertical.Value = true;
			_scroll = box.Entity.AddChild("Top").AttachComponent<ScrollContainer>();
			_scroll.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			_scroll.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_richText = _scroll.Entity.AddChild("Top").AttachComponent<RichTextLabel>();
			_richText.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			_richText.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_richText.TextSelectionEnabled.Value = true;
			_richText.ContextMenuEnabled.Value = true;
			_richText.Threading.Value = false;
			_richText.FitContent.Value = true;
			_richText.ScrollActive.Value = false;
			_richText.AutoWrapMode.Value = RAutowrapMode.WordSmart;
		}

		private string GetUserList() {
			var returnstring = "";
			if (Engine.worldManager.FocusedWorld != null) {
				for (var i = 0; i < Engine.worldManager.FocusedWorld.Users.Count; i++) {
					var item = Engine.worldManager.FocusedWorld.Users[i];
					returnstring += $"User: {i + 1} UserRef: {item.Pointer} UserName: {item.UserName} PeerLoaded: {item.CurrentPeer != null} UserID: {item.userID.Value} IsLocal: {Engine.worldManager.FocusedWorld?.GetLocalUser() == item} SyncStreamsCount: {item.syncStreams.Count} isPresent: {item.isPresent.Value} isConnected: {item.IsConnected} peerID: {item.CurrentPeer?.ID.ToString() ?? "null"}  latency: {item.CurrentPeer?.latency ?? -1} platform: {item.Platform?.Value.ToString() ?? "null"} \n";
				}
			}
			else {
				returnstring = "Not in session\n";
			}
			return returnstring;
		}

		protected override void Step() {
			if (_richText is null) {
				return;
			}
			var FocusedWorldNoneSync = Engine.worldManager.FocusedWorld?.WorldObjects.Where(x => x.Value is ISyncObject syncObject && !syncObject.HasBeenNetSynced);
			_richText.Text.Value = @$"=====---- EngineStatistics ----=====
Is Online {Engine.netApiManager.Client.IsOnline}
Server Ping {Engine.netApiManager.Client.Ping}
Is Login {Engine.netApiManager.Client.IsLogin}
Username {Engine.netApiManager.Client.User?.UserName ?? "Null"}
UserID {Engine.netApiManager.Client.User?.Id ?? new Guid()}

worldManager stepTime {Engine.worldManager.TotalStepTime * 1000f:f3}ms
FPS {1 / RTime.Elapsed:f3}
Worlds Open {Engine.worldManager.worlds.Count}
LastFocusChange {Engine.worldManager.FocusedWorld?.LastFocusChange}
=====    FocusedWorldUsers    =====
{GetUserList()}
=====FocusedWorldNetStatistics=====
{(Engine.worldManager.FocusedWorld?.NetStatistics is not null ?
$@"BytesReceived {Engine.worldManager.FocusedWorld?.NetStatistics?.BytesReceived.ToString()}
BytesSent {Engine.worldManager.FocusedWorld?.NetStatistics?.BytesSent}
PacketLoss {Engine.worldManager.FocusedWorld?.NetStatistics?.PacketLoss}
PacketLossPercent {Engine.worldManager.FocusedWorld?.NetStatistics?.PacketLossPercent}
PacketsReceived {Engine.worldManager.FocusedWorld?.NetStatistics?.PacketsReceived}
PacketsSent {Engine.worldManager.FocusedWorld?.NetStatistics?.PacketsSent}" : "No NetStatistics for this world")}
=====---- FocusedWorld ----=====
UserID {Engine.worldManager.FocusedWorld.GetLocalUser()?.userID.Value ?? "Null"}
IsLoading {Engine.worldManager.FocusedWorld?.IsLoading}
IsLoadingNet {Engine.worldManager.FocusedWorld?.IsLoadingNet}
WaitingForState {Engine.worldManager.FocusedWorld?.WaitingForWorldStartState}
IsDeserializing {Engine.worldManager.FocusedWorld?.IsDeserializing}
World Name {Engine.worldManager.FocusedWorld?.WorldName.Value ?? "Null"}
Session Name {Engine.worldManager.FocusedWorld?.SessionName.Value ?? "Null"}
MasterUserID {Engine.worldManager.FocusedWorld?.MasterUser}
UserID {Engine.worldManager.FocusedWorld?.LocalUserID}
UserCount {Engine.worldManager.FocusedWorld?.Users.Count}
Updating Entities {Engine.worldManager.FocusedWorld?.UpdatingEntityCount}
Entities {Engine.worldManager.FocusedWorld?.EntityCount}
Networkeds {Engine.worldManager.FocusedWorld?.NetworkedObjectsCount}
WorldObjects {Engine.worldManager.FocusedWorld?.WorldObjectsCount}
RenderComponents {Engine.worldManager.FocusedWorld?.RenderingComponentsCount}
GlobalStepables {Engine.worldManager.FocusedWorld?.GlobalStepableCount}
stepTime {(Engine.worldManager.FocusedWorld?.stepTime * 1000f).Value:f3}ms
ObjectCreationAndDeleteUpdatesCount {Engine.worldManager.FocusedWorld?.ObjectCreationAndDeleteUpdatesCount.ToString() ?? "Null"}
UpdatedNetValuesCount {Engine.worldManager.FocusedWorld?.UpdatedNetValuesCount.ToString() ?? "Null"}
ReliableNetPackedAmount {Engine.worldManager.FocusedWorld?.ReliableNetPackedAmount.ToString() ?? "Null"}
NoneNetSynced {FocusedWorldNoneSync?.Count().ToString() ?? "Null"}
FirstTypeNoneNetSynced {FocusedWorldNoneSync?.FirstOrDefault().Value?.GetType()?.GetFormattedName() ?? "Null"}
===== -----------------------=====";
		}

	}
}
