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
using RhuEngine.Components.PrivateSpace;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	public class KeyBoard : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UICanvas uICanvas;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UnlitMaterial mit;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Grabbable grabable;

		[Flags]
		public enum KeyBoardLayOutSelection
		{
			Base = 0,
			Shift = 1,
			AltGr = 2,
			FN = 4,
		}

		[OnChanged(nameof(LoadKeyboard))]
		public readonly Sync<KeyBoardLayOutSelection> CurrentLayout;
		public override void OnAttach() {
			uICanvas = Entity.AddChild("Canvas").AttachComponent<UICanvas>();
			uICanvas.scale.Value = new Vector3f(10f, 3.3f, 1);
			mit = Entity.AttachComponent<UnlitMaterial>();
			mit.DullSided.Value = true;
			mit.Transparency.Value = Transparency.Blend;
			uICanvas.Entity.position.Value = new Vector3f(0, 1, -1);
			grabable = uICanvas.Entity.AttachComponent<Grabbable>();
			Engine.SettingsUpdate += LoadKeyboard;
			Entity.EnabledChanged += Entity_EnabledChanged;
		}

		bool _keyboardLoaded = false;	

		private void Entity_EnabledChanged() {
			if(!Entity.IsEnabled) {
				return;
			}
			if (_keyboardLoaded) {
				return;
			}
			//Todo: Keyboard Update
			//Task.Run(LoadKeyboard);
		}

		public void LoadKeyboard() {
			_keyboardLoaded = true;
			var targetcode = CultureInfo.InstalledUICulture;
			if (Engine.MainSettings.ThreeLetterLanguageName is not null) {
				targetcode = new CultureInfo(Engine.MainSettings.ThreeLetterLanguageName, false);
			}
			var id = Engine.MainSettings.KeyboardLayoutID;
			if (Engine.MainSettings.KeyboardLayoutID == -1) {
				id = targetcode.KeyboardLayoutId;
			}
			RLog.Info($"Started loading keyboard with id {id} Layer {CurrentLayout.Value}");
			var layout = Engine.localisationManager.GetKeyboardLayout(id);
			uICanvas.Entity.DestroyChildren();
			if (layout is null) {
				RLog.Info("No Layout data for keyboard");
				return;
			}
			RLog.Info("Starting Building keyboard");
			try {
				var uiBuilder = new UIBuilder(uICanvas.Entity, mit);
				var grabber = uiBuilder.CurretRectEntity.AttachComponent<UIGrabInteraction>();
				grabber.Grabeded.Target = grabable.RemoteGrab;
				uiBuilder.PushRect();
				uiBuilder.AddRectangle(0, 0.9f, true);
				var rowAmount = (float)layout["rowAmount"];
				var colAmount = (float)layout["colAmount"];
				foreach (var key in layout["keys"]) {
					var row = (float)key["row"] / rowAmount;
					var col = (float)key["col"] / colAmount;
					var w = (float)key["w"] / colAmount;
					var h = (float)key["h"] / rowAmount;
					uiBuilder.PushRect(new Vector2f(col, row), new Vector2f(col + w, row + h));
					var aspic = (Math.Min(w, h) / Math.Max(w, h));
					uiBuilder.PushRect(null, null, 0);
					uiBuilder.SetOffsetMinMax(new Vector2f(0.05f), new Vector2f(-0.05f));
					var buttonInterAction = uiBuilder.AddButton(false, 0.3f, 0.9f);
					JToken layer;
					try {
						layer = key["layers"]["ALL"];
					}
					catch {
						layer = null;
					}
					layer ??= key["layers"][((int)CurrentLayout.Value).ToString()];
					string code = null;
					try {
						code = (string)layer["code"];
					}
					catch {
					}
					var ckey = -1;
					try {
						ckey = (int)Enum.Parse(typeof(Key), (string)layer["key"], true);
					}
					catch {
						ckey = (int)layer["key"];
					}
					var lable = (string)layer["lable"];
					var toggle = false;
					try {
						toggle = (bool)layer["toggle"];
					}
					catch { }
					var changeLayer = -1;
					try {
						changeLayer = (int)layer["changeLayer"];
					}
					catch { }
					var buttonEvent = uiBuilder.AttachComponentToStack<ButtonEventManager>();
					var pressCall = uiBuilder.AttachComponentToStack<DelegateCall>();
					buttonEvent.Click.Target = pressCall.CallDelegate;
					buttonInterAction.ButtonEvent.Target = buttonEvent.Call;
					pressCall.action = () => ProcesButtonPress(changeLayer, toggle, ckey, code);
					var releasesCall = uiBuilder.AttachComponentToStack<DelegateCall>();
					buttonEvent.Releases.Target = releasesCall.CallDelegate;
					releasesCall.action = () => ProcesButtonReleases(changeLayer, toggle, ckey, code);
					uiBuilder.PushRect(null, null, 0);
					uiBuilder.SetOffsetMinMax(new Vector2f(0.01f), new Vector2f(-0.01f));
					var text = uiBuilder.AddText(lable, null, 1.7f);
					uiBuilder.PopRect();
					uiBuilder.PopRect();
					uiBuilder.PopRect();
					uiBuilder.PopRect();
				}
				uiBuilder.PopRect();
			}
			catch (Exception error) {
				RLog.Err($"Failed to build keyboard {error}");
			}
		}
		public readonly Dictionary<string,DateTime> PressingKeys = new();

		public readonly HashSet<int> ToggleKeys = new();
		private void ProcesButtonPress(int changeLayer, bool toggle, int ckey, string code) {
			if (toggle) {
				if (ToggleKeys.Contains(ckey)) {
					ToggleKeys.Remove(ckey);
				}
				else {
					ToggleKeys.Add(ckey);
				}
			}
			if (code is not null) {
				RInput.InjectedTypeDelta += code;
				PressingKeys.Add(code, DateTime.UtcNow);
				void CheckIfStillPressing() {
					if (PressingKeys.ContainsKey(code)) {
						RWorld.ExecuteOnEndOfFrame(CheckIfStillPressing);
						if ((DateTime.UtcNow - PressingKeys[code]).TotalSeconds >= 1) {
							RInput.InjectedTypeDelta += code;
							PressingKeys.Remove(code);
							PressingKeys.Add(code, DateTime.UtcNow);
						}
					}
				}
				RWorld.ExecuteOnEndOfFrame(() => {
					RInput.InjectedTypeDelta = RInput.InjectedTypeDelta.Replace(code, string.Empty);
					CheckIfStillPressing();
				});
			}
		}
		private void ProcesButtonReleases(int changeLayer, bool toggle, int ckey, string code) {
			if (code is not null) {
				PressingKeys.Remove(code);
				RInput.InjectedTypeDelta = RInput.InjectedTypeDelta.Replace(code, string.Empty);
			}
		}
	}
}
