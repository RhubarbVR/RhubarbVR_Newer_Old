using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MessagePack;
using RhuEngine.WorldObjects;
using System.Linq;

namespace RhuEngine.Components.ScriptNodes
{
	public class ScriptNodeDataHolder
	{
		public object[] localValues;
		public ScriptNodeDataHolder(uint AmountOfLocalValues) {
			localValues = new object[AmountOfLocalValues];
		}
	}

}
