﻿using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;

namespace IngameScriptBuilder {
    public static class Clipboard {
        public static void SetText(string text) {
            SetText(text, TextDataFormat.UnicodeText);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static void SetText(string text, TextDataFormat format) {
            var thread = new Thread(x => {
                if (string.IsNullOrWhiteSpace(text)) {
                    return;
                }
                System.Windows.Forms.Clipboard.SetText(text, format);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}