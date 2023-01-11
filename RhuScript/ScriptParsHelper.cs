
using System.Text.RegularExpressions;

using RhuScript.ScriptParts;

namespace RhuScript
{

	public sealed class ScriptParsHelper
	{
		public string Script;
		public string CleanedScript;

		public readonly Dictionary<Guid, string> StringData = new();

		public readonly Stack<string> CurrentSection = new();

		public ScriptParsHelper(string script) {
			Script = script;
			CleanedScript = "";
			var currentScriptData = "";
			var inAddBlock = false;
			var lastWasSkiper = false;
			for (var i = 0; i < Script.Length; i++) {
				var currentChar = Script[i];
				if (inAddBlock) {
					if (lastWasSkiper) {
						if (currentChar == 'n') {
							currentScriptData += "\n";
						}
						else {
							currentScriptData += currentChar;
						}
						continue;
					}
					if (currentChar == '"') {
						inAddBlock = false;
						var data = Guid.NewGuid();
						StringData.Add(data, currentScriptData);
						CleanedScript += "st_comp_" + data.ToString();
						currentScriptData = "";
					}
					if (currentChar == '\\') {
						lastWasSkiper = true;
					}
				}
				else {
					if (currentChar == '"') {
						inAddBlock = true;
					}
					else {
						if (currentChar is not '\n' and not '\r') {
							CleanedScript += currentChar;
						}
					}
				}
			}
			CurrentSection.Push(CleanedScript);
		}

		public IScriptPartDataGet<T> GetExecutionData<T>() {

		}
	}

}