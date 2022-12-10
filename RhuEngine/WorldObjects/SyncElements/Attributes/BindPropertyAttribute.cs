using System;

namespace RhuEngine.WorldObjects
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class BindPropertyAttribute : Attribute
	{
		public string Data { get; private set; }
		public BindPropertyAttribute(string value) {
			Data = value;
		}
	}
}
