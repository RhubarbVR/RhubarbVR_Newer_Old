using System;
using System.Collections.Generic;
using System.Text;

namespace RhuSettings
{
	public class DataNode : DataObject
	{
		private object _val = null;

		public object Getval() {
			return _val;
		}
		public void Setval(object newval) {
			_val = newval;
		}
	}
}
