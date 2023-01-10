using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using Godot;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public interface ICanvasItemNodeLinked
	{
		CanvasItem CanvasItem { get; }

		void Children_OnReorderList();
		void UpdateParrent();
	}

	public abstract class CanvasItemNodeLinked<T, T2> : EngineWorldLinkBase<T>, RhuEngine.Components.ICanvasItemLinked, ICanvasItemNodeLinked where T : RhuEngine.Components.CanvasItem, new() where T2 : CanvasItem, new()
	{
		public T2 node;

		public event Action VisibilityChanged
		{
			add {
				if (node is null) {
					return;
				}
				node.VisibilityChanged += value;
			}
			remove {
				if (node is null) {
					return;
				}
				node.VisibilityChanged -= value;
			}
		}
		public event Action Hidden { add { if (node is null) { return; } node.Hidden += value; } remove { if (node is null) { return; } node.Hidden -= value; } }
		public event Action ItemRectChanged { add { if (node is null) { return; } node.ItemRectChanged += value; } remove { if (node is null) { return; } node.ItemRectChanged -= value; } }

		public abstract string ObjectName { get; }

		public CanvasItem CanvasItem => node;



		public override void Init() {
			node = new T2 {
				Name = ObjectName
			};
			LinkedComp.Entity.ViewportUpdateEvent += UpdateParrent;
			LinkedComp.Entity.CanvasItemUpdateEvent += UpdateParrent;
			LinkedComp.Entity.children.OnReorderList += Children_OnReorderList;
			LinkedComp.Entity.OnParentChanged += UpdateParrent;
			UpdateParrent();
			LoadCanvasItemLink();
			StartContinueInit();
			Children_OnReorderList();
		}

		public void Children_OnReorderList() {
			if (node is null) {
				return;
			}
			if (LinkedComp is null) {
				return;
			}
			if (LinkedComp.IsDestroying | LinkedComp.IsRemoved) {
				return;
			}
			for (var i = 0; i < LinkedComp.Entity.children.Count; i++) {
				var item = LinkedComp.Entity.children[i];
				if (item?.CanvasItem?.WorldLink is ICanvasItemNodeLinked canvasItem) {
					if (canvasItem?.CanvasItem?.GetParent() == CanvasItem) {
						node.MoveChild(canvasItem.CanvasItem, -1);
					}
					else {
						if (CanvasItem == canvasItem?.CanvasItem) {
							continue;
						}
						if (canvasItem.CanvasItem is null) {
							continue;
						}
						canvasItem.CanvasItem?.GetParent()?.RemoveChild(canvasItem.CanvasItem);
						CanvasItem.AddChild(canvasItem.CanvasItem);
						canvasItem.CanvasItem.Owner = CanvasItem;
						CanvasItem.MoveChild(canvasItem.CanvasItem, -1);
					}
				}
			}
		}


		public virtual void UpdateParrent() {
			if (node is null) {
				return;
			}
			if (LinkedComp is null) {
				return;
			}
			if (LinkedComp.IsDestroying | LinkedComp.IsRemoved) {
				return;
			}
			if (node.Owner is null) {
				if (LinkedComp.Entity.Viewport?.WorldLink is ViewportLink ee && (LinkedComp.Entity?.InternalParent?.CanvasItem is null)) {
					ee.node.AddChild(node);
					node.Owner = ee.node;
				}
				else if (LinkedComp.Entity.InternalParent?.Viewport?.WorldLink is ViewportLink eee && (LinkedComp.Entity?.InternalParent?.CanvasItem is null)) {
					eee.node.AddChild(node);
					node.Owner = eee.node;
				}
				else {
					if (LinkedComp.World.IsPersonalSpace) {
						EngineRunnerHelpers._.AddChild(node);
						node.Owner = EngineRunnerHelpers._;
					}
					else {
						EngineRunnerHelpers._.ThowAway.AddChild(node);
						node.Owner = EngineRunnerHelpers._.ThowAway;
					}
				}
			}
			if (LinkedComp.Entity?.InternalParent?.CanvasItem?.WorldLink is ICanvasItemNodeLinked linked) {
				linked.Children_OnReorderList();
			}
		}

		public abstract void StartContinueInit();

		public override void Remove() {
			LinkedComp.Entity.ViewportUpdateEvent -= UpdateParrent;
			LinkedComp.Entity.CanvasItemUpdateEvent -= UpdateParrent;
			if (LinkedComp.Entity.children is not null) {
				LinkedComp.Entity.children.OnReorderList -= Children_OnReorderList;
			}
			LinkedComp.Entity.OnParentChanged -= UpdateParrent;
			node?.QueueFree();
			node = null;
		}

		public override void Started() {
			if (node is null) {
				return;
			}
			node.Visible = true;
		}

		public override void Stopped() {
			if (node is null) {
				return;
			}
			node.Visible = false;
		}

		private void LoadCanvasItemLink() {
			LinkedComp.Modulate.Changed += Modulate_Changed;
			LinkedComp.ModulateSelf.Changed += ModulateSelf_Changed;
			LinkedComp.ShowBehindParent.Changed += ShowBehindParent_Changed;
			LinkedComp.TopLevel.Changed += TopLevel_Changed;
			LinkedComp.ClipChildren.Changed += ClipChildren_Changed;
			LinkedComp.LightMask.Changed += Mask_Changed;
			LinkedComp.Filter.Changed += Filter_Changed;
			LinkedComp.Repeat.Changed += Repeat_Changed;
			LinkedComp.UseParentMaterial.Changed += UseParentMaterial_Changed;
			LinkedComp.Material.LoadChange += Material_LoadChange;
			Modulate_Changed(null);
			ModulateSelf_Changed(null);
			ShowBehindParent_Changed(null);
			TopLevel_Changed(null);
			ClipChildren_Changed(null);
			Mask_Changed(null);
			Filter_Changed(null);
			Repeat_Changed(null);
			UseParentMaterial_Changed(null);
			Material_LoadChange(null);
		}

		private void Material_LoadChange(RMaterial obj) {
			node.Material = LinkedComp.Material?.Asset?.Target is GodotMaterial godotMaterial ? (godotMaterial?.Material) : null;
		}

		private void UseParentMaterial_Changed(IChangeable obj) {
			node.UseParentMaterial = LinkedComp.UseParentMaterial.Value;
		}

		private void Repeat_Changed(IChangeable obj) {
			node.TextureRepeat = LinkedComp.Repeat.Value switch {
				RhuEngine.Components.RElementTextureRepeat.Disable => CanvasItem.TextureRepeatEnum.Disabled,
				RhuEngine.Components.RElementTextureRepeat.Enabled => CanvasItem.TextureRepeatEnum.Enabled,
				RhuEngine.Components.RElementTextureRepeat.Mirror => CanvasItem.TextureRepeatEnum.Mirror,
				_ => CanvasItem.TextureRepeatEnum.ParentNode,
			};
		}

		private void Filter_Changed(IChangeable obj) {
			node.TextureFilter = LinkedComp.Filter.Value switch {
				RhuEngine.Components.RElementTextureFilter.Nearest => CanvasItem.TextureFilterEnum.Nearest,
				RhuEngine.Components.RElementTextureFilter.Linear => CanvasItem.TextureFilterEnum.Linear,
				RhuEngine.Components.RElementTextureFilter.LinearMipmap => CanvasItem.TextureFilterEnum.LinearWithMipmaps,
				RhuEngine.Components.RElementTextureFilter.NearestMipmap => CanvasItem.TextureFilterEnum.NearestWithMipmaps,
				RhuEngine.Components.RElementTextureFilter.NearestMipmapAnisotropic => CanvasItem.TextureFilterEnum.NearestWithMipmapsAnisotropic,
				RhuEngine.Components.RElementTextureFilter.LinearMipmapAnisotropic => CanvasItem.TextureFilterEnum.LinearWithMipmapsAnisotropic,
				_ => CanvasItem.TextureFilterEnum.ParentNode,
			};
		}

		private void Mask_Changed(IChangeable obj) {
			node.LightMask = (int)LinkedComp.LightMask.Value;
		}

		private void ClipChildren_Changed(IChangeable obj) {
			node.ClipChildren = (CanvasItem.ClipChildrenMode)LinkedComp.ClipChildren.Value;
		}

		private void TopLevel_Changed(IChangeable obj) {
			node.TopLevel = LinkedComp.TopLevel.Value;
		}

		private void ShowBehindParent_Changed(IChangeable obj) {
			node.ShowBehindParent = LinkedComp.ShowBehindParent.Value;
		}

		private void ModulateSelf_Changed(IChangeable obj) {
			node.SelfModulate = new Color(LinkedComp.ModulateSelf.Value.r, LinkedComp.ModulateSelf.Value.g, LinkedComp.ModulateSelf.Value.b, LinkedComp.ModulateSelf.Value.a);
		}

		private void Modulate_Changed(IChangeable obj) {
			node.Modulate = new Color(LinkedComp.Modulate.Value.r, LinkedComp.Modulate.Value.g, LinkedComp.Modulate.Value.b, LinkedComp.Modulate.Value.a);
		}
	}



	public abstract class WorldNodeLinked<T, T2> : EngineWorldLinkBase<T> where T : LinkedWorldComponent, new() where T2 : Node, new()
	{
		public T2 node;
		public abstract string ObjectName { get; }
		public virtual bool GoToEngineRoot => true;

		public override void Init() {
			node = new T2 {
				Name = ObjectName
			};
			LinkedComp.Entity.ViewportUpdateEvent += Entity_ViewportUpdateEvent;
			Entity_ViewportUpdateEvent();
			StartContinueInit();
		}

		public virtual void Entity_ViewportUpdateEvent() {
			node.GetParent()?.RemoveChild(node);
			if (LinkedComp.Entity.Viewport?.WorldLink is ViewportLink ee) {
				if (LinkedComp.Entity.Viewport != LinkedComp) {
					ee.node.AddChild(node);
					node.Owner = ee.node;
				}
				else {
					if (GoToEngineRoot) {
						EngineRunnerHelpers._.AddChild(node);
						node.Owner = EngineRunnerHelpers._;
					}
					else {
						EngineRunnerHelpers._.ThowAway.AddChild(node);
						node.Owner = EngineRunnerHelpers._.ThowAway;
					}

				}
			}
			else {
				if (GoToEngineRoot) {
					EngineRunnerHelpers._.AddChild(node);
					node.Owner = EngineRunnerHelpers._;
				}
				else {
					EngineRunnerHelpers._.ThowAway.AddChild(node);
					node.Owner = EngineRunnerHelpers._.ThowAway;
				}
			}
		}

		public abstract void StartContinueInit();

		public override void Remove() {
			LinkedComp.Entity.ViewportUpdateEvent -= Entity_ViewportUpdateEvent;
			node?.QueueFree();
			node = null;
		}


	}


	public abstract class WorldPositionLinked<T, T2> : WorldNodeLinked<T, T2> where T : LinkedWorldComponent, new() where T2 : Node3D, new()
	{
		public override void Init() {
			base.Init();
			node.TopLevel = true;
			node.RotationEditMode = Node3D.RotationEditModeEnum.Basis;
			LinkedComp.Entity.GlobalTransformChange += Entity_GlobalTransformChange;
			StartContinueInit();
			UpdatePosThisFrame = true;
		}

		public override void Remove() {
			LinkedComp.Entity.GlobalTransformChange -= Entity_GlobalTransformChange;
			base.Remove();
		}


		public override void Started() {
			if (node is null) {
				return;
			}

			node.Visible = true;
		}

		public override void Stopped() {
			if (node is null) {
				return;
			}
			node.Visible = false;
		}

		public bool UpdatePosThisFrame { get; private set; } = true;

		private void Entity_GlobalTransformChange(Entity obj, bool data) {
			UpdatePosThisFrame = true;
		}

		public override void Render() {
			if (UpdatePosThisFrame) {
				node.SetPos(LinkedComp.Entity.GlobalTrans);
				UpdatePosThisFrame = false;
			}
		}
	}
}
