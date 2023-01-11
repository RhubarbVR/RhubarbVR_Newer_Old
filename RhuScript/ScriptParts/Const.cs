using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RhuScript.ScriptParts
{

	public sealed class Const<T> : IScriptPartDataGet<T>
	{
		public T Value { get; set; }

		public object Data => Value;

		public void Build() {

		}

		public void Parse(ScriptParsHelper data) {
			var currentPart = data.CurrentSection.Peek();
			if (currentPart == "null") {
				Value = default;
				return;
			}
			if (typeof(T) == typeof(string)) {
				if (currentPart.StartsWith("st_comp_")) {
					if (Guid.TryParse(currentPart.Substring("st_comp_".Length), out var guid)) {
						Value = (T)(object)data.StringData[guid];
					}
				}
			}
			try {
				var converter = TypeDescriptor.GetConverter(typeof(T));
				Value = (T)converter.ConvertFrom(currentPart);
			}
			catch {
				Value = default;
			}
		}

		public void UnParse(StringBuilder stringBuilder, bool bodyStatement) {
			stringBuilder.Append(Value?.ToString() ?? "null");
		}


		public void VoidGetData() {

		}
	}
}
