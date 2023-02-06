using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Inspectors" })]
	[UpdatingComponent]
	public class EntityHierarchy : BaseInspector<Entity>
	{
		protected override bool MultiThreaded => false;

		[OnChanged(nameof(Name_Changed))]
		public readonly Linker<string> TopName;

		[OnChanged(nameof(Name_Changed))]
		public readonly Linker<string> ExtraTopName;
		public readonly SyncRef<EntityInspector> TargetWorldObjectInspector;
		private Entity _entity;
		public override void LocalBind() {
			base.LocalBind();
			if (_entity is not null) {
				_entity.OnDispose -= Entity_OnDispose;
				_entity.name.Changed -= Name_Changed;
				_entity.children.Changed -= ChildrenUpdate;
				if ((_entity.IsDestroying | _entity.IsRemoved) & TargetObject.Target is null) {
					TargetObject.Target = World.RootEntity;
				}
			}
			_entity = TargetObject.Target;
			if (_entity is not null) {
				_entity.OnDispose += Entity_OnDispose;
				_entity.name.Changed += Name_Changed;
				_entity.children.Changed += ChildrenUpdate;
				Name_Changed(null);
				ChildrenUpdate(null);
			}
		}

		private void Name_Changed(IChangeable obj) {
			var outPutValue = TargetObject.Target is null ? "NULL" : $"{TargetObject.Target.Name} ({TargetObject.Target.Pointer.HexString()})"; 
			if (TopName.Linked) {
				TopName.LinkedValue = outPutValue;
			}
			if (ExtraTopName.Linked) {
				ExtraTopName.LinkedValue = outPutValue;
			}
		}

		private void Entity_OnDispose(object obj) {
			if ((TargetObject.Target?.IsDestroying ?? true) | (TargetObject.Target?.IsRemoved ?? true)) {
				Entity.Destroy();
			}
		}
		public readonly SyncRef<ButtonBase> TargetButton;

		[Exposed]
		public void GetRef() {
			if (TargetButton.Target is not null) {
				try {
					PrivateSpaceManager.GetGrabbableHolder(TargetButton.Target.LastHanded).GetGrabbableHolderFromWorld(World).Referencer.Target = TargetObject.Target;
				}
				catch {
				}
			}
		}
		public readonly SyncRef<DropDown> DropDown;

		private bool _isOpenLastFrame = false;

		protected override void Step() {
			base.Step();
			if (LocalUser != MasterUser) {
				return;
			}
			if (DropDown.Target?.DropDownButton.Target is null) {
				return;
			}
			if (_isOpenLastFrame != DropDown.Target.DropDownButton.Target.ButtonPressed.Value) {
				_isOpenLastFrame = DropDown.Target.DropDownButton.Target.ButtonPressed.Value;
				if (_isOpenLastFrame) {
					ChildrenUpdate(null);
				}
			}
		}

		private void ChildrenUpdate(IChangeable changeable) {
			if (LocalUser != MasterUser) {
				return;
			}
			if (IsRemoved | IsDestroying) {
				return;
			}
			if (TargetObject.Target is null) {
				return;
			}
			if (DropDown.Target?.DropDownData.Target is null) {
				return;
			}
			if (!(DropDown.Target?.DropDownButton.Target?.ButtonPressed.Value ?? false)) {
				return;
			}
			if (DropDown.Target.DropDownData.Target.children.Count < 2) {
				return;
			}
			var allOtherEntityHyro = (from ent in DropDown.Target.DropDownData.Target.children[1].children.Cast<Entity>()
									  let targetElement = ent.GetFirstComponent<EntityHierarchy>()
									  where targetElement is not null
									  select targetElement).ToArray();
			EntityHierarchy FindRelation(Entity entity) {
				foreach (var item in allOtherEntityHyro) {
					if (item.TargetObject.Target == entity) {
						return item;
					}
				}
				return null;
			}
			var index = 1;
			foreach (var item in TargetObject.Target.children.Cast<Entity>()) {
				var trains = FindRelation(item);
				var hraicty = trains is null
					? DropDown.Target.DropDownData.Target.children[1].AddChild(item.Pointer.HexString()).AttachComponent<EntityHierarchy>()
					: trains;
				if (hraicty.TargetObject.Target != item) {
					hraicty.TargetObject.Target = item;
				}
				if (hraicty.TargetWorldObjectInspector.Target != TargetWorldObjectInspector.Target) {
					hraicty.TargetWorldObjectInspector.Target = TargetWorldObjectInspector.Target;
				}
				hraicty.Entity.orderOffset.Value = index;
				index++;
			}
		}
		[Exposed]
		public void SetTarget() {
			if (TargetWorldObjectInspector.Target is null) {
				return;
			}
			TargetWorldObjectInspector.Target.TargetObject.Target = TargetObject.Target;
		}

		public override void Dispose() {
			if (_entity is not null) {
				_entity.OnDispose -= Entity_OnDispose;
				_entity.name.Changed -= Name_Changed;
				_entity.children.Changed -= ChildrenUpdate;
			}
			base.Dispose();
		}

		protected override void BuildUI() {
			_isOpenLastFrame = false;
			var box = Entity.GetFirstComponentOrAttach<BoxContainer>();
			var dropDown = box.Entity.AddChild().AttachComponent<DropDown>();
			if (dropDown?.DropDownButton?.Target is null) {
				return;
			}
			dropDown.DropDownButton.Target.MinSize.Value = new Vector2i(25, 0);
			dropDown.DropDownButton.Target.HorizontalFilling.Value = RFilling.Fill;
			var switcher = dropDown.DropDownButton.Target.Entity.AttachComponent<ValueFieldCopyTernary<string>>();
			switcher.Condition.Target = dropDown.DropDownButton.Target.ButtonPressed;
			switcher.True.Value = "/\\";
			switcher.False.Value = "\\/";
			switcher.Target.Target = dropDown.DropDownButton.Target.Text;
			var mainButton = dropDown.DropDownHeader.Target.AddChild("Child").AttachComponent<Button>();
			TopName.Target = mainButton.Text;
			mainButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			mainButton.Pressed.Target = SetTarget;
			var button = mainButton.Entity.AddChild().AttachComponent<ButtonBase>();
			TargetButton.Target = button;
			button.InputFilter.Value = RInputFilter.Pass;
			button.ButtonMask.Value = RButtonMask.Secondary;
			button.ButtonDown.Target = GetRef;
			var otherBox = dropDown.DropDownData.Target.AttachComponent<BoxContainer>();
			otherBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			otherBox.Entity.AddChild("Spacer").AttachComponent<UIElement>().MinSize.Value = new Vector2i(15, 0);
			var boxElement = otherBox.Entity.AddChild().AttachComponent<BoxContainer>();
			boxElement.Vertical.Value = true;
			boxElement.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			DropDown.Target = dropDown;
		}

	}
}