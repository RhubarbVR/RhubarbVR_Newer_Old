using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhubarbSourceGen;
using System.Threading;

namespace SourceGenerator
{

	[Generator]
	public class RhubarbSourceGenerator : ISourceGenerator
	{
		public static bool isRhuEngineDebug;
		public static bool isRhuEngine;
		public static bool isRNumerics;

		public void Execute(GeneratorExecutionContext context) {
			isRhuEngineDebug = context.Compilation.AssemblyName == "RhuEngineDebug";
			isRhuEngine = isRhuEngineDebug || context.Compilation.AssemblyName == "RhuEngine";
			if (isRhuEngine) {
#if DEBUG
				//if (!Debugger.IsAttached) {
				//	Debugger.Launch();
				//}
#endif
				Debug.WriteLine($"Execute code generator {(isRhuEngineDebug ? "RhuEngineDebug" : "RhuEngine")}");
				SyncObjectProcessor.Build(context);
			}
			isRNumerics = context.Compilation.AssemblyName == "RNumerics";
			if (isRNumerics) {
#if DEBUG
				//if (!Debugger.IsAttached) {
				//	Debugger.Launch();
				//}
#endif
				Debug.WriteLine("Execute code generator RNumerics");
			}
		}

		public void Initialize(GeneratorInitializationContext context) {

		}
	}
}
