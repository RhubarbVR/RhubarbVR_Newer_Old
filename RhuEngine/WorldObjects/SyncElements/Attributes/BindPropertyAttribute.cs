using System;

namespace RhuEngine.WorldObjects
{
	public sealed class BindPropertyAttribute : Attribute
	{
		public string Data { get; private set; }
		public BindPropertyAttribute(string value) {
			Data = value;
		}
	}
}
