using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TextCopy;

public static partial class ClipboardService
{
    static Func<CancellationToken, Task<string>> CreateAsyncGet()
    {
		return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? WindowsClipboard.GetTextAsync
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? OsxClipboard.GetTextAsync
			: RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxClipboard.GetTextAsync : throw new NotSupportedException();
	}

	static Func<string> CreateGet()
    {
		return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? WindowsClipboard.GetText
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? OsxClipboard.GetText
			: RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxClipboard.GetText : throw new NotSupportedException();
	}
}
