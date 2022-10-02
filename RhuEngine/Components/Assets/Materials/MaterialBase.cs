using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	public abstract class MaterialBase<T> : AssetProvider<RMaterial> where T: IStaticMaterial
	{
		public T _material;

		[OnChanged(nameof(RenderOrderOffsetUpdate))]
		public readonly Sync<int> RenderOrderOffset;

		protected void RenderOrderOffsetUpdate() {
			if (_material is null) {
				return;
			}
			RenderThread.ExecuteOnEndOfFrame(() => _material.Material.RenderOrderOffset = RenderOrderOffset); 
		}

		public virtual T GetMaterialFromLinker() {
			return StaticMaterialManager.GetMaterial<T>();
		}

		protected abstract void UpdateAll();

		protected void LoadMaterial() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RenderThread.ExecuteOnEndOfFrame(() => {
				_material = GetMaterialFromLinker();
				Load(_material.Material);
				UpdateAll();
				_material.Material.RenderOrderOffset = RenderOrderOffset;
			});
		}

		public override void Dispose() {
			base.Dispose();
			_material?.Dispose();
			_material = default;
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			LoadMaterial();
		}
	}
}
