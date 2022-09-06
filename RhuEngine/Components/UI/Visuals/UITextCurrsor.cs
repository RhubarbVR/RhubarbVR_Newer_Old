using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Linq;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Normal)]
	[Category(new string[] { "UI/Visuals" })]
	public class UITextCurrsor : Component
	{
		public readonly AssetRef<RMaterial> Material;

		public readonly AssetRef<RMesh> CurrsorMesh;

		public readonly Sync<Colorf> Tint;

		[Default(0.5f)]
		public readonly Sync<float> FlashSpeed;

		public readonly SyncRef<UIText> TextComp;

		public readonly SyncRef<ICurrsorTextProvider> TextCurrsor;

		protected override void OnAttach() {
			base.OnAttach();
			Tint.Value = new Colorf(0.5f, 0.5f, 0.5f, 0.5f);
			CurrsorMesh.Target = World.RootEntity.GetFirstComponentOrAttach<TrivialBox3Mesh>();
		}


	}
}
