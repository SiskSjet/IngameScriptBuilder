using System;
using System.Linq;

namespace IngameScriptBuilder {
    internal class Program {
        private static int Main(string[] args) {
            if (args.Any(x => x.Count(c => c == '\"') % 2 > 0)) {
                Console.WriteLine("Unexpected excaped quote.");
                return 1;
            }

            try {
                return new App().Execute(args);
            } catch (Exception ex) {
                Console.Write(ex);
                return 1;
            }
        }
    }
}