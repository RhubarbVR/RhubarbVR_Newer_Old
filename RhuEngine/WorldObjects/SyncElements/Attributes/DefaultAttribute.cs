using System;

namespace RhuEngine.WorldObjects
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class DefaultAttribute : Attribute
	{
		public object Data { get; private set; }
		public DefaultAttribute(object value) {
			Data = value;
		}
	}
}
