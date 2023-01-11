using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RNumerics;

namespace RhuScript.ScriptParts
{

	public sealed class ScriptRoot : IScriptVoidMethod
	{

		public readonly Dictionary<string, (object data, string flag, bool privateData, bool readOnly)> StaticValues = new();

		public readonly List<IScriptPartDataGet> Parts = new();

		public void ScriptValueUpdate(string key, object data, string flag = null, bool privateData = false, bool readOnly = false) {
			if (StaticValues.ContainsKey(key)) {
				StaticValues[key] = (data, flag, privateData, readOnly);
			}
			else {
				StaticValues.Add(key, (data, flag, privateData, readOnly));
			}
		}

		public void SetStaticValue(string target, object data, Func<string, bool> flagChck = null) {
			if (StaticValues.TryGetValue(target, out var tempData)) {
				if (tempData.privateData) {
					return;
				}
				if (tempData.readOnly) {
					return;
				}
				if (flagChck is not null) {
					if (!flagChck.Invoke(tempData.flag)) {
						return;
					}
				}
				tempData.data = data;
				StaticValues[target] = tempData;
			}
		}

		public object GetStaticValue(string target, Func<string, bool> flagChck = null) {
			if (StaticValues.TryGetValue(target, out var tempData)) {
				if (tempData.privateData) {
					return null;
				}
				if (tempData.readOnly) {
					return null;
				}
				if (flagChck is not null) {
					if (!flagChck.Invoke(tempData.flag)) {
						return null;
					}
				}
				return tempData.data;
			}
			return null;
		}

		public void Build() {
			for (var i = 0; i < Parts.Count; i++) {
				Parts[i].Build();
			}
		}

		[ThreadStatic]
		static internal ScriptRoot _currentScriptRoot = null;

		public void Invoke() {
			var lastScriptRoot = _currentScriptRoot;
			_currentScriptRoot = this;
			ExecutionSafty.Start();
			try {
				for (var i = 0; i < Parts.Count; i++) {
					Parts[i].VoidGetData();
				}
			}
			finally {
				_currentScriptRoot = lastScriptRoot;
				ExecutionSafty.End();
			}
		}

		public void UnParse(StringBuilder stringBuilder, bool bodyStatement = false) {
			for (var i = 0; i < Parts.Count; i++) {
				Parts[i].UnParse(stringBuilder, true);
				stringBuilder.Append(";\n");
			}
		}

		public void Parse(ScriptParsHelper data) {
			var currentData = data.CurrentSection.Peek().Split(';');
			for (var i = 0; i < currentData.Length; i++) {
				var codeBlock = currentData[i];
				if (codeBlock.Contains('=')) {
					var enter = codeBlock.IndexOf('=');
					var typeStart = codeBlock.LastIndexOf(" ", enter - 2);
					var depth = 0;
					var typeEnd = 0;
					for (var x = 0; x < typeStart; x++) {
						var currentIndex = typeStart - x - 1;
						var currentChar = codeBlock[currentIndex];
						if (currentChar == '>') {
							depth++;
						}
						if (currentChar == '<') {
							depth--;
						}
						if (depth == 0 && currentChar == ' ') {
							typeEnd = currentIndex;
							break;
						}
					}
					var type = FamcyTypeParser.PraseType(codeBlock.Substring(typeEnd, typeStart - typeEnd));
					var newVaraibleBlock = (IScriptPartDataGet)Activator.CreateInstance(typeof(Variable<>).MakeGenericType(type));
					Parts.Add(newVaraibleBlock);
					data.CurrentSection.Push(codeBlock);
					newVaraibleBlock.Parse(data);
					data.CurrentSection.Pop();
				}
				else if (string.IsNullOrEmpty(codeBlock)) {
				}
				else {
					throw new Exception($"No Idea what {codeBlock} is");
				}
			}
		}

		public ScriptRoot(List<IScriptPartDataGet> scriptPartDataGets) {
			Parts = scriptPartDataGets;
		}
		public ScriptRoot() {
		}
		public ScriptRoot(string code) {
			Parse(new ScriptParsHelper(code));
		}
	}
}
