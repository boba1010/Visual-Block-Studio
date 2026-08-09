using System.Runtime.InteropServices;

namespace Visual_Block_Studio.Helpers;

public static partial class Clipboard
{
    private const uint CF_UNICODETEXT = 13;
    private const uint GMEM_MOVEABLE = 0x0002;

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool OpenClipboard(IntPtr hWndNewOwner);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseClipboard();

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EmptyClipboard();

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial IntPtr GetClipboardData(uint uFormat);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GlobalLock(IntPtr hMem);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GlobalUnlock(IntPtr hMem);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GlobalFree(IntPtr hMem);

    public static void SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        IntPtr hwnd = GetForegroundWindow();

        if (!OpenClipboard(hwnd))
            return;

        IntPtr hGlobal = IntPtr.Zero;

        try
        {
            if (!EmptyClipboard())
                return;

            int byteCount = checked((text.Length + 1) * sizeof(char));

            hGlobal = GlobalAlloc(
                GMEM_MOVEABLE,
                (UIntPtr)byteCount);

            if (hGlobal == IntPtr.Zero)
                return;

            IntPtr target = GlobalLock(hGlobal);

            if (target == IntPtr.Zero)
                return;

            try
            {
                Marshal.Copy(
                    text.ToCharArray(),
                    0,
                    target,
                    text.Length);

                Marshal.WriteInt16(
                    target,
                    text.Length * sizeof(char),
                    0);
            }
            finally
            {
                GlobalUnlock(hGlobal);
            }

            if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
                return;

            hGlobal = IntPtr.Zero;
        }
        finally
        {
            if (hGlobal != IntPtr.Zero)
                GlobalFree(hGlobal);

            CloseClipboard();
        }
    }

    public static string GetText()
    {
        IntPtr hwnd = GetForegroundWindow();

        if (!OpenClipboard(hwnd))
            return string.Empty;

        try
        {
            IntPtr hGlobal = GetClipboardData(CF_UNICODETEXT);

            if (hGlobal == IntPtr.Zero)
                return string.Empty;

            IntPtr source = GlobalLock(hGlobal);

            if (source == IntPtr.Zero)
                return string.Empty;

            try
            {
                return Marshal.PtrToStringUni(source) ?? string.Empty;
            }
            finally
            {
                GlobalUnlock(hGlobal);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    public static void Clear()
    {
        IntPtr hwnd = GetForegroundWindow();

        if (!OpenClipboard(hwnd))
            return;

        try
        {
            EmptyClipboard();
        }
        finally
        {
            CloseClipboard();
        }
    }
}
