using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

static class OsxClipboard
{
    static readonly IntPtr _nsString = objc_getClass("NSString");
    static readonly IntPtr _nsPasteboard = objc_getClass("NSPasteboard");
	static readonly IntPtr _nsStringPboardType;
	static readonly IntPtr _utfTextType;
    static readonly IntPtr _generalPasteboard;
    static readonly IntPtr _initWithUtf8Register = sel_registerName("initWithUTF8String:");
    static readonly IntPtr _allocRegister = sel_registerName("alloc");
    static readonly IntPtr _setStringRegister = sel_registerName("setString:forType:");
    static readonly IntPtr _stringForTypeRegister = sel_registerName("stringForType:");
    static readonly IntPtr _utf8Register = sel_registerName("UTF8String");
    static readonly IntPtr _generalPasteboardRegister = sel_registerName("generalPasteboard");
    static readonly IntPtr _clearContentsRegister = sel_registerName("clearContents");

    static OsxClipboard()
    {
        _utfTextType = objc_msgSend(objc_msgSend(_nsString, _allocRegister), _initWithUtf8Register, "public.utf8-plain-text");
        _nsStringPboardType = objc_msgSend(objc_msgSend(_nsString, _allocRegister), _initWithUtf8Register, "NSStringPboardType");

        _generalPasteboard = objc_msgSend(_nsPasteboard, _generalPasteboardRegister);
    }

    public static string GetText()
    {
        var ptr = objc_msgSend(_generalPasteboard, _stringForTypeRegister, _nsStringPboardType);
        var charArray = objc_msgSend(ptr, _utf8Register);
        return Marshal.PtrToStringAnsi(charArray);
    }

    public static Task<string> GetTextAsync(CancellationToken _)
    {
        return Task.FromResult(GetText());
    }

    public static void SetText(string text)
    {
        IntPtr str = default;
        try
        {
            str = objc_msgSend(objc_msgSend(_nsString, _allocRegister), _initWithUtf8Register, text);
            objc_msgSend(_generalPasteboard, _clearContentsRegister);
            objc_msgSend(_generalPasteboard, _setStringRegister, str, _utfTextType);
        }
        finally
        {
            if (str != default)
            {
                objc_msgSend(str, sel_registerName("release"));
            }
        }
    }

    public static Task SetTextAsync(string text, CancellationToken _)
    {
        SetText(text);
        return Task.CompletedTask;
    }

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit", CharSet = CharSet.Unicode)]
    static extern IntPtr objc_getClass(string className);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit", CharSet = CharSet.Unicode)]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit", CharSet = CharSet.Unicode)]
    static extern IntPtr sel_registerName(string selectorName);
}
