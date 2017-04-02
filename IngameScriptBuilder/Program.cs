using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace IngameScriptBuilder {
    class Program {
        static int Main(string[] args) {
            try {
                return new App().Execute(args);
            } catch (Exception ex) {
                Console.Write(ex);
                return 1;
            }
        }
    }
}
