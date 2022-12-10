using System;

namespace RhuEngine.WorldObjects
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class NoSyncUpdateAttribute : Attribute
	{
		public NoSyncUpdateAttribute() {
		}
	}
}
