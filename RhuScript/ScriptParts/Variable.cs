using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RNumerics;

namespace RhuScript.ScriptParts
{

	public sealed class Variable<T> : IScriptPartDataGet<T>
	{
		public IScriptPartDataGet<T> FirstData { get; set; }

		public string Name { get; set; }

		public string Flag { get; set; }

		public bool IsStatic { get; set; }

		public bool IsPublic => Flag == "public";
		public bool IsReadOnly { get; set; }

		public T Value { get; set; }

		public object Data => Value;

		public void Build() {

		}

		public void Parse(ScriptParsHelper data) {
			FirstData = data.GetExecutionData<T>();
		}

		public void UnParse(StringBuilder stringBuilder, bool bodyStatement) {
			if (bodyStatement) {
				if (Flag is not null) {
					stringBuilder.Append(Flag);
					stringBuilder.Append(' ');
				}
				if (IsReadOnly) {
					stringBuilder.Append("readonly ");
				}
				stringBuilder.Append(typeof(T).GetFormattedName());
				stringBuilder.Append(' ');
				stringBuilder.Append(Name);
				stringBuilder.Append(" = ");
				FirstData.UnParse(stringBuilder, false);
			}
			else {
				stringBuilder.Append(Name);
			}
		}

		public void VoidGetData() {
			if (IsStatic) {
				ScriptRoot._currentScriptRoot.ScriptValueUpdate(Name, Value, Flag, !IsPublic, IsReadOnly);
			}
			Value = FirstData.Value;
		}
	}
}
