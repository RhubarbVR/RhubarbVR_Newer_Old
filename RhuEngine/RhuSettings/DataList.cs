using System;
using System.Collections.Generic;
using System.Text;

namespace RhuSettings
{
	public class DataList : DataObject
	{
		private readonly Dictionary<string, DataObject> _list = new();

		public Dictionary<string, DataObject>.Enumerator GetEnumerator() {
			return _list.GetEnumerator();
		}

		public void AddDataObject(string loc, DataObject val) {
			_list.Add(loc, val);
		}

		public DataObject GetDataObject(string loc) {
			return _list.TryGetValue(loc, out var value) ? value : null;
		}

		public DataList AddNewList(string loc) {
			var data = new DataList();
			_list.Add(loc, data);
			return data;
		}

		public DataList AddList(string loc) {
			return _list.TryGetValue(loc, out var value) ? (DataList)value : AddNewList(loc);
		}
	}
}
