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
using System.Globalization;
using RhuEngine.Components.PrivateSpace;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public class StartMenu : Component
	{
		[NoLoad]
		[NoSave]
		[NoSync]
		TaskBar _taskBar;

		[Exsposed]
		public void ExitButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			Engine.Close();
		}

		[Exsposed]
		public void SettingsButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			_taskBar.OpenProgram<SettingsProgram>();
		}

		[Exsposed]
		public void FileExplorerButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			_taskBar.OpenProgram<FileExplorerProgram>();
		}

		[Exsposed]
		public void UserButton(ButtonEvent buttonEvent) {

		}

		public void BuildStart(TaskBar taskBar,UIRect parentrect, AssetProvider<RMaterial> mit, AssetProvider<RMaterial> iconsMit, SpriteProvder iconsSprite) {
			_taskBar = taskBar;
			var uibuilder = new UIBuilder(Entity, mit, parentrect, true);
			var buttonColor = new Colorf(0.1f, 0.8f);
			uibuilder.AttachChildRect<CuttingUIRect>(new Vector2f(0), new Vector2f(0.5f, 1), 0);
			{
				uibuilder.AttachChildRect<VerticalList>(null, null, 0);
				{
					uibuilder.PushRect(new Vector2f(0f), new Vector2f(1f, 0.2f), 0);
					{
						uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
						{
							uibuilder.AddButton(false, buttonColor);
							{
								uibuilder.PushRect(new Vector2f(0.05f), new Vector2f(0.95f));
								{
									uibuilder.PushRect(new Vector2f(0.25f, 0f), new Vector2f(1f), 0);
									{
										uibuilder.AddSprit(new Vector2i(0), new Vector2i(0), iconsMit, iconsSprite);
									}
									uibuilder.PopRect();
									uibuilder.PushRect(new Vector2f(0f), new Vector2f(0.25f, 1f), 0);
									{
										uibuilder.AddText("Trains", null, null, null, true);
									}
									uibuilder.PopRect();
								}
								uibuilder.PopRect();
							}
							uibuilder.PopRect();
						}
						uibuilder.PopRect();
					}
					uibuilder.PopRect();
				}
			uibuilder.PopRect();
			}
			uibuilder.PopRect();
			var list = uibuilder.AttachChildRect<VerticalList>(new Vector2f(0.5f, 0), new Vector2f(1), 0);
			list.Fit.Value = true;

			uibuilder.PushRect(null, null, 0);
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
			uibuilder.AddButton(false, buttonColor).ButtonEvent.Target += ExitButton;
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
			uibuilder.PushRect(new Vector2f(0), new Vector2f(0.25f, 1f), 0);
			uibuilder.AddSprit(new Vector2i(12, 0), new Vector2i(12, 0), iconsMit, iconsSprite);
			uibuilder.PopRect();
			uibuilder.PushRect(new Vector2f(0.25f, 0), new Vector2f(1f), 0);
			uibuilder.AddText("Common.Exit");
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
			uibuilder.AddButton(false, buttonColor).ButtonEvent.Target += SettingsButton;
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
			uibuilder.PushRect(new Vector2f(0), new Vector2f(0.25f, 1f), 0);
			uibuilder.AddSprit(new Vector2i(0, 0), new Vector2i(0, 0), iconsMit, iconsSprite);
			uibuilder.PopRect();
			uibuilder.PushRect(new Vector2f(0.25f, 0), new Vector2f(1f), 0);
			uibuilder.AddText("Programs.Settings.Name");
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
			uibuilder.AddButton(false, buttonColor).ButtonEvent.Target += FileExplorerButton;
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
				uibuilder.PushRect(new Vector2f(0), new Vector2f(0.25f,1f), 0);
					uibuilder.AddSprit(new Vector2i(1,0), new Vector2i(1, 0), iconsMit, iconsSprite);
				uibuilder.PopRect();
					uibuilder.PushRect(new Vector2f(0.25f,0), new Vector2f(1f),0);
					uibuilder.AddText("Programs.FileExplorer.Name");
					uibuilder.PopRect();
				uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
			uibuilder.AddButton(false, buttonColor).ButtonEvent.Target += UserButton;
			uibuilder.PushRect(new Vector2f(0.05f), new Vector2f(0.95f));
			uibuilder.PushRect(new Vector2f(0.75f, 0f), new Vector2f(1f), 0);
			var imgasset = uibuilder.AttachComponentToStack<StaticTexture>();
			uibuilder.AddImg(imgasset);
			uibuilder.PopRect();

			uibuilder.PushRect(new Vector2f(0f), new Vector2f(0.75f, 1f), 0);
			var text = uibuilder.AddText("Trains", null, null, null, true);
			uibuilder.PopRect();
			void OnLogin(bool login) {
				if (login) {
					text.Text.Value = Engine.netApiManager.User.UserName;
					imgasset.url.Value = "https://cataas.com/cat";
				}
				else {
					text.Text.Value = "Cats";
					imgasset.url.Value = "https://cataas.com/cat";
				}
			};
			Engine.netApiManager.OnLoginAndLoggout += OnLogin;
			OnLogin(Engine.netApiManager.IsLoggedIn);
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();

			uibuilder.PopRect();
		}

		public override void Step() {
		}

	}
}
