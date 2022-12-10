using System;


namespace RhuEngine.WorldObjects
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class NoSyncAttribute : Attribute
	{
		public NoSyncAttribute() {
		}
	}
}

