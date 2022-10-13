﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Flags]
	public enum RLightMask : int
	{
		None = 0,
		Layer_0 = 1,
		Layer_1 = 2,
		Layer_2 = 4,
		Layer_3 = 8,
		Layer_4 = 16,
		Layer_5 = 32,
		Layer_6 = 64,
		Layer_7 = 128,
		Layer_8 = 256,
		Layer_9 = 512,
		Layer_10 = 1024,
		Layer_11 = 2048,
		Layer_12 = 4096,
		Layer_13 = 8192,
		Layer_14 = 16384,
		Layer_15 = 268435456,
	}
	public enum RElementTextureFilter
	{
		Inhernt,
		Nearest,
		Linear,
		LinearMipmap,
		NearestMipmap,
		NearestMipmapAnisotropic,
		LinearMipmapAnisotropic,
	}

	public enum RElementTextureRepeat
	{
		Inhernt,
		Disable,
		Enabled,
		Mirror
	}

	[SingleComponentLock]
	public abstract class CanvasItem : LinkedWorldComponent
	{
		protected override bool AddToUpdateList => false;

		public readonly Sync<Colorf> Modulate;
		public readonly Sync<Colorf> ModulateSelf;

		public readonly Sync<bool> ShowBehindParent;
		public readonly Sync<bool> TopLevel;
		public readonly Sync<bool> ClipChildren;

		[Default(RLightMask.Layer_0)]
		public readonly Sync<RLightMask> LightMask;
		[Default(RElementTextureFilter.Inhernt)]
		public readonly Sync<RElementTextureFilter> Filter;
		[Default(RElementTextureRepeat.Inhernt)]
		public readonly Sync<RElementTextureRepeat> Repeat;

		public readonly Sync<bool> UseParentMaterial;
		public readonly AssetRef<RMaterial> Material;

		protected override void OnAttach() {
			base.OnAttach();
			Modulate.Value = Colorf.White;
			ModulateSelf.Value = Colorf.White;
		}
	}
}
