using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.Managers;
using RhuEngine.Physics;
using RhuEngine.Settings;

using RhuSettings;

using RNumerics;

namespace RhuEngine
{
	public static class EngineHelpers
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
		public static Engine MainEngine;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
		public static string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
	}
}