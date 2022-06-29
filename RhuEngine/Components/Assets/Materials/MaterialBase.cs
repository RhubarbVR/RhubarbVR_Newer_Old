﻿using System;

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

		public virtual T GetMaterialFromLinker() {
			return StaticMaterialManager.GetMaterial<T>();
		}

		public abstract void UpdateAll();

		public void LoadMaterial() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_material = GetMaterialFromLinker();
			Load(_material.Material);
			UpdateAll();
		}

		public override void Dispose() {
			base.Dispose();
			_material?.Dispose();
			_material = default;
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMaterial();
		}
	}
}
