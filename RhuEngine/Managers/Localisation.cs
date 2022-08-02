using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RhuEngine.AssetSystem;
using RhuEngine.AssetSystem.AssetProtocals;
using System.IO;
using System.Threading.Tasks;
using RhuEngine.Settings;
using RhuEngine.Linker;
using RNumerics;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RhuEngine.Managers
{
	public class LocalisationManager : IManager
	{
		public event Action LocalReload;
		private Engine _engine;
		public string localDir;
		public JToken fallBack;
		public JToken parretnfallBack;
		public JToken main;

		public HashSet<string> NeededKeys;

		public struct LocalInfo
		{
			public string name;
			public string[] authors;
			public string languageName;
		}
		public struct KeyLayoutInfo
		{
			public string name;
			public string[] authors;
			public int id;
		}

		public IEnumerable<LocalInfo> GetLocals() {
			if (Directory.Exists(localDir)) {
				foreach (var item in Directory.GetFiles(localDir)) {
					LocalInfo? localInfo = null;
					try {
						var mainobject = JObject.Parse(File.ReadAllText(item));
						localInfo = new LocalInfo { name = (string)mainobject["name"], languageName = (string)mainobject["languageName"], authors = ((mainobject["authors"]).ToObject<List<string>>()).ToArray() };
					}
					catch { }
					if (localInfo is not null) {
						yield return localInfo ?? new LocalInfo();
					}
				}
			}
		}

		public IEnumerable<KeyLayoutInfo> GetKeyboardLayouts() {
			if (Directory.Exists(localDir)) {
				foreach (var item in Directory.GetFiles(localDir)) {
					KeyLayoutInfo? localInfo = null;
					try {
						var mainobject = JObject.Parse(File.ReadAllText(item));
						localInfo = new KeyLayoutInfo { name = (string)mainobject["name"], id = (int)mainobject["id"], authors = ((mainobject["authors"]).ToObject<List<string>>()).ToArray() };
					}
					catch { }
					if (localInfo is not null) {
						yield return localInfo ?? new KeyLayoutInfo();
					}
				}
			}
		}

		public JObject GetKeyboardLayout(int id) {
			if (Directory.Exists(localDir)) {
				JObject english = null;
				foreach (var item in Directory.GetFiles(localDir)) {
					KeyLayoutInfo? localInfo = null;
					JObject mainobject = null;
					try {
						mainobject = JObject.Parse(File.ReadAllText(item));
						localInfo = new KeyLayoutInfo { name = (string)mainobject["name"], id = (int)mainobject["id"], authors = ((mainobject["authors"]).ToObject<List<string>>()).ToArray() };
					}
					catch { }
					if (localInfo is not null) {
						if (localInfo.Value.id == 1033) {
							english = mainobject;
						}
						if(localInfo.Value.id == id) {
							return mainobject;
						}
					}
				}
				RLog.Info("Failed to find keyboard layout going to qwerty English");
				return english;
			}
			return null;
		}

		public string GetLocalString(string key) {
			if (string.IsNullOrEmpty(key)) {
				return "";
			}
			JToken returnobj;
			if (main is not null) {
				returnobj = main[key];
				if (returnobj is not null) {
					return (string)returnobj;
				}
			}
			if (parretnfallBack is not null) {
				returnobj = parretnfallBack[key];
				if (returnobj is not null) {
					return (string)returnobj;
				}
			}
			NeededKeys?.Add(key);
			if (fallBack is null) {
				return key;
			}
			returnobj = fallBack[key];
			if (returnobj is not null) {
				return (string)returnobj;
			}
			return key;
		}

		public void LoadLocal() {
			var targetcode = CultureInfo.InstalledUICulture;
			if (_engine.MainSettings.ThreeLetterLanguageName is not null) {
				targetcode = new CultureInfo(_engine.MainSettings.ThreeLetterLanguageName, false);
			}
			RLog.Info($"Local is {targetcode.Name} parrent is {targetcode.Parent.Name}");
			if (Directory.Exists(localDir)) {
				foreach (var item in Directory.GetFiles(localDir)) {
					try {
						var mainobject = JObject.Parse(File.ReadAllText(item));
						if (((string)mainobject["languageName"]) == "en") {
							fallBack = mainobject["sheet"];
						}
						if(!string.IsNullOrEmpty(targetcode.Parent.Name)) {
							if (((string)mainobject["languageName"]) == targetcode.Parent.Name) {
								parretnfallBack = mainobject["sheet"];
							}
						}
						if (((string)mainobject["languageName"]) == targetcode.Name) {
							main = mainobject["sheet"];
						}
					}
					catch { }
				}
			}
			LocalReload?.Invoke();
		}

		public void Dispose() {
			if(NeededKeys is null) {
				return;
			}
			if (!Directory.Exists(localDir)) {
				Directory.CreateDirectory(localDir);
			}
			File.Delete(localDir + "NeededKeys.text");
			var returnString = new List<string>();
			foreach (var item in NeededKeys) {
				returnString.Add($"		\"{item}\":\"{item}\",");
			}
			File.WriteAllLines(localDir + "NeededKeys.text", returnString);
		}

		public void Init(Engine engine) {
			localDir = Engine.BaseDir + "/Locales/";
			_engine = engine;
			if (_engine._buildMissingLocal) {
				NeededKeys = new HashSet<string>();
			}
			LoadLocal();
			_engine.SettingsUpdate += LoadLocal;
		}

		public void Step() {

		}
		public void RenderStep() {
		}
	}
}
