using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;

namespace IngameScriptBuilder {
    public class Generator {
        private const int IndentSize = 4;
        public static async Task GenerateAsync(string projectPath, string output, IEnumerable<string> excludeFiles, IEnumerable<string> excludeDirectories, CancellationToken cancellationToken) {
            var path = Path.GetFullPath(projectPath);
            var workspace = MSBuildWorkspace.Create();
            var options = workspace.Options
                .WithChangedOption(CSharpFormattingOptions.IndentBlock, true)
                .WithChangedOption(CSharpFormattingOptions.IndentBraces, false)
                .WithChangedOption(CSharpFormattingOptions.IndentSwitchCaseSection, true)
                .WithChangedOption(CSharpFormattingOptions.IndentSwitchSection, true)

                .WithChangedOption(CSharpFormattingOptions.NewLineForCatch, false)
                .WithChangedOption(CSharpFormattingOptions.NewLineForClausesInQuery, true)
                .WithChangedOption(CSharpFormattingOptions.NewLineForElse, false)
                .WithChangedOption(CSharpFormattingOptions.NewLineForFinally, false)
                .WithChangedOption(CSharpFormattingOptions.NewLineForMembersInAnonymousTypes, true)
                .WithChangedOption(CSharpFormattingOptions.NewLineForMembersInObjectInit, true)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false)
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, false)

                .WithChangedOption(CSharpFormattingOptions.SpaceAfterCast, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceAfterColonInBaseTypeDeclaration, true)
                .WithChangedOption(CSharpFormattingOptions.SpaceAfterComma, true)
                .WithChangedOption(CSharpFormattingOptions.SpaceAfterControlFlowStatementKeyword, true)
                .WithChangedOption(CSharpFormattingOptions.SpaceAfterDot, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceAfterMethodCallName, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceAfterSemicolonsInForStatement, true)

                .WithChangedOption(CSharpFormattingOptions.SpaceBeforeColonInBaseTypeDeclaration, true)
                .WithChangedOption(CSharpFormattingOptions.SpaceBeforeComma, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceBeforeDot, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceBeforeOpenSquareBracket, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceBeforeSemicolonsInForStatement, false)

                .WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptyMethodCallParentheses, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptyMethodDeclarationParentheses, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceBetweenEmptySquareBrackets, false)

                .WithChangedOption(CSharpFormattingOptions.SpaceWithinCastParentheses, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceWithinExpressionParentheses, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceWithinMethodCallParentheses, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceWithinMethodDeclarationParenthesis, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceWithinOtherParentheses, false)
                .WithChangedOption(CSharpFormattingOptions.SpaceWithinSquareBrackets, false)

                .WithChangedOption(CSharpFormattingOptions.SpacesIgnoreAroundVariableDeclaration, false)
                .WithChangedOption(CSharpFormattingOptions.SpacingAfterMethodDeclarationName, false)
                .WithChangedOption(CSharpFormattingOptions.WrappingKeepStatementsOnSingleLine, false)
                .WithChangedOption(CSharpFormattingOptions.WrappingPreserveSingleLine, true);

            var rootDirectory = Path.HasExtension(path) ? Path.GetDirectoryName(path) ?? path.Replace(Path.GetFileName(path), "") : path;
            var filter = new List<string> { "Properties", "obj", "bin" };
            filter.AddRange(excludeDirectories);
            filter.AddRange(excludeFiles);
            filter = filter.ConvertAll(x => Path.Combine(rootDirectory, x));

            var members = new List<MemberDeclarationSyntax>();

            if (Path.HasExtension(projectPath)) {
                var project = await workspace.OpenProjectAsync(projectPath, cancellationToken);
                Console.WriteLine($"Building {project.Name}");
                // bug: no documents in loaded project.
                // todo: figure out why no documents exist in loaded projects.
                Console.WriteLine($"{project.Documents.Count()} documents");
                if (!project.HasDocuments) {
                    Console.WriteLine($"No documents found in project: {path}");
                    Console.WriteLine("Won't work until i figured out why loaded projects don't have documents.");
                    return;
                }

                // note: if file path is relative convert to full path. can't check it until i fix the no document thing.
                var documents = project.Documents.Where(x => !filter.Any(x.FilePath.StartsWith));

                foreach (var document in documents) {
                    var tree = await document.GetSyntaxTreeAsync(cancellationToken);
                    var types = await GetMemberAsync(tree, cancellationToken);
                    members.AddRange(types);
                }
            } else {
                if (!Directory.GetFiles(path, "*.cs").Any()) {
                    Console.WriteLine($"No source files found in {path}. Make sure you select the project root.");
                    return;
                }

                var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories).Where(x => !filter.Any(x.StartsWith));

                foreach (var file in files) {
                    var code = await new StreamReader(file).ReadToEndAsync();
                    var tree = CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default, "", null, cancellationToken);
                    var types = await GetMemberAsync(tree, cancellationToken);
                    members.AddRange(types);
                }
            }

            var entry = members.OfType<ClassDeclarationSyntax>().FirstOrDefault(x => x.Identifier.Text == "Program");
            if (entry?.BaseList == null || !entry.BaseList.ToString().Contains("MyGridProgram")) {
                Console.WriteLine($"No entry point found. {Environment.NewLine}Create a 'Program' class that inherit from 'MyGridProgram'");
                return;
            }
            // todo: sort nested member like: type > name.
            var newEntry = entry.AddMembers(members.Where(x => x != entry).ToArray());
            var formatedEntry = Formatter.Format(newEntry, workspace, options, cancellationToken);

            string result;
            if (formatedEntry == null) {
                Console.WriteLine("Unexpected format error. Resumeing without formating.");
                result = newEntry.GetText().ToString();
            } else {
                result = formatedEntry.GetText().ToString();
            }

            result = result.Remove(0, result.IndexOf("{", StringComparison.Ordinal) + 1);
            result = result.Remove(result.LastIndexOf("}", StringComparison.Ordinal));
            result = UnindentAsMuchAsPossible(new string(' ', IndentSize) + result.Trim());

            Console.WriteLine($"{AppInfo.Title} [Version {AppInfo.AssemblyVersion}]{Environment.NewLine}");
            Console.WriteLine($"Length: {result.Length} / 100000");
            if (string.IsNullOrWhiteSpace(output)) {
                Clipboad.SetText(result);
                Console.WriteLine("Script copied to Clipboad.");
            } else {
                using (var writer = new StreamWriter(output)) {
                    await writer.WriteAsync(result);
                    Console.WriteLine($"Script written to file: {output}");
                }
            }
        }

        private static string UnindentAsMuchAsPossible(string text) {
            var lines = Regex.Split(text, "\r\n|\r|\n");
            var minDistance = lines.Where(line => line.Length > 0).Min(line => line.TakeWhile(char.IsWhiteSpace).Sum(c => c == '\t' ? IndentSize : 1));
            var result = string.Join(Environment.NewLine, lines.Select(line => line.Replace("\t", new string(' ', IndentSize))).Select(line => line.Substring(Math.Min(line.Length, minDistance))));
            return result;
        }

        private static async Task<IEnumerable<MemberDeclarationSyntax>> GetMemberAsync(SyntaxTree tree, CancellationToken cancellationToken) {
            var root = (CompilationUnitSyntax)await tree.GetRootAsync(cancellationToken);
            return root.DescendantNodes(x => x is CompilationUnitSyntax || x is NamespaceDeclarationSyntax).OfType<MemberDeclarationSyntax>().Where(x => x.GetType() != typeof(NamespaceDeclarationSyntax));
        }
    }
}