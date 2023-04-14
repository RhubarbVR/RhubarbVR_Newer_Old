using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace RhubarbEasyBuild
{
	public static class GodotRunner
	{
		public enum RunningPlatform
		{
			Windows,
			Linux,
			Macos,
		}

		public static RunningPlatform CurrentPlatform
		{
			get {
				return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? !Environment.Is64BitProcess ? throw new NotSupportedException("Needs 64 bit support") : RunningPlatform.Windows
					: RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
					? !Environment.Is64BitProcess ? throw new NotSupportedException("Needs 64 bit support") : RunningPlatform.Linux
					: RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
					? RunningPlatform.Macos
					: throw new NotSupportedException("Platform not supported");
			}
		}

		public const string VERSION = "4.0.2";

		public static string[] GodotDownloadURL = new string[] {
			$"https://github.com/godotengine/godot/releases/download/{VERSION}-stable/Godot_v{VERSION}-stable_mono_win64.zip", //Windows
			$"https://github.com/godotengine/godot/releases/download/{VERSION}-stable/Godot_v{VERSION}-stable_mono_linux_x86_64.zip", //Linux
			$"https://github.com/godotengine/godot/releases/download/{VERSION}-stable/Godot_v{VERSION}-stable_mono_macos.universal.zip", //Macos
		};

		public static string GetDownloadURL(RunningPlatform platform) {
			return GodotDownloadURL[(int)platform];
		}

		public static string GodotPath => Path.Combine(Directory.GetCurrentDirectory(), "godotbuilds", $"v{VERSION}");

		public static string WindowsExeGodotPath => Path.Combine(GodotPath, $"Godot_v{VERSION}-stable_mono_win64", $"Godot_v{VERSION}-stable_mono_win64.exe");
		public static string LinuxGodotPath => Path.Combine(GodotPath, $"Godot_v{VERSION}-stable_mono_linux_x86_64", $"Godot_v{VERSION}-stable_mono_linux.x86_64");
		public static string OSXGodotPath => Path.Combine(GodotPath, "Godot_mono.app", "Contents", "MacOS", "Godot");

		public static string GetExePath(RunningPlatform runningPlatform) {
			return runningPlatform switch {
				RunningPlatform.Windows => WindowsExeGodotPath,
				RunningPlatform.Linux => LinuxGodotPath,
				RunningPlatform.Macos => OSXGodotPath,
				_ => throw new NotSupportedException(),
			};
		}

		public static string TempFile => Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

		public static void Unzip(string zipFilePath, string extractPath) {
			if (!File.Exists(zipFilePath)) {
				throw new FileNotFoundException("Zip file not found.", zipFilePath);
			}
			if (!Directory.Exists(extractPath)) {
				Directory.CreateDirectory(extractPath);
			}
			ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);
		}

		public static async Task<bool> DownloadGodot() {
			if (Directory.Exists(GodotPath)) {
				return true;
			}
			var targetURL = GetDownloadURL(CurrentPlatform);
			var tempDownloadPath = TempFile + ".zip";
			Console.WriteLine($"Downloading Godot TargetURL:{targetURL} TempFile:{tempDownloadPath}");
			var downloader = new FileDownloader(targetURL, tempDownloadPath);
			if (!await downloader.StartDownload()) {
				Console.WriteLine($"Failed To download Godot");
				return false;
			}
			Unzip(tempDownloadPath, GodotPath);
			File.Delete(tempDownloadPath);
			return true;
		}

		public static Process RunGodot(string argument) {
			var exePath = GetExePath(CurrentPlatform);
			return !File.Exists(exePath)
				? throw new Exception("Run point not found")
				: Process.Start(new ProcessStartInfo {
					Arguments = argument,
					FileName = exePath,

				});
		}

	}
}