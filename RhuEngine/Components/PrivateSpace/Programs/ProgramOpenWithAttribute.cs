using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using TextCopy;

namespace RhuEngine.Components
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ProgramOpenWithAttribute : Attribute
	{
		public string[] mimeTypes = Array.Empty<string>();

		public ProgramOpenWithAttribute(params string[] mimeTypes) {
			this.mimeTypes = mimeTypes;
		}

		public ProgramOpenWithAttribute(string mimeTypes) {
			this.mimeTypes = new string[] { mimeTypes };
		}
		

		public static IEnumerable<Type> GetAllPrograms(string targetMime,params Assembly[] asim) {
			foreach (var item in asim.SelectMany(x => x.GetTypes()).Where(x => x.GetCustomAttribute<ProgramOpenWithAttribute>(true) is not null)) {
				if(targetMime == "*") {
					yield return item;
				}
				else {
					var types = item.GetCustomAttribute<ProgramOpenWithAttribute>(true).mimeTypes;
					if(types.Contains(targetMime) || types.Contains("*")) {
						yield return item;
					}
				}
			}
		}
	}
}
