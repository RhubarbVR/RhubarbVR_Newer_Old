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
using System.Reflection;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	public class StartMenu : Component
	{
		[NoLoad]
		[NoSave]
		[NoSync]
		TaskBar _taskBar;

		[Exposed]
		public void ExitButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			Engine.Close();
		}

		[Exposed]
		public void VRChangeAction(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			Engine.EngineLink.ChangeVR(!RWorld.IsInVR);
		}

		[Exposed]
		public void SettingsButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			_taskBar.OpenProgram<SettingsProgram>();
		}

		[Exposed]
		public void FileExplorerButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			_taskBar.OpenProgram<FileExplorerProgram>();
		}

		[Exposed]
		public void UserButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}

		}

		public void BuildStart(TaskBar taskBar, UIRect parentrect, AssetProvider<RMaterial> mit, AssetProvider<RMaterial> iconsMit, SpriteProvder iconsSprite) {
			_taskBar = taskBar;
			var uibuilder = new UIBuilder(Entity, mit, parentrect, true);
			var buttonColor = new Colorf(0.1f, 0.8f);
			var scroller = uibuilder.AttachComponentToStack<UIScrollInteraction>();
			uibuilder.AttachChildRect<CuttingUIRect>(new Vector2f(0), new Vector2f(0.5f, 1), 0);
			var itemScroller = uibuilder.AttachChildRect<BasicScrollRect>(null, null, 0);
			var itemslist = uibuilder.AttachChildRect<VerticalList>(null, null, 0);
			scroller.OnScroll.Target = itemScroller.Scroll;
			var programs =
						 from assem in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
						 from t in assem.GetTypes().AsParallel()
						 where typeof(Program).IsAssignableFrom(t)
						 where t.GetCustomAttribute<RemoveFromProgramListAttribute>(true) == null
						 where !t.IsAbstract
						 where t.GetConstructor(Type.EmptyTypes) != null
						 select t;
			foreach (var type in programs) {
				var item = (Program)Activator.CreateInstance(type);
				uibuilder.PushRect(new Vector2f(0f), new Vector2f(1f, 0.2f), 0);
				uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
				var rawAction = uibuilder.AttachComponentToStack<DelegateCall>();
				rawAction.action = () => taskBar.OpenProgram(type);
				var buttoninteractrion = uibuilder.AddButtonEvent(rawAction.CallDelegate, null, null, false, 0.2f);
				uibuilder.PushRect(new Vector2f(0.05f), new Vector2f(0.95f));
				uibuilder.PushRect(new Vector2f(0.75f, 0f), new Vector2f(1f), 0);
				if (item.Texture is null) {
					uibuilder.AddSprit(item.Icon ?? new Vector2i(7, 5), item.Icon ?? new Vector2i(7, 5), iconsMit, iconsSprite);
				}
				else {
					var assetProvider = uibuilder.AttachComponentToStack<RawAssetProvider<RTexture2D>>();
					assetProvider.LoadAsset(item.Texture);
					uibuilder.AddImg(assetProvider);
				}
				uibuilder.PopRect();
				uibuilder.PushRect(new Vector2f(0f), new Vector2f(0.75f, 1f), 0);
				uibuilder.AddText(item.ProgramName, null, 1.9f, 1, null, !item.LocalName);
				uibuilder.PopRect();
				uibuilder.PopRect();
				uibuilder.PopRect();


				uibuilder.PopRect();
				uibuilder.PopRect();
			}
			uibuilder.PopRect();

			uibuilder.PopRect();
			uibuilder.PopRect();

			var list = uibuilder.AttachChildRect<VerticalList>(new Vector2f(0.5f, 0), new Vector2f(1), 0);
			list.Fit.Value = true;

			uibuilder.PushRect(null, null, 0);
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
			uibuilder.AddButton(false, 0.2f).ButtonEvent.Target += ExitButton;
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
			uibuilder.PushRect(new Vector2f(0), new Vector2f(0.25f, 1f), 0);
			uibuilder.AddSprit(new Vector2i(12, 0), new Vector2i(12, 0), iconsMit, iconsSprite);
			uibuilder.PopRect();
			uibuilder.PushRect(new Vector2f(0.25f, 0), new Vector2f(1f), 0);
			uibuilder.AddText("Common.Exit", null, 1.9f, 1);
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
			uibuilder.AddButton(false, 0.2f).ButtonEvent.Target += SettingsButton;
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
			uibuilder.PushRect(new Vector2f(0), new Vector2f(0.25f, 1f), 0);
			uibuilder.AddSprit(new Vector2i(0, 0), new Vector2i(0, 0), iconsMit, iconsSprite);
			uibuilder.PopRect();
			uibuilder.PushRect(new Vector2f(0.25f, 0), new Vector2f(1f), 0);
			uibuilder.AddText("Programs.Settings.Name", null, 1.9f, 1);
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
			uibuilder.AddButton(false, 0.2f).ButtonEvent.Target += FileExplorerButton;
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
			uibuilder.PushRect(new Vector2f(0), new Vector2f(0.25f, 1f), 0);
			uibuilder.AddSprit(new Vector2i(1, 0), new Vector2i(1, 0), iconsMit, iconsSprite);
			uibuilder.PopRect();
			uibuilder.PushRect(new Vector2f(0.25f, 0), new Vector2f(1f), 0);
			uibuilder.AddText("Programs.FileExplorer.Name", null, 1.9f, 1);
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();

			if (Engine.EngineLink.LiveVRChange) {
				uibuilder.PushRect(null, null, 0);
				uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
				uibuilder.AddButton(false, 0.2f).ButtonEvent.Target += VRChangeAction;
				uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
				uibuilder.PushRect(new Vector2f(0), new Vector2f(0.25f, 1f), 0);
				uibuilder.AddSprit(new Vector2i(16, 5), new Vector2i(16, 5), iconsMit, iconsSprite);
				uibuilder.PopRect();
				uibuilder.PushRect(new Vector2f(0.25f, 0), new Vector2f(1f), 0);
				var vrText = uibuilder.AddTextWithLocal("Actions.ChangeVR.Enable", 1.9f, 1);
				Action<bool> action = (mode) => vrText.Key.Value = mode ? "Actions.ChangeVR.Disable" : "Actions.ChangeVR.Enable";
				Engine.EngineLink.VRChange += action;
				action.Invoke(RWorld.IsInVR);
				uibuilder.PopRect();
				uibuilder.PopRect();
				uibuilder.PopRect();
				uibuilder.PopRect();
				uibuilder.PopRect();
			}
			uibuilder.PushRect(null, null, 0);
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PopRect();

			uibuilder.PushRect(null, null, 0);
			uibuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f), 0);
			uibuilder.AddButton(false, 0.2f).ButtonEvent.Target += UserButton;
			uibuilder.PushRect(new Vector2f(0.05f), new Vector2f(0.95f));
			uibuilder.PushRect(new Vector2f(0.75f, 0f), new Vector2f(1f), 0);
			var imgasset = uibuilder.AttachComponentToStack<StaticTexture>();
			uibuilder.AddImg(imgasset);
			uibuilder.PopRect();

			uibuilder.PushRect(new Vector2f(0f), new Vector2f(0.75f, 1f), 0);
			var text = uibuilder.AddText("Trains", null, 1.9f, 1, null, true);
			uibuilder.PopRect();
			void OnLogin(bool login) {
				if (login) {
					text.Text.Value = Engine.netApiManager.Client.User?.UserName;
					imgasset.url.Value = "https://cataas.com/cat";
				}
				else {
					text.Text.Value = "Cats";
					imgasset.url.Value = "https://cataas.com/cat";
				}
			};
			Engine.netApiManager.Client.OnLogin += (use) => OnLogin(true);
			Engine.netApiManager.Client.OnLogout += () => OnLogin(false);
			OnLogin(Engine.netApiManager.Client.IsLogin);
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();
			uibuilder.PopRect();

			uibuilder.PopRect();
		}

	}
}
