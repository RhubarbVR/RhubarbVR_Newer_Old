using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Channels;
using static RhuEngine.Components.UIWindow3D;
using Assimp;
using System.Linq;

namespace RhuEngine.Components
{
	[Category("UI")]
	[UpdateLevel(UpdateEnum.PlayerInput)]
	public sealed partial class UIWindow3D : Component
	{
		[OnChanged(nameof(MarkUodateMeshes))]
		public readonly Sync<Vector2i> Reslution;
		[Default(0.001f)]
		[OnChanged(nameof(MarkUodateMeshes))]
		public readonly Sync<float> PixelSize;

		[OnChanged(nameof(MarkUodateMeshes))]
		public readonly Linker<float> Width;
		[OnChanged(nameof(MarkUodateMeshes))]
		public readonly Linker<float> Height;

		[OnChanged(nameof(MarkUodateMeshes))]
		public readonly Linker<float> HeaderWidth;
		[OnChanged(nameof(MarkUodateMeshes))]
		public readonly Linker<float> HeaderHeight;

		[OnChanged(nameof(MarkUodateMeshes))]
		public readonly Linker<Vector3f> HeaderPosOffset;

		[Default(0.03f)]
		public readonly Sync<float> HeaderSize;

		public readonly SyncRef<Entity> HeaderRoot;

		protected override void Step() {
			base.Step();
			foreach (var item in Buttons.Cast<HeaderUIButton>()) {
				item.Step();
			}
		}

		public sealed partial class HeaderUIButton : SyncObject
		{
			private bool _hoveredLast;
			private bool _pressLast;

			public void Step() {
				if (MainShape.Target is null) {
					return;
				}
				if (MainShape.Target.LazeredThisFrame || MainShape.Target.TouchThisFrame) {
					Hover.Value = true;
					_hoveredLast = true;
					if (MainShape.Target.LazerPressForce > 0.5f || MainShape.Target.TouchThisFrame) {
						Press.Value = true;
						if (!_pressLast) {
							RUpdateManager.ExecuteOnEndOfUpdate(PressAction.Invoke);
						}
						_pressLast = true;
					}
					else {
						if (_pressLast) {
							Press.Value = false;
							_pressLast = false;
						}
					}
				}
				else {
					if (_hoveredLast) {
						Press.Value = false;
						Hover.Value = false;
						_hoveredLast = false;
						_pressLast = false;
					}
				}
			}

			private void MarkForUpdate() {
				if (Parent.Parent is UIWindow3D window) {
					window.MarkUodateMeshes();
				}
			}

			[OnChanged(nameof(AssetUpdate))]
			public readonly SyncRef<IAssetProvider<RTexture2D>> TextureIcon;

			[OnChanged(nameof(UpdateButtonColor))]
			public readonly Sync<bool> Hover;
			[OnChanged(nameof(UpdateButtonColor))]
			public readonly Sync<bool> Press;

			[OnChanged(nameof(UpdateButtonColor))]
			public readonly Sync<Colorf> ButtonColor;
			[OnChanged(nameof(UpdateButtonColor))]
			public readonly Sync<Colorf> ButtonPressColor;
			[OnChanged(nameof(UpdateButtonColor))]
			public readonly Sync<Colorf> ButtonHoverColor;

			public readonly SyncRef<UnlitMaterial> TargetIconMat;

			public readonly SyncRef<Entity> Button;
			public readonly SyncRef<UIPanelMesh> Pannel;
			public readonly SyncRef<PhysicsObject> MainShape;

			public readonly SyncDelegate PressAction;

			[OnChanged(nameof(MarkForUpdate))]
			public readonly Linker<Vector3f> Offset;
			[OnChanged(nameof(MarkForUpdate))]
			public readonly Linker<float> Width;
			[OnChanged(nameof(MarkForUpdate))]
			public readonly Linker<float> Height;

			[OnChanged(nameof(UpdateButtonColor))]
			public readonly Linker<Colorf> ButtonColorLink;

			private void UpdateButtonColor() {
				if (ButtonColorLink.Linked) {
					var lastColor = ButtonColor.Value;
					if (Hover.Value) {
						lastColor = ButtonHoverColor.Value;
					}
					if (Press.Value) {
						lastColor = ButtonPressColor.Value;
					}
					ButtonColorLink.LinkedValue = lastColor;
				}
			}

			protected override void FirstCreation() {
				base.FirstCreation();
				if (Parent.Parent is UIWindow3D window) {
					if (window.HeaderRoot.Target is null) {
						Destroy();
					}
					else {
						var headerButton = window.HeaderRoot.Target.AddChild("HeaderButton");
						var button = headerButton.AttachMeshWithMeshRender<UIPanelMesh, UnlitMaterial>();
						var meshShape = button.Item1.Entity.AttachComponent<MeshShape>();
						meshShape.CursorShape.Value = RCursorShape.PointingHand;
						MainShape.Target = meshShape;
						meshShape.TargetMesh.Target = button.Item1;
						Pannel.Target = button.Item1;
						Width.Target = button.Item1.Width;
						Height.Target = button.Item1.Height;
						Offset.Target = button.Item1.PosOffset;
						TargetIconMat.Target = button.Item2;

						button.Item2.Transparency.Value = Transparency.Blend;

						var mat = headerButton.AttachComponent<UnlitMaterial>();
						button.Item3.materials.Add().Target = mat;
						mat.DullSided.Value = true;
						mat.Transparency.Value = Transparency.Blend;
						mat.RenderPriority.Value = -1;

						ButtonColorLink.Target = mat.Tint;
						Button.Target = headerButton;
					}
				}
				else {
					Destroy();
				}
			}

			private void AssetUpdate() {
				if (TargetIconMat.Target is null) {
					return;
				}
				if (TargetIconMat.Target.MainTexture.Target != TextureIcon.Target) {
					TargetIconMat.Target.MainTexture.Target = TextureIcon.Target;
				}
			}

		}

		[OnChanged(nameof(MarkUodateMeshes))]
		public readonly SyncObjList<HeaderUIButton> Buttons;

		public void MarkUodateMeshes() {
			RUpdateManager.ExecuteOnEndOfUpdate(this, UpdateNow);
		}

		private void UpdateNow() {
			var screenWidth = Reslution.Value.x * PixelSize.Value;
			var screenHeight = Reslution.Value.y * PixelSize.Value;

			if (Height.Linked) {
				Height.LinkedValue = screenHeight;
			}
			if (Width.Linked) {
				Width.LinkedValue = screenWidth;
			}
			var leftOffset = (HeaderSize.Value + 0.035f) * Buttons.Count;
			if (HeaderWidth.Linked) {
				HeaderWidth.LinkedValue = screenWidth - leftOffset;
			}
			if (HeaderHeight.Linked) {
				HeaderHeight.LinkedValue = HeaderSize.Value;
			}
			if (HeaderPosOffset.Linked) {
				HeaderPosOffset.LinkedValue = new Vector3f(leftOffset, -(HeaderSize.Value + 0.035f));
			}
			foreach (var item in Buttons.Cast<HeaderUIButton>()) {
				leftOffset -= HeaderSize.Value + 0.035f;
				if (item.Width.Linked) {
					item.Width.LinkedValue = HeaderSize.Value;
				}
				if (item.Height.Linked) {
					item.Height.LinkedValue = HeaderSize.Value;
				}
				if (item.Offset.Linked) {
					item.Offset.LinkedValue = new Vector3f(leftOffset, -(HeaderSize.Value + 0.035f));
				}
			}
		}
		[OnChanged(nameof(CollapseUpdate))]
		public readonly Sync<bool> Collapse;
		[OnChanged(nameof(CollapseUpdate))]
		public readonly Linker<bool> CollapseEntity;
		[OnChanged(nameof(CollapseUpdate))]
		public readonly Linker<RhubarbAtlasSheet.RhubarbIcons> CollapseIcon;

		public readonly SyncRef<UIMeshShape> MainUI;

		public readonly SyncRef<UnlitMaterial> MainUIMat;

		[Exposed]
		public void CollapseToggle() {
			Collapse.Value = !Collapse.Value;
		}

		public void CollapseUpdate() {
			if (CollapseEntity.Linked) {
				CollapseEntity.LinkedValue = !Collapse.Value;
			}
			if (CollapseIcon.Linked) {
				CollapseIcon.LinkedValue = Collapse.Value ? RhubarbAtlasSheet.RhubarbIcons.Uncollapse : RhubarbAtlasSheet.RhubarbIcons.Collapse;
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			Entity.AttachComponent<Grabbable>();
			var mainPannel = Entity.AddChild("MainPannel").AttachMeshWithMeshRender<UIPanelMesh, UnlitMaterial>();
			var mainShape = mainPannel.Item1.Entity.AttachComponent<UIMeshShape>();
			mainShape.FlipY.Value = true;
			mainShape.TargetMesh.Target = mainPannel.Item1;
			MainUI.Target = mainShape;
			var mainMat = mainPannel.Item2;
			MainUIMat.Target = mainMat;
			CollapseEntity.Target = mainPannel.Item1.Entity.enabled;
			Height.Target = mainPannel.Item1.Height;
			Width.Target = mainPannel.Item1.Width;
			mainPannel.Item2.Transparency.Value = Transparency.Blend;
			var backGround = Entity.AttachComponent<UnlitMaterial>();
			mainPannel.Item3.materials.Add().Target = backGround;
			var color = Colorf.RhubarbGreen;
			if (World.IsPersonalSpace) {
				color = Colorf.RhubarbRed;
			}
			if (World.IsOverlayWorld) {
				color = Colorf.SelectionGold;
			}
			backGround.Tint.Value = color * new Colorf(0.5f, 0.5f, 0.5f, 0.9f);

			backGround.Transparency.Value = Transparency.Blend;
			backGround.DullSided.Value = true;
			backGround.RenderPriority.Value = -1;

			HeaderRoot.Target = Entity.AddChild("HeaderRoot");
			var headerPan = HeaderRoot.Target.AttachMeshWithMeshRender<UIPanelMesh, UnlitMaterial>();
			HeaderHeight.Target = headerPan.Item1.Height;
			HeaderWidth.Target = headerPan.Item1.Width;
			HeaderPosOffset.Target = headerPan.Item1.PosOffset;
			headerPan.Item2.Transparency.Value = Transparency.Blend;
			headerPan.Item2.Tint.Value = backGround.Tint.Value;
			var meshShape = HeaderRoot.Target.AttachComponent<MeshShape>();
			meshShape.CursorShape.Value = RCursorShape.Move;
			meshShape.TargetMesh.Target = headerPan.Item1;

			var clapsButton = Buttons.Add();
			clapsButton.ButtonColor.Value = Colorf.DarkGray * new Colorf(0.7f, 0.7f, 0.7f, 0.85f);
			clapsButton.ButtonHoverColor.Value = Colorf.DarkGray * new Colorf(0.8f, 0.8f, 0.8f, 0.85f);
			clapsButton.ButtonPressColor.Value = Colorf.DarkGray * new Colorf(0.9f, 0.9f, 0.9f, 0.85f);
			clapsButton.PressAction.Target = CollapseToggle;
			var clapsTextue = Entity.AttachComponent<SingleIconTex>();
			clapsTextue.MaxUV.Target = clapsButton.Pannel.Target.UVMax;
			clapsTextue.MinUV.Target = clapsButton.Pannel.Target.UVMin;

			clapsTextue.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Collapse;
			CollapseIcon.Target = clapsTextue.Icon;
			clapsButton.TextureIcon.Target = clapsTextue;

			var closeButton = Buttons.Add();
			var closeTextue = Entity.AttachComponent<SingleIconTex>();
			closeTextue.MaxUV.Target = closeButton.Pannel.Target.UVMax;
			closeTextue.MinUV.Target = closeButton.Pannel.Target.UVMin;
			closeTextue.Icon.Value = RhubarbAtlasSheet.RhubarbIcons.Close;
			closeButton.TextureIcon.Target = closeTextue;
			closeButton.ButtonColor.Value = Colorf.Red * new Colorf(0.7f, 0.7f, 0.7f, 0.85f);
			closeButton.ButtonHoverColor.Value = Colorf.Red * new Colorf(0.8f, 0.8f, 0.8f, 0.85f);
			closeButton.ButtonPressColor.Value = Colorf.Red * new Colorf(0.9f, 0.9f, 0.9f, 0.85f);
			closeButton.PressAction.Target = Close;
			Reslution.Value = new Vector2i(1080, 720);
		}

		public event Action OnClose;

		[Exposed]
		public void Close() {
			OnClose?.Invoke();
			Entity.Destroy();
		}
	}
}
