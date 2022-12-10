using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Reflection;

namespace OpusDotNet
{
	/// <summary>
	/// Android Test
	/// </summary>
	public static class AndroidTest
	{
		/// <summary>
		/// Run Chech
		/// </summary>
		/// <returns>If it is anroid</returns>
		public static bool Check() {
			return OperatingSystem.IsAndroid();
		}
	}

	/// <summary>
	/// The Library manager
	/// </summary>
	public static class NativeLib
	{
		/// <summary>
		/// Makes think lib is already loaded
		/// </summary>
		public static void ForceLoad() {
			_loaded = true;
		}

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
			return LoadLibraryW("opus") != IntPtr.Zero || LoadLibraryW($"runtimes/win-{arch}/native/opus.dll") != IntPtr.Zero;
		}


		[DllImport("libdl", CharSet = CharSet.Unicode)]
		static extern IntPtr dlopen(string fileName, int flags);

		static bool LoadUnix(string arch) {
			const int RTLD_NOW = 2;
			return dlopen("opus.so", RTLD_NOW) != IntPtr.Zero
				|| dlopen($"./runtimes/linux-{arch}/native/opus.so", RTLD_NOW) != IntPtr.Zero
				|| dlopen($"{AppDomain.CurrentDomain.BaseDirectory}/runtimes/linux-{arch}/native/opus.so", RTLD_NOW) != IntPtr.Zero;
		}
	}

	internal static class API
	{

		// Encoder

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern SafeEncoderHandle opus_encoder_create(int Fs, int channels, int application, out int error);

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern void opus_encoder_destroy(IntPtr st);

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern int opus_encode(SafeEncoderHandle st, IntPtr pcm, int frame_size, IntPtr data, int max_data_bytes);

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern int opus_encoder_ctl(SafeEncoderHandle st, int request, out int value);

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern int opus_encoder_ctl(SafeEncoderHandle st, int request, int value);

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern int opus_encode_float(SafeEncoderHandle st, float[] pcm, int frame_size, byte[] data, int max_data_bytes);
		// Decoder

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern SafeDecoderHandle opus_decoder_create(int Fs, int channels, out int error);

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern void opus_decoder_destroy(IntPtr st);

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern int opus_decode(SafeDecoderHandle st, IntPtr data, int len, IntPtr pcm, int frame_size, int decode_fec);

		[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
		public static extern int opus_decode_float(SafeDecoderHandle st, byte[] data, int len, float[] pcm, int frame_size, int decode_fec);
		// Helper Methods

		public static int GetSampleCount(double frameSize, int sampleRate) {
			// Number of samples per channel.
			return (int)(frameSize * sampleRate / 1000);
		}

		public static int GetPCMLength(int samples, int channels) {
			// 16-bit audio contains a sample every 2 (16 / 8) bytes, so we multiply by 2.
			return samples * channels * 2;
		}

		public static double GetFrameSize(int pcmLength, int sampleRate, int channels) {
			return (double)pcmLength / sampleRate / channels / 2 * 1000;
		}

		public static void ThrowIfError(int result) {
			if (result < 0) {
				throw new OpusException(result);
			}
		}
	}
}
