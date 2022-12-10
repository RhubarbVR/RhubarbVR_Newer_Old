using System;
using System.Collections.Generic;
using System.Linq;

namespace RhuEngine.WorldObjects.ECS
{
	[AttributeUsage(AttributeTargets.Class)]
	public class OverlayOnlyAttribute : Attribute
	{

	}
	[AttributeUsage(AttributeTargets.Class)]
	public class PrivateSpaceOnlyAttribute : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Class)]
	public class CategoryAttribute : Attribute
	{
		public readonly string[] Paths;

		public CategoryAttribute(params string[] paths) {
			var end = new List<string>();
			foreach (var item in paths) {
				var p = from e in item.Split('/', '\\')
						where !string.IsNullOrEmpty(e)
						select e;
				end.AddRange(p);
			}
			Paths = end.ToArray();
		}
	}
}
