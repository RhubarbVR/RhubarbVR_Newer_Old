using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace RhubarbCloudClient
{
	/// <summary>
	/// Base class for Localisation
	/// </summary>
	public abstract class Localisation
	{
		/// <summary>
		/// Fired when the localisation is reloaded
		/// </summary>
		public event Action LocalReload;
		public JToken fallBack;
		public JToken parretnfallBack;
		public JToken main;

		public HashSet<string> NeededKeys;
		/// <summary>
		/// The name of the local, the authors of the local and the language name.
		/// </summary>
		/// <returns></returns>
		public struct LocalInfo
		{
			public string name;
			public string[] authors;
			public string languageName;
		}
		/// <summary>
		/// The name of the local, the authors of the local and the language name.
		/// </summary>
		/// <returns></returns>
		public struct KeyLayoutInfo
		{
			public string name;
			public string[] authors;
			public int id;
		}

		public abstract IEnumerable<string> GetFiles();
		public abstract string ReadFile(string item);
		public abstract void Log(string data);
		/// <summary>
		/// Get the 3 letter code of the local
		/// </summary>
		/// <returns></returns>
		public abstract string Three_Letter { get; }
		/// <summary>
		/// Gets a list of the locals
		/// </summary>
		/// <returns>A List of the locals</returns>
		public IEnumerable<LocalInfo> GetLocals() {
			foreach (var item in GetFiles()) {
				LocalInfo? localInfo = null;
				try {
					var mainobject = JObject.Parse(ReadFile(item));
					localInfo = new LocalInfo { name = (string)mainobject["name"], languageName = (string)mainobject["languageName"], authors = mainobject["authors"].ToObject<List<string>>().ToArray() };
				}
				catch { }
				if (localInfo is not null) {
					yield return localInfo ?? new LocalInfo();
				}
			}

		}
		/// <summary>
		/// Gets all keyboard layouts
		/// </summary>
		/// <returns></returns>
		public IEnumerable<KeyLayoutInfo> GetKeyboardLayouts() {
			foreach (var item in GetFiles()) {
				KeyLayoutInfo? localInfo = null;
				try {
					var mainobject = JObject.Parse(ReadFile(item));
					localInfo = new KeyLayoutInfo { name = (string)mainobject["name"], id = (int)mainobject["id"], authors = mainobject["authors"].ToObject<List<string>>().ToArray() };
				}
				catch { }
				if (localInfo is not null) {
					yield return localInfo ?? new KeyLayoutInfo();
				}
			}

		}
		/// <summary>
		/// Gets a keyboard layout from the ID
		/// </summary>
		/// <param name="id">The keyboard layout to use</param>
		/// <returns>A JObject with the keyboard layout. If no layout is found qwerty english is returned.</returns>
		public JObject GetKeyboardLayout(int id) {
			JObject english = null;
			foreach (var item in GetFiles()) {
				KeyLayoutInfo? localInfo = null;
				JObject mainobject = null;
				try {
					mainobject = JObject.Parse(ReadFile(item));
					localInfo = new KeyLayoutInfo { name = (string)mainobject["name"], id = (int)mainobject["id"], authors = mainobject["authors"].ToObject<List<string>>().ToArray() };
				}
				catch { }
				if (localInfo is not null) {
					if (localInfo.Value.id == 1033) {
						english = mainobject;
					}
					if (localInfo.Value.id == id) {
						return mainobject;
					}
				}
			}
			Log("Failed to find keyboard layout going to qwerty English");
			return english;
		}

		///<summary>
		/// Get Local String
		///</summary>
		///<param name="key">Localisastion key</param>
		///<param name="prams">Objects to pass to the string formatter</param>
		public string GetLocalString(string key, params object[] prams) {
			return GetLocalString(key + ";" + string.Join(";", prams.Select((x) => x?.ToString() ?? "NULL")));
		}

		/// <summary>
		/// Gets a localised string
		/// </summary>
		/// <param name="dataString">The string to localise, in the form of key;param1;param2;...</param>
		/// <returns>The localised string.</returns>
		public string GetLocalString(string dataString) {
			var prams = dataString?.Split(';') ?? Array.Empty<string>();
			if (prams.Length == 0) {
				return "";
			}
			var key = prams[0];
			for (var i = 1; i < prams.Length; i++) {
				prams[i - 1] = prams[i];
			}
			Array.Resize(ref prams, prams.Length - 1);
			if (string.IsNullOrEmpty(key)) {
				return "";
			}
			JToken returnobj;
			if (main is not null) {
				returnobj = main[key];
				if (returnobj is not null) {
					return string.Format((string)returnobj, prams);
				}
			}
			if (parretnfallBack is not null) {
				returnobj = parretnfallBack[key];
				if (returnobj is not null) {
					return string.Format((string)returnobj, prams);
				}
			}
			NeededKeys?.Add(key);
			if (fallBack is null) {
				return dataString;
			}
			returnobj = fallBack[key];
			return returnobj is not null ? string.Format((string)returnobj, prams) : dataString;
		}
		/// <summary>
		/// Loads the local
		/// </summary>
		public void LoadLocal() {
			var targetcode = CultureInfo.InstalledUICulture;
			if (Three_Letter is not null) {
				targetcode = new CultureInfo(Three_Letter, false);
			}

			Log($"Local is {targetcode.Name} parent is {targetcode.Parent.Name}");
			foreach (var item in GetFiles()) {
				try {
					var mainobject = JObject.Parse(ReadFile(item));
					if (((string)mainobject["languageName"]) == "en") {
						fallBack = mainobject["sheet"];
						Log("Loaded English Fall back");
					}
					if (!string.IsNullOrEmpty(targetcode.Parent.Name)) {
						if (((string)mainobject["languageName"]) == targetcode.Parent.Name) {
							parretnfallBack = mainobject["sheet"];
							Log("Loaded Fall back");
						}
					}
					if (((string)mainobject["languageName"]) == targetcode.Name) {
						main = mainobject["sheet"];
						Log("Loaded Main");
					}
				}
				catch { }
			}
			LocalReload?.Invoke();
		}

	}
}
