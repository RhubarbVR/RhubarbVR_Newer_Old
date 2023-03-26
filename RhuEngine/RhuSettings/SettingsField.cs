using System;
using System.Collections.Generic;
using System.Text;

namespace RhuSettings
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class NeedsRebootAttribute : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class SettingsFieldAttribute : Attribute
	{
		public string Path = "/";

		public string help;

		public string[] oldFields;

		public SettingsFieldAttribute(string[] _oldFields, string _help = "", string path = "/") : this(_help, path) {
			oldFields = _oldFields;
		}
		public SettingsFieldAttribute(string _help = "", string path = "/") {
			help = _help;
			Path = path;
		}
	}
}
