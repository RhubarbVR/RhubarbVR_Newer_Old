using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Inspectors" })]
	public class EntityInspector : BaseInspector<Entity>
	{
		protected override void BuildUI() {
			var mainBox = Entity.AttachComponent<BoxContainer>();
			mainBox.Vertical.Value = false;
			mainBox.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			mainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var hyricryPannelField = Entity.AddChild("Hyricry").AttachComponent<Panel>();
			hyricryPannelField.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			hyricryPannelField.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var hyricryscrollField = hyricryPannelField.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
			hyricryscrollField.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			hyricryscrollField.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var EntityscrollField = Entity.AddChild("EntityView").AttachComponent<ScrollContainer>();
			EntityscrollField.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			EntityscrollField.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var worldObject = EntityscrollField.Entity.AddChild("ScrollChild").AttachComponent<WorldObjectInspector>();
			worldObject.TargetObject.Target = TargetObject.Target;
			if (worldObject.Entity.CanvasItem is UIElement uI) {
				uI.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
				uI.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			}
			var textLabel = worldObject.Entity.AddChild("compsLabel").AttachComponent<TextLabel>();
			textLabel.Text.Value = Engine.localisationManager.GetLocalString("Editor.Components");
			textLabel.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			textLabel.HorizontalAlignment.Value = RHorizontalAlignment.Center;
			textLabel.TextSize.Value = 18;

			var comps = worldObject.Entity.AddChild("comps");
			var compthing = comps.AttachComponent<SyncListInspector<SyncAbstractObjList<IComponent>>>();
			compthing.TargetObject.Target = TargetObject.Target?.components;
			var attachComp = worldObject.Entity.AddChild("comps").AttachComponent<Button>();
			attachComp.Pressed.Target = compthing.Add;
			attachComp.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			attachComp.Alignment.Value = RButtonAlignment.Center;
			attachComp.Text.Value = Engine.localisationManager.GetLocalString("Editor.AddComponent");
		}
	}
}