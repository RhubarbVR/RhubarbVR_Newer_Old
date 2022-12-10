using System;

namespace RhuEngine.WorldObjects
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class OnAssetLoadedAttribute : Attribute
	{
		public string Data { get; private set; }
		public OnAssetLoadedAttribute(string value) {
			Data = value;
		}
	}
}
