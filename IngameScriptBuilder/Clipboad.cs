using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace IngameScriptBuilder {
    public static class Clipboad {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public static void SetText(string text) {
            var isAscii = text != null && text == Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(text));
            SetText(text, isAscii ? Format.CF_UNICODETEXT : Format.CF_TEXT);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static void SetText(string text, Format format) {
            var thread = new Thread(x => {
                if (string.IsNullOrWhiteSpace(text)) {
                    return;
                }

                if (!OpenClipboard(IntPtr.Zero)) {
                    return;
                }

                uint sizeOfChar;
                IntPtr source;
                switch (format) {
                    case Format.CF_TEXT:
                        sizeOfChar = 1;
                        source = Marshal.StringToHGlobalAnsi(text);
                        break;
                    case Format.CF_UNICODETEXT:
                        sizeOfChar = 2;
                        source = Marshal.StringToHGlobalUni(text);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
                var characters = (uint) text.Length;
                var bytes = (characters + 1) * sizeOfChar;

                const int GMEM_MOVABLE = 0x0002;
                const int GMEM_ZEROINIT = 0x0040;
                const int GHND = GMEM_MOVABLE | GMEM_ZEROINIT;

                var hGlobal = GlobalAlloc(GHND, (UIntPtr) bytes);
                if (hGlobal == IntPtr.Zero) {
                    return;
                }

                try {
                    var target = GlobalLock(hGlobal);
                    if (target == IntPtr.Zero) {
                        return;
                    }

                    try {
                        CopyMemory(target, source, bytes);
                    } finally {
                        GlobalUnlock(target);
                    }

                    if (SetClipboardData((uint) format, hGlobal).ToInt64() != 0) {
                        hGlobal = IntPtr.Zero;
                    }
                } finally {
                    Marshal.FreeHGlobal(source);

                    if (hGlobal != IntPtr.Zero) {
                        GlobalFree(hGlobal);
                    }
                    CloseClipboard();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private enum Format : uint {
            CF_TEXT = 1,
            CF_UNICODETEXT = 13
        }
    }
}