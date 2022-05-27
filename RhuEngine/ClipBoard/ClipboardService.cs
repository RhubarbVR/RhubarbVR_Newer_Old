#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TextCopy;

/// <summary>
/// Provides methods to place text on and retrieve text from the system Clipboard.
/// </summary>
public static partial class ClipboardService
{
    static readonly Func<CancellationToken, Task<string>> _getAsyncFunc;
	static readonly Func<string> _getFunc;

	/// <summary>
	/// Retrieves text data from the Clipboard.
	/// </summary>
	public static Task<string> GetTextAsync(CancellationToken cancellation = default)
    {
		if (OverRide is not null) {
			return OverRide.GetTextAsync();
		}
		return _getAsyncFunc(cancellation);
    }

	public static IClipboard OverRide;

    /// <summary>
    /// Retrieves text data from the Clipboard.
    /// </summary>
    public static string GetText()
    {
		if(OverRide is not null) {
			return OverRide.GetText();
		}
        return _getFunc();
    }

    static readonly Func<string, CancellationToken, Task> _setAsyncAction;
    static readonly Action<string> _setAction;

    static ClipboardService()
    {
        _getAsyncFunc = CreateAsyncGet();
        _getFunc = CreateGet();
        _setAsyncAction = CreateAsyncSet();
        _setAction = CreateSet();
    }

    /// <summary>
    /// Clears the Clipboard and then adds text data to it.
    /// </summary>
    public static Task SetTextAsync(string text, CancellationToken cancellation = default)
    {
		if (OverRide is not null) {
			return OverRide.SetTextAsync(text, cancellation);
		}
		return _setAsyncAction(text, cancellation);
    }

    /// <summary>
    /// Clears the Clipboard and then adds text data to it.
    /// </summary>
    public static void SetText(string text)
    {
		if (OverRide is not null) {
			OverRide.SetText(text);
		}
		else {
			_setAction(text);
		}
    }
}