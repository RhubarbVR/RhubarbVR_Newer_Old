using System;

namespace RhuEngine.WorldObjects
{
	public class OnAssetLoadedAttribute : Attribute
	{
		public string Data { get; private set; }
		public OnAssetLoadedAttribute(string value) {
			Data = value;
		}
	}
}
