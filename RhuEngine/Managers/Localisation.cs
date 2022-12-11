using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	/// <summary>
	/// LocalisationManager
	/// </summary>
	/// <seealso cref="RhubarbCloudClient.Localisation" />
	/// <seealso cref="RhuEngine.Managers.IManager" />
	public sealed class LocalisationManager : Localisation, IManager
	{
		private Engine _engine;
		public string localDir;

		/// <summary>
		/// Gets the three letter language code for current localisation
		/// </summary>
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
			localDir = EngineHelpers.BaseDir + "/Locales/";
			_engine = engine;
			LoadLocal();
			_engine.SettingsUpdate += LoadLocal;
		}

		public void Step() {

		}
		public void RenderStep() {
		}
		/// <summary>
		/// Loads local files
		/// </summary>
		public override IEnumerable<string> GetFiles() {
			if (Directory.Exists(localDir)) {
				foreach (var item in Directory.GetFiles(localDir)) {
					yield return item;
				}
			}
		}
		/// <summary>
		/// Reads local files
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override string ReadFile(string item) {
			return File.ReadAllText(item);
		}
		/// <summary>
		/// Logs some data
		/// </summary>
		/// <param name="data"></param>
		public override void Log(string data) {
			RLog.Info(data);
		}
	}
}
