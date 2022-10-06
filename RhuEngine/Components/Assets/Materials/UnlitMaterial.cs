using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	[AllowedOnWorldRoot]
	public sealed class UnlitMaterial : MaterialBase<IUnlitMaterial>
	{
		[OnChanged(nameof(TransparencyUpdate))]
		public readonly Sync<Transparency> Transparency;

		[OnChanged(nameof(TintUpdate))]
		public readonly Sync<Colorf> Tint;

		[OnChanged(nameof (DullSidedUpdate))]
		public readonly Sync<bool> DullSided;

		[OnAssetLoaded(nameof(TextureUpdate))]
		public readonly AssetRef<RTexture2D> MainTexture;

		protected override void OnAttach() {
			base.OnAttach();
			Tint.Value = Colorf.White;
			Transparency.Value = Linker.Transparency.None;
		}

		protected override void UpdateAll() {
			TransparencyUpdate();
			TextureUpdate();
			TintUpdate();
			DullSidedUpdate();
		}

		private void DullSidedUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.DullSided = DullSided.Value;
				_material.Material?.UpdatePrams();
			});
		}

		private void TextureUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.Texture = MainTexture.Asset;
				_material.Material?.UpdatePrams();
			});
		}
		private void TintUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.Tint = Tint;
				_material.Material?.UpdatePrams();
			});
		}
		private void TransparencyUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.Transparency = Transparency;
				_material.Material?.UpdatePrams();
			});
		}
	}
}
