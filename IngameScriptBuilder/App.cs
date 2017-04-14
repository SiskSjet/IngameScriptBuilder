using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;

namespace IngameScriptBuilder {
    public class App : CommandLineApplication {
        private readonly CommandOption _excludeDirectories;
        private readonly CommandOption _excludeFiles;
        private readonly CommandOption _help;
        private readonly CommandOption _minify;
        private readonly CommandArgument _output;
        private readonly CommandArgument _project;
        private readonly CommandOption _removeComments;
        private readonly CommandOption _removeDocumentations;
        private readonly CommandOption _version;

        public App() : base(false) {
            Name = AppInfo.FileName;
            FullName = AppInfo.Title;
            Description = AppInfo.Description;

            _project = Argument("Project", "Directory which include all your source files or a .csproj project file");
            _output = Argument("Output", "If empty script is generated in clipboard else to specified file");

            _minify = Option("-m | --minify", "Minify the ingame script", CommandOptionType.NoValue);
            _removeComments = Option("-r | --removeComments", "Generate script without comments", CommandOptionType.NoValue);
            _removeDocumentations = Option("-rd | --removeDocumentation", "Generate script without documentations", CommandOptionType.NoValue);
            _excludeDirectories = Option("-xd | --excludeDirectory", "Exclude an directory", CommandOptionType.MultipleValue);
            _excludeFiles = Option("-xf | --excludefile", "Exclude a file", CommandOptionType.MultipleValue);

            _version = Option("-v | --version", "Show version information", CommandOptionType.NoValue);
            _help = Option("-h | --help", "Show help information", CommandOptionType.NoValue);

            OnExecute((Func<int>)RunCommand);
        }

        public new void ShowHelp(string commandName = null) {
            var lines = Regex.Split(GetHelpText(commandName), "\r\n|\r|\n");
            lines[0] = $"{FullName} [Version {AppInfo.AssemblyVersion}]{Environment.NewLine}{Description}";
            Console.WriteLine(string.Join(Environment.NewLine, lines));
        }

        public new void ShowVersion() {
            Console.WriteLine(AppInfo.ProductVersion);
        }

        private int RunCommand() {
            if (_help.HasValue()) {
                ShowHelp();
                return 0;
            }
            if (_version.HasValue()) {
                ShowVersion();
                return 0;
            }

            if (string.IsNullOrWhiteSpace(_project.Value)) {
                Console.WriteLine("A project file or directory is required.");
                return 1;
            }

            if (!(Path.HasExtension(_project.Value) && Path.GetExtension(_project.Value) == ".csproj"))
                if (!Directory.Exists(_project.Value)) {
                    Console.WriteLine("No valid project found at {0}", _project.Value);
                    return 1;
                }

            // fixme: weird invalid char exeption on powershell autocomplete project path. try normalize project path.
            var project = _project.Value;
            var output = _output.Value;
            var minify = _minify.HasValue();
            var removeComments = _removeComments.HasValue();
            var removeDocumentations = _removeDocumentations.HasValue();
            var excludeFiles = _excludeFiles.Values;
            var excludeDirectories = _excludeDirectories.Values;

            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            Console.CancelKeyPress += delegate { cts.Cancel(); };

            try {
                // todo: implement parameters.
                Generator.GenerateAsync(project, ct).Wait(ct);
                return 0;
            } catch (Exception exception) {
                Console.WriteLine(exception);
                return 1;
            }
        }
    }
}