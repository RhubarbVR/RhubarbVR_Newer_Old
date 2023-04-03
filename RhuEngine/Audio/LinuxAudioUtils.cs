using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace RhuEngine
{
	public unsafe static class LinuxAudioUtils
	{
		private const int SND_PCM_STREAM_CAPTURE = 0;
		private const int SND_PCM_NONBLOCK = 1;

		[DllImport("libasound.so")]
		private static extern int snd_device_name_hint(int card, string iface, out IntPtr hints);

		[DllImport("libasound.so")]
		private static extern void snd_device_name_free_hint(IntPtr hints);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_open(out IntPtr handle, string name, int stream, int mode);

		[DllImport("libasound.so")]
		private static extern int snd_pcm_close(IntPtr handle);

		[DllImport("libasound.so")]
		private static extern int snd_device_name_get_hint(IntPtr hint, string id, out IntPtr name);

		[DllImport("libasound.so")]
		private static extern IntPtr snd_device_name_hint_next(IntPtr hint);

		[DllImport("libasound.so")]
		private static extern void snd_device_name_free(IntPtr name);

		[DllImport("libasound.so")]
		private static extern string snd_strerror(int errnum);

		private static bool CheckForlibasound() {
			try {
				snd_device_name_hint(-1, "pcm", out var hintsPtr);
			}
			catch (DllNotFoundException) {
				return false;
			}
			return true;
		}

		public static IEnumerable<string> GetInputDeviceNames() {
			var err = snd_device_name_hint(-1, "pcm", out var hintsPtr);

			if (err != 0) {
				throw new Exception($"Failed to get device names: {snd_strerror(err)}");
			}

			try {
				var deviceNames = new List<string>();
				var curHint = hintsPtr;

				while (curHint != IntPtr.Zero) {
					var descPtr = IntPtr.Zero;
					var err2 = snd_device_name_get_hint(curHint, "NAME", out var namePtr);

					if (err2 == 0 && namePtr != IntPtr.Zero) {
						var name = Marshal.PtrToStringAnsi(namePtr);

						if (!string.IsNullOrWhiteSpace(name)) {
							// Try to open the device to see if it's an input device
							var err3 = snd_pcm_open(out var handle, name, SND_PCM_STREAM_CAPTURE, SND_PCM_NONBLOCK);

							if (err3 == 0) {
								snd_pcm_close(handle);
								deviceNames.Add(name);
							}
						}
					}

					snd_device_name_free(namePtr);
					snd_device_name_free(descPtr);

					curHint = snd_device_name_hint_next(curHint);
				}

				return deviceNames;
			}
			finally {
				snd_device_name_free_hint(hintsPtr);
			}
		}
	}
}
