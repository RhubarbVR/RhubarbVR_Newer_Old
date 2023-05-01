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
	public sealed partial class EntityInspector : BaseInspector<Entity>
	{
		[OnChanged(nameof(Name_Changed))]
		public readonly Linker<string> TopName;

		public readonly SyncRef<EntityHierarchy> Hierarchy;

		private Entity _entity;
		public override void LocalBind() {
			base.LocalBind();
			if (_entity is not null) {
				_entity.OnDispose -= Entity_OnDispose;
				_entity.name.Changed -= Name_Changed;
				if ((_entity.IsDestroying | _entity.IsRemoved) & TargetObject.Target is null) {
					TargetObject.Target = World.RootEntity;
				}
			}
			_entity = TargetObject.Target;
			if (_entity is not null) {
				_entity.OnDispose += Entity_OnDispose;
				_entity.name.Changed += Name_Changed;
			}
		}

		private void Name_Changed(IChangeable obj) {
			if (TopName.Linked) {
				TopName.LinkedValue = TargetObject.Target is null ? "NULL" : $"{TargetObject.Target.Name} ({TargetObject.Target.Pointer.HexString()})";
			}
		}

		private void Entity_OnDispose(object obj) {
			if ((TargetObject.Target?.IsDestroying ?? true) | (TargetObject.Target?.IsRemoved ?? true)) {
				TargetObject.Target = World.RootEntity;
			}
		}

		protected override void BuildUI() {
			var mainBox = Entity.GetFirstComponentOrAttach<BoxContainer>();
			mainBox.Vertical.Value = false;
			mainBox.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			mainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var hyricryPannelField = Entity.AddChild("Hyricry").AttachComponent<Panel>();
			hyricryPannelField.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			hyricryPannelField.HorizontalFilling.Value = RFilling.Fill;
			hyricryPannelField.MinSize.Value = new Vector2i(250, 0);
			var hyricryscrollField = hyricryPannelField.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
			hyricryscrollField.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			hyricryscrollField.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var boxContainer = hyricryscrollField.Entity.AddChild("Hyraricy").AttachComponent<BoxContainer>();
			boxContainer.Vertical.Value = true;
			boxContainer.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			var boxContainerSided = boxContainer.Entity.AddChild("Hyraricy").AttachComponent<BoxContainer>();
			boxContainerSided.Vertical.Value = false;
			boxContainerSided.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			
			Button AddButtonIconHy(Action action, RhubarbAtlasSheet.RhubarbIcons rhubarbIcons) {
				var button = boxContainerSided.Entity.AddChild("Button").AttachComponent<Button>();
				button.MinSize.Value = new Vector2i(45);
				var icon = button.Entity.AttachComponent<SingleIconTex>();
				icon.Icon.Value = rhubarbIcons;
				button.Icon.Target = icon;
				button.ExpandIcon.Value = true;
				button.IconAlignment.Value = RButtonAlignment.Center;
				if (action != null) {
					button.Pressed.Target = action;
				}
				return button;
			}
			AddButtonIconHy(HierarchyToParent, RhubarbAtlasSheet.RhubarbIcons.UpArrow);
			
			var hyraricyText = boxContainerSided.Entity.AddChild("Text").AttachComponent<TextLabel>();
			hyraricyText.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			hyraricyText.TextSize.Value = 15;

			var EntityscrollField = Entity.AddChild("EntityView").AttachComponent<ScrollContainer>();
			EntityscrollField.VerticalFilling.Value = RFilling.Expand | RFilling.Fill;
			EntityscrollField.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var worldObject = EntityscrollField.Entity.AddChild("ScrollChild").AttachComponent<WorldObjectInspector>();

			var hyraicyBit = boxContainer.Entity.AddChild("FirstHyraricyElement").AttachComponent<EntityHierarchy>();
			Hierarchy.Target = hyraicyBit;
			hyraicyBit.TargetObject.Target = TargetObject.Target;
			hyraicyBit.TargetWorldObjectInspector.Target = this;
			hyraicyBit.ExtraTopName.Target = hyraricyText.Text;
			try {
				hyraicyBit.DropDown.Target.DropDownButton.Target.ButtonPressed.Value = true;
			}
			catch { }

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

			var headeren = worldObject.Entity.AddChild("Header");
			headeren.orderOffset.Value = -1;
			var header = headeren.AttachComponent<BoxContainer>();
			header.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			Button AddButtonIcon(Action action, RhubarbAtlasSheet.RhubarbIcons rhubarbIcons) {
				var button = headeren.AddChild("Button").AttachComponent<Button>();
				button.MinSize.Value = new Vector2i(45);
				var icon = button.Entity.AttachComponent<SingleIconTex>();
				icon.Icon.Value = rhubarbIcons;
				button.Icon.Target = icon;
				button.ExpandIcon.Value = true;
				button.IconAlignment.Value = RButtonAlignment.Center;
				if (action != null) {
					button.Pressed.Target = action;
				}
				return button;
			}

			AddButtonIcon(TargetObject.Target.Destroy, RhubarbAtlasSheet.RhubarbIcons.Trash);
			AddButtonIcon(AddChild, RhubarbAtlasSheet.RhubarbIcons.Plus);
			AddButtonIcon(AddParent, RhubarbAtlasSheet.RhubarbIcons.DownArrow);

			var text = headeren.AddChild("TopText").AttachComponent<TextLabel>();
			text.TextSize.Value = 17;
			TopName.Target = text.Text;
			text.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

		}

		[Exposed]
		public void HierarchyToParent() {
			if(Hierarchy.Target is null) {
				return;
			}
			Hierarchy.Target.TargetObject.Target = (Hierarchy.Target.TargetObject.Target?.parent.Target??World.RootEntity);
		}

		[Exposed]
		public void AddParent() {
			if (TargetObject.Target is null) {
				return;
			}
			if(TargetObject.Target.InternalParent is null) {
				return;
			}
			var local = TargetObject.Target.LocalTrans;
			var newParent = TargetObject.Target.InternalParent.AddChild(TargetObject.Target.name.Value + " Parent");
			newParent.LocalTrans = local;
			TargetObject.Target.parent.Target = newParent;
			TargetObject.Target.LocalTrans = Matrix.Identity;
			TargetObject.Target = newParent;
		}

		[Exposed]
		public void AddChild() {
			if (TargetObject.Target is null) {
				return;
			}
			TargetObject.Target = TargetObject.Target.AddChild(TargetObject.Target.name.Value + " Child");
		}
	}
}