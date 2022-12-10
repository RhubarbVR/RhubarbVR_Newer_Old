using System;

namespace RhuEngine.WorldObjects
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class NoLoadAttribute : Attribute
	{
		public NoLoadAttribute() {
		}
	}
}
