using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using Godot;
using RhuEngine.Components;
using static Godot.Control;
using System.Net.Sockets;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class ItemListLink : UIElementLinkBase<RhuEngine.Components.ItemList, Godot.ItemList>
	{
		public override string ObjectName => "ItemList";

		public override void StartContinueInit() {

			LinkedComp.SelectMode.Changed += SelectMode_Changed;
			LinkedComp.AllowReselect.Changed += AllowReselect_Changed;
			LinkedComp.AllowRMBSelect.Changed += AllowRMBSelect_Changed;
			LinkedComp.MaxTextLines.Changed += MaxTextLines_Changed;
			LinkedComp.AutoHeight.Changed += AutoHeight_Changed;
			LinkedComp.TextOverrun.Changed += TextOverrun_Changed;
			LinkedComp.Items.Changed += Items_Changed;
			LinkedComp.MaxColumns.Changed += MaxColumns_Changed;
			LinkedComp.SameWidthColumns.Changed += SameWidthColumns_Changed;
			LinkedComp.FixedWidthColumns.Changed += FixedWidthColumns_Changed;
			LinkedComp.IconMode.Changed += IconMode_Changed;
			LinkedComp.IconScale.Changed += IconScale_Changed;
			LinkedComp.FixedIconSize.Changed += FixedIconSize_Changed;
			SelectMode_Changed(null);
			AllowReselect_Changed(null);
			AllowRMBSelect_Changed(null);
			MaxTextLines_Changed(null);
			AutoHeight_Changed(null);
			TextOverrun_Changed(null);
			Items_Changed(null);
			MaxColumns_Changed(null);
			SameWidthColumns_Changed(null);
			FixedWidthColumns_Changed(null);
			IconMode_Changed(null);
			IconScale_Changed(null);
			FixedIconSize_Changed(null);
		}

		private void FixedIconSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FixedIconSize = new Vector2I(LinkedComp.FixedIconSize.Value.x, LinkedComp.FixedIconSize.Value.y));
		}

		private void IconScale_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.IconScale = LinkedComp.IconScale.Value);
		}

		private void IconMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.IconMode = LinkedComp.IconMode.Value switch { RItemListIconMode.Left => Godot.ItemList.IconModeEnum.Left, _ => Godot.ItemList.IconModeEnum.Top, });
		}

		private void FixedWidthColumns_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FixedColumnWidth = LinkedComp.FixedWidthColumns.Value);
		}

		private void SameWidthColumns_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SameColumnWidth = LinkedComp.SameWidthColumns.Value);
		}

		private void MaxColumns_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MaxColumns = LinkedComp.MaxColumns.Value);
		}

		//ToDO finishItems
		private void Items_Changed(IChangeable obj) {
		}

		private void TextOverrun_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextOverrunBehavior = LinkedComp.TextOverrun.Value switch { ROverrunBehavior.TrimChar => TextServer.OverrunBehavior.TrimChar, ROverrunBehavior.TrimWord => TextServer.OverrunBehavior.TrimWord, ROverrunBehavior.TrimEllipsis => TextServer.OverrunBehavior.TrimEllipsis, ROverrunBehavior.TrimWordEllipsis => TextServer.OverrunBehavior.TrimWordEllipsis, _ => TextServer.OverrunBehavior.NoTrimming, });
		}

		private void AutoHeight_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AutoHeight = LinkedComp.AutoHeight.Value);
		}

		private void MaxTextLines_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MaxTextLines = LinkedComp.MaxTextLines.Value);
		}

		private void AllowRMBSelect_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AllowRmbSelect = LinkedComp.AllowRMBSelect.Value);
		}

		private void AllowReselect_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AllowReselect = LinkedComp.AllowReselect.Value);
		}

		private void SelectMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SelectMode = LinkedComp.SelectMode.Value switch { RItemListSelectMode.Multi => Godot.ItemList.SelectModeEnum.Multi, _ => Godot.ItemList.SelectModeEnum.Single, });
		}
	}
}
