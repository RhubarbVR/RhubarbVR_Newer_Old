using System;

namespace RhuEngine.WorldObjects
{
	public  class OnChangedAttribute : Attribute
	{
		public string Data { get; private set; }
		public OnChangedAttribute(string value) {
			Data = value;
		}
	}
}
