using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	public abstract partial class MaterialBase<T> : AssetProvider<RMaterial> where T : IStaticMaterial
	{
		public T _material;

		public readonly Sync<int> RenderPriority;

		public virtual T GetMaterialFromLinker() {
			return StaticMaterialManager.GetMaterial<T>();
		}

		protected abstract void UpdateAll();

		int _oldRenderOrder = 0; 

		protected void LoadMaterial() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			RenderThread.ExecuteOnEndOfFrame(() => {
				_material = GetMaterialFromLinker();
				Load(_material.Material);
				UpdateAll();
				var newVal = Math.Min(RenderPriority.Value, short.MaxValue) - _oldRenderOrder;
				_oldRenderOrder = newVal;
				_material.Material.RenderPriority += newVal;
			});
		}

		public override void Dispose() {
			_material?.Dispose();
			_material = default;
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			LoadMaterial();
		}
	}
}
