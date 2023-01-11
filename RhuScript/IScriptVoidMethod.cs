using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhuScript
{
	public interface IScriptVoidMethod : IScriptPart
	{
		public void Invoke();
	}
}
