#if (NETSTANDARD || NETFRAMEWORK || NET5_0_OR_GREATER)
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TextCopy;

public static partial class ClipboardService
{
    static Func<string, CancellationToken, Task> CreateAsyncSet()
    {
		return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? WindowsClipboard.SetTextAsync
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? OsxClipboard.SetTextAsync
			: RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxClipboard.SetTextAsync : ((_, _) => throw new NotSupportedException());
	}

	static Action<string> CreateSet()
    {
		return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? WindowsClipboard.SetText
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? OsxClipboard.SetText
			: RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxClipboard.SetText : (_ => throw new NotSupportedException());
	}
}
#endif