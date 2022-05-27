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
	public class UITextCurrsor : Component, IUpdatingComponent
	{
		public readonly AssetRef<RMaterial> Material;

		public readonly AssetRef<RMesh> CurrsorMesh;

		public readonly Sync<Colorf> Tint;

		[Default(0.5f)]
		public readonly Sync<float> FlashSpeed;

		[OnChanged(nameof(BindToTextComp))]
		public readonly SyncRef<UIText> TextComp;

		public readonly SyncRef<ICurrsorTextProvider> TextCurrsor;

		[NoLoad]
		[NoSave]
		[NoSync]
		private UIText _lastUIText;

		public override void OnAttach() {
			base.OnAttach();
			Tint.Value = new Colorf(0.5f, 0.5f, 0.5f, 0.5f);
			CurrsorMesh.Target = World.RootEntity.GetFirstComponentOrAttach<TrivialBox3Mesh>();
		}

		public void BindToTextComp() {
			if (_lastUIText is not null) {
				_lastUIText.OnCharRender -= LastUIText_OnCharRender;
			}
			_lastUIText = null;
			if (TextComp.Target is null) {
				return;
			}
			_lastUIText = TextComp.Target;
			_lastUIText.OnCharRender += LastUIText_OnCharRender;
		}

		public override void Step() {
			base.Step();
			_timer += RTime.Elapsedf;
			if (_timer > (FlashSpeed.Value * 2)) {
				_timer = _timer - (FlashSpeed.Value * 2);
			}
		}

		private float _timer = 0f;

		private void LastUIText_OnCharRender(Matrix arg1, DynamicTextRender.TextChar arg2, int index) {
			if (_timer < FlashSpeed.Value) {
				return;
			}
			if (Material.Asset is null) {
				return;
			}
			if (CurrsorMesh.Asset is null) {
				return;
			}
			if (TextCurrsor.Target is null) {
				return;
			}
			if (TextComp.Target is null) {
				return;
			}
			if (arg2 is null) {
				return;
			}
			if (!TextCurrsor.Target.RenderCurrsor) {
				return;
			}
			var pos = TextComp.Target.Text.Value.Length - TextCurrsor.Target.CurrsorPos + 1;
			var otherpos = pos + TextCurrsor.Target.CurrsorLength;
			if (otherpos < pos) {
				(otherpos, pos) = (pos, otherpos);
			}
			if (index > otherpos) {
				return;
			}
			if (index < pos) {
				return;
			}
			if (otherpos - pos == 0) {
				if (arg2.NullChar) {
					CurrsorMesh.Asset.Draw(index.ToString() + Pointer.id.ToString(), Material.Asset, Matrix.TS(new Vector3f(0.01f, 0.5f), new Vector3f(0.1f, 1.1f, 0.01f)) * arg1, arg2.color * Tint.Value, (Entity.UIRect?.ZDepth ?? 0) + 1151);
				}
				else {
					CurrsorMesh.Asset.Draw(index.ToString() + Pointer.id.ToString(), Material.Asset, Matrix.TS(new Vector3f(0.01f, 0.5f), new Vector3f(0.1f, 1.1f, 0.01f)) * arg1, arg2.color * Tint.Value, (Entity.UIRect?.ZDepth ?? 0) + 1151);
				}
			}
			else {
				if (otherpos == index) {
					return;
				}
				if (!arg2.NullChar) {
					CurrsorMesh.Asset.Draw(index.ToString() + Pointer.id.ToString(), Material.Asset, Matrix.TS(new Vector3f(-(arg2.textsize.x * 10 / 2), 0.55f), new Vector3f((arg2.textsize.x * 10) + 0.1f, 1.2f, 0.01f)) * arg1, arg2.color * Tint.Value, (Entity.UIRect?.ZDepth ?? 0) + 1151);
				}
			}

		}

	}
}
