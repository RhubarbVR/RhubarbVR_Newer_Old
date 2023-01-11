using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuScript
{
	public interface IScriptPart
	{
		public void Build();

		public void UnParse(StringBuilder stringBuilder, bool bodyStatement);

		public void Parse(ScriptParsHelper data);
	}
}
