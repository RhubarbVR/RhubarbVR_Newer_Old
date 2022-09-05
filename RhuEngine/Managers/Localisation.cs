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
using RhubarbCloudClient;

namespace RhuEngine.Managers
{
	public class LocalisationManager : Localisation, IManager
	{
		private Engine _engine;
		public string localDir;

		public override string Three_Letter => _engine.MainSettings.ThreeLetterLanguageName;

		public void Dispose() {
			if (NeededKeys is null) {
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

		public override IEnumerable<string> GetFiles() {
			if (Directory.Exists(localDir)) {
				foreach (var item in Directory.GetFiles(localDir)) {
					yield return item;
				}
			}
		}

		public override string ReadFile(string item) {
			return File.ReadAllText(item);
		}

		public override void Log(string data) {
			RLog.Info(data);
		}
	}
}
