using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RhuEngine.WorldObjects;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public class TaskBar : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UICanvas uICanvas;
		public override void OnAttach() {
			base.OnAttach();
			Entity.position.Value = new Vector3f(-0.7f, 0.1f, 0);
			Entity.rotation.Value = Quaternionf.CreateFromEuler(-22.5f, 0, 0);
			uICanvas = Entity.AttachComponent<UICanvas>();
			uICanvas.scale.Value = new Vector3f(16, 2, 1);
			uICanvas.FrontBind.Value = true;
			uICanvas.TopOffset.Value = true;
			var shader = World.RootEntity.GetFirstComponentOrAttach<UnlitClipShader>();
			var mit = Entity.AttachComponent<DynamicMaterial>();
			mit.shader.Target = shader;
			mit.transparency.Value = Transparency.Blend;
			var rectTwo = Entity.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = Vector2f.Zero;
			rectTwo.AnchorMax.Value = Vector2f.One;
			var img = Entity.AttachComponent<UIRectangle>();
			img.Tint.Value = new Colorf(0,0,0,0.9f);
			img.Material.Target = mit;
			var listentit = Entity.AddChild("list");
			var list = listentit.AttachComponent<HorizontalList>();
			list.Fit.Value = true;
			void AddButton() {
				var child = listentit.AddChild("childEliment");
				var rectTwo = child.AttachComponent<UIRect>();
				rectTwo.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
				rectTwo.AnchorMax.Value = new Vector2f(0.9f, 0.9f);
				var img = child.AttachComponent<UIRectangle>();
				img.Tint.Value = new Colorf(0.2f, 0.2f, 0.2f, 0.9f);
				img.Material.Target = mit;
				child.AttachComponent<UIButtonInteraction>();
			}
			AddButton();
			AddButton();
			AddButton();
			AddButton();
			AddButton();
			AddButton();
			AddButton();

		}


		public override void Step() {
			
		}

	}
}
