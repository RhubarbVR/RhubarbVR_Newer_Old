using System.Diagnostics;

namespace RhubarbEasyBuild
{
	public static class Program
	{
		public static string RhubarbDir;

		static readonly string[] _options = new string[] {
			"-run-debug-novr",
			"-run-debug-vr",
			"-run-debug-vsdebug-novr",
			"-run-debug-vsdebug-vr",
			"-run-debug-visuals-novr",
			"-run-debug-visuals-vr",
			"-build-debug",
			"-build-headless"
		};

		static bool IsRhubarbDir(string path) {
			if (File.Exists(Path.Combine(path, "RhubarbGodot", "project.godot"))) {
				RhubarbDir = path;
				return true;
			}
			return false;
		}

		static async Task Main(string[] args) {
			Console.WriteLine("Welcome To RhubarbVR Easy Build!");
			var workingDir = Directory.GetCurrentDirectory();
			if (!(IsRhubarbDir(workingDir) || IsRhubarbDir(Path.Combine(workingDir, "..")) || IsRhubarbDir(Path.Combine(workingDir, "..", "..", "..", "..")))) {
				Console.WriteLine("Failed to find rhubarb project");
				return;
			}
			if (RhubarbDir is null) {
				Console.WriteLine("Failed to find rhubarb project");
				return;
			}
			RhubarbDir = Path.GetFullPath(RhubarbDir);
			Console.WriteLine($"Rhubarb Project Path:{RhubarbDir}");
			string selectedOption;
			if (args.Length == 0) {
				Console.WriteLine("Choose what you want to do!");
				for (var i = 0; i < _options.Length; i++) {
					Console.WriteLine($"{i}: {_options[i]}");
				}
			choose:
				Console.Write("Choose:");
				var selected = Console.ReadLine();
				if (!int.TryParse(selected, out var selectedIndex)) {
					Console.WriteLine("Put in a number");
					goto choose;
				}
				if (selectedIndex >= _options.Length) {
					Console.WriteLine($"{selectedIndex} is Not valid option");
					goto choose;
				}
				selectedOption = _options[selectedIndex];
			}
			else {
				selectedOption = args[0];
			}
			selectedOption = selectedOption.ToLower();
			Console.WriteLine($"Selected {selectedOption}");
			if (selectedOption.Contains("run-debug-novr")) {
				await RunDebug(false);
			}
			else if (selectedOption.Contains("run-debug-vr")) {
				await RunDebug(true);
			}
			else if (selectedOption.Contains("run-debug-vsdebug-novr")) {
				await RunDebug(false, "-run-vsdebug");
			}
			else if (selectedOption.Contains("run-debug-vsdebug-vr")) {
				await RunDebug(true, "-run-vsdebug");
			}
			else if (selectedOption.Contains("run-debug-visuals-novr")) {
				await RunDebug(false, "-debug-visuals");
			}
			else if (selectedOption.Contains("run-debug-visuals-vr")) {
				await RunDebug(true, "-debug-visuals");
			}
			else if (selectedOption.Contains("build-debug")) {
				await BuildDebug();
			}
			else if (selectedOption.Contains("build-headless")) {
				await BuildHeadLess();
			}
			else {
				Console.WriteLine("Command Not known");
			}
		}

		static string BaseCommand => $"\"{Path.Combine(RhubarbDir, "RhubarbGodot")}\"";

		private static void CopyFilesRecursively(string sourcePath, string targetPath) {
			Directory.CreateDirectory(targetPath);
			foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
			}

			foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
				File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
			}
		}

		static async Task BuildDebug() {
			await GodotRunner.DownloadGodot();
			var process = GodotRunner.RunGodot("--path " + BaseCommand + " --build-solutions --headless --quit ");
			await process.WaitForExitAsync();
			process.Dispose();

			Console.WriteLine("Done building rhubarb now copying files");
			var files = Path.Combine(RhubarbDir, "RhubarbGodot", ".godot", "mono", "temp", "bin", "Debug");
			var copyToPath = Path.Combine(RhubarbDir, "RhubarbGodot");
			if (!Directory.Exists(files)) {
				throw new Exception("Failed to find build files");
			}
			foreach (var dirs in Directory.GetDirectories(files)) {
				var name = Path.GetFileName(dirs);
				Console.WriteLine($"Copying {name}");
				CopyFilesRecursively(dirs, Path.Combine(copyToPath, name));
			}


		}

		static async Task BuildHeadLess() {
			var process = Process.Start(new ProcessStartInfo {
				Arguments = "build ./Rhubarb_VR_HeadLess/Rhubarb_VR_HeadLess.csproj",
				FileName = "dotnet",
				WorkingDirectory = Program.RhubarbDir
			});
			await process.WaitForExitAsync();
			process.Dispose();
		}

		static async Task RunDebug(bool vr, params string[] extraArgs) {
			await BuildDebug();
			var extraComand = string.Join(' ', extraArgs);
			var runInVRCommand = vr ? " --xr-mode on" : "";
			var process = GodotRunner.RunGodot("--path " + BaseCommand + runInVRCommand + " " + extraComand);
			await process.WaitForExitAsync();
			process.Dispose();
		}

	}
}