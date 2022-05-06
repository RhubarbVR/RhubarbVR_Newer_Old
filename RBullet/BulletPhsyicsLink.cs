using System;
using System.Runtime.InteropServices;

using RhuEngine.Physics;
using BulletSharp;

namespace RBullet
{  
	public class BulletPhsyicsLink: PhysicsHelper.Physics<BulletRigidBodyCollider, BulletPhysicsSim, BulletColliderShape>
    {
		public static class AndroidTest
		{
			static bool? _isAndroid;
			/// <summary>
			/// Run Chech
			/// </summary>
			/// <returns>If it is anroid</returns>
			public static bool Check() {
				if (_isAndroid != null) {
					return (bool)_isAndroid;
				}
				using (var process = new System.Diagnostics.Process()) {
					process.StartInfo.FileName = "getprop";
					process.StartInfo.Arguments = "ro.build.user";
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.CreateNoWindow = true;
					try {
						process.Start();
						var output = process.StandardOutput.ReadToEnd();
						_isAndroid = string.IsNullOrEmpty(output) ? (bool?)false : (bool?)true;
					}
					catch {
						_isAndroid = false;
					}
					return (bool)_isAndroid;
				}
			}
		}


		static class NativeLib
		{
			static bool _loaded = false;
			internal static bool Load() {
				if (_loaded) {
					return true;
				}

				// Android uses a different strategy for linking the DLL
				if (AndroidTest.Check()) {
					return true;
				}

				var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64
					? "arm64"
					: "x64";
				_loaded = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? LoadWindows(arch)
					: LoadUnix(arch);
				return _loaded;
			}

			[DllImport("kernel32", CharSet = CharSet.Unicode)]
			static extern IntPtr LoadLibraryW(string fileName);
			static bool LoadWindows(string arch) {
				return LoadLibraryW("libbulletc.dll") != IntPtr.Zero || LoadLibraryW($"runtimes/win-{arch}/native/libbulletc.dll") != IntPtr.Zero;
			}


			[DllImport("libdl", CharSet = CharSet.Ansi)]
			static extern IntPtr dlopen(string fileName, int flags);
			static bool LoadUnix(string arch) {
				const int RTLD_NOW = 2;
				return dlopen("libbulletc.dll", RTLD_NOW) != IntPtr.Zero
					|| dlopen($"./runtimes/linux-{arch}/native/libbulletc.dll", RTLD_NOW) != IntPtr.Zero
					|| dlopen($"{AppDomain.CurrentDomain.BaseDirectory}/runtimes/linux-{arch}/native/libbulletc.dll", RTLD_NOW) != IntPtr.Zero;
			}
		}

		public BulletPhsyicsLink() {
			if (!NativeLib.Load()) {
				throw new Exception("Failed to load lib");
			}
		}

	}
}
