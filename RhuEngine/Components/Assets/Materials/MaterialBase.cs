using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	public abstract partial class MaterialBase<T> : AssetProvider<RMaterial> where T : RMaterial, new()
	{
		internal T _material;

		[OnAssetLoaded(nameof(NextPassLoaded))]
		public readonly AssetRef<RMaterial> NextPass;

		[OnChanged(nameof(RenderPriorityChange))]
		public readonly Sync<int> RenderPriority;

		private void NextPassLoaded(RMaterial _) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.NextPass = NextPass.Asset;
			});
		}

		private void RenderPriorityChange(IChangeable _) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				var newVal = Math.Min(RenderPriority.Value, short.MaxValue) - _oldRenderOrder;
				_oldRenderOrder = newVal;
				_material.RenderPriority += newVal;
			});
		}

		protected abstract void UpdateAll();

		private int _oldRenderOrder = 0;

		protected void LoadMaterial() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RenderThread.ExecuteOnEndOfFrame(() => {
				_material = new T();
				Load(_material);
				UpdateAll();
				RenderPriorityChange(null);
				NextPassLoaded(null);
			});
		}

		public override void Dispose() {
			_material?.Dispose();
			_material = null;
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			LoadMaterial();
		}
	}
}
