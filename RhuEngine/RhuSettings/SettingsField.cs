using System;
using System.Collections.Generic;
using System.Text;

namespace RhuSettings
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SettingsField : Attribute
	{
		public string Path = "/";

		public string help;

		public string[] oldFields;

		public SettingsField(string[] _oldFields, string _help = "", string path = "/") : this(_help, path) {
			oldFields = _oldFields;
		}
		public SettingsField(string _help = "", string path = "/") {
			help = _help;
			Path = path;
		}
	}
}
