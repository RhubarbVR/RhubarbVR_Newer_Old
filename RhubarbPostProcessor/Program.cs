using System;

namespace RhubarbPostProcessor
{
	public sealed class RhubarbPostProcessor
	{
		static void Main(string[] args) {
			if (args.Length == 0) {
				Console.WriteLine("Need to specify Target DLL");
				return;
			}
			var targetFile = Path.GetFullPath(args[0]);
			Console.WriteLine($"Starting PostProcessor On DLL {targetFile}");
			var exstras = new string[args.Length - 1];
			for (var i = 0; i < exstras.Length; i++) {
				exstras[i] = args[i + 1];
				Console.WriteLine($"Added Extra LibFolder {exstras[i]}");
			}
			new RhuPostProcessor.PostProcess().ProcessDLL(targetFile, exstras);
		}
	}
}