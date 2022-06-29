//using RhuEngine.WorldObjects;
//using RhuEngine.WorldObjects.ECS;

//using StereoKit;
//using RhuEngine.Components.ScriptNodes;
//using System;
//using RhuEngine.DataStructure;
//using SharedModels;
//using System.Collections.Generic;
//using System.Linq;

//namespace RhuEngine.Components
//{
//	[Category(new string[] { "RhuScript/ScriptBuilders/VisualForm" })]
//	public class NodeButton : UIComponent
//	{
//		public SyncRef<Node> Node;
//		public Sync<Type> TargetType;
//		public Sync<bool> IsOutput;
//		public Sync<string> ToolTipText;
//		public Sync<float> Level;
//		public Sync<bool> IsClicked;
//		public Sync<bool> RenderLabel;
//		public SyncDelegate<Action<bool, NodeButton, Type>> Clicked;
//		[OnChanged(nameof(ConnectedToChange))]
//		public SyncRef<NodeButton> ConnectedTo;
//		[NoLoad][NoSave][NoSync][NoSyncUpdate]
//		NodeButton _loadedTo;
//		private void ConnectedToChange() {
//			if(_loadedTo != ConnectedTo.Target) {
//				_loadedTo?.ConnectFrom.Remove(this);
//			}
//			ConnectedTo.Target?.ConnectFrom.Add(this);
//			_loadedTo = ConnectedTo.Target;
//		}

//		public override void Dispose() {
//			if (_loadedTo != null) {
//				_loadedTo?.ConnectFrom.Remove(this);
//			}
//			ConnectedTo.Target?.ConnectFrom.Add(this);
//			_loadedTo = null;
//			base.Dispose();
//		}

//		public HashSet<NodeButton> ConnectFrom = new();

//		public Pose LastGlobalPos; // Way to get other nodes pos
//		public override void RenderUI() {
//			var typecolor = TargetType.Value.GetTypeColor();
//			var typecolor2 = (ConnectedTo.Target?.TargetType.Value).GetTypeColor();
//			UI.PushTint(typecolor * 2f);
//			UI.PushId(Pointer.GetHashCode());
//			var enabled = IsClicked.Value;
//			var ypos = Level - Engine.UISettings.padding;
//			var buttonSize = 0.025f;
//			var pos = IsOutput
//				? (UI.LayoutAt - UI.LayoutRemaining.XY0).X0Z + new Vec3(0, ypos, 0)
//				: UI.LayoutAt + new Vec3((Engine.UISettings.padding * 1.35f) + (0.025f / 2), ypos + Engine.UISettings.padding, 0);
//			var centerPos = pos + new Vec3(-buttonSize / 2, -buttonSize / 2, 0);
//			LastGlobalPos = Hierarchy.ToWorld(new Pose(centerPos, Quat.Identity));
//			if (Helper.IsHovering(FingerId.Index, JointId.Tip, pos, new Vec3((Engine.UISettings.padding + buttonSize) / 2, (Engine.UISettings.padding + buttonSize) / 2, buttonSize * 3), out var hand)) {
//				var textpos = Input.Hand(hand).Get(FingerId.Index, JointId.Tip).Pose.position;
//				Hierarchy.Enabled = false;
//				Text.Add($"{ToolTipText.Value}\n Type: {TargetType.Value.GetFormattedName()}", Matrix.TS((hand == Handed.Right)?0.1f:-0.1f, -0.01f, -0.05f, 0.9f) * new Pose(textpos, Quat.LookAt(textpos, Input.Head.position)).ToMatrix(), Engine.MainTextStyle);
//				Hierarchy.Enabled = true;
//			}

//			if (UI.ToggleAt(" ", ref enabled, pos, new Vec2(buttonSize))) {
//				AddWorldCoroutine(() => Clicked.Target?.Invoke(enabled, this, TargetType));
//			}
//			UI.PopId();
//			UI.PopTint();
//			if (RenderLabel) {
//				UI.Label(ToolTipText.Value);
//			}

//			if (ConnectedTo.Target is not null) {
//				// Whould like to make better at some point 
//				var startPos = centerPos;
//				var endPos = Hierarchy.ToLocal(ConnectedTo.Target.LastGlobalPos);
//				var lasttarget = startPos;
//				var m = startPos.Dist(endPos.position) * 0.5f;
//				var handle1 = (-Vec3.Right * m) + startPos;
//				var handle2 = (endPos.Right * m) + endPos.position;
//				var CurveSteps = (m * 100);
//				for (var i = 0; i < CurveSteps; i++) {
//					var poser = (float)(i) / ((float)CurveSteps);
//					var current = Helper.Bezier(startPos, handle1, handle2, endPos.position, poser);
//					Lines.Add(lasttarget, current + ((lasttarget - current).Normalized * 0.01f), ((i % 2) == 1) ? typecolor: typecolor2, 0.01f);
//					lasttarget = current;
//				}
//				Lines.Add(lasttarget, endPos.position, typecolor, 0.01f);
//			}
//		}
//	}
//}
