﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IngameScriptBuilder.SyntaxRewriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Options;

namespace IngameScriptBuilder {
    public class Generator {
        private const int IndentSize = 4;
        private const string MainClassName = "Program";
        private const string MainBaseName = "MyGridProgram";

        public static async Task GenerateAsync(string projectPath, string output, IEnumerable<string> excludeFiles, IEnumerable<string> excludeDirectories, bool removeComments, bool removeDocumentation, bool minify, CancellationToken cancellationToken) {
            var path = Path.GetFullPath(projectPath);
            var workspace = MSBuildWorkspace.Create();
            var options = CreateOptions(workspace);

            var rootDirectory = Path.HasExtension(path) ? Path.GetDirectoryName(path) ?? path.Replace(Path.GetFileName(path), "") : path;
            var filter = new List<string> { "Properties", "obj", "bin" };
            filter.AddRange(excludeDirectories);
            filter.AddRange(excludeFiles);
            filter = filter.ConvertAll(x => Path.Combine(rootDirectory, x));

            var member = new List<MemberDeclarationSyntax>();

            if (Path.HasExtension(projectPath)) {
                var project = await workspace.OpenProjectAsync(projectPath, cancellationToken);

                if (!project.HasDocuments) {
                    Console.WriteLine($"No documents found in project: {path}");
                    return;
                }

                var documents = project.Documents.Where(x => !filter.Any(x.FilePath.StartsWith));

                foreach (var document in documents) {
                    var tree = await document.GetSyntaxTreeAsync(cancellationToken);
                    var types = await GetMemberAsync(tree, cancellationToken);
                    member.AddRange(types);
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
                    member.AddRange(types);
                }
            }

            var mainClass = GetMainClass(member);
            member.Remove(mainClass);
            InjectTypes(ref mainClass, member);

            var rewriter = GetRewriter(removeComments, removeDocumentation, minify);
            Rewrite(ref mainClass, rewriter);

            if (!minify) {
                mainClass = (ClassDeclarationSyntax)Formatter.Format(mainClass, workspace, options, cancellationToken);
            }

            var result = SyntaxFactory.CompilationUnit().WithMembers(mainClass.Members).GetText(Encoding.UTF8).ToString();
            if (!minify) {
                result = UnindentAsMuchAsPossible(new string(' ', IndentSize) + result.Trim());
            }

            Console.WriteLine($"{AppInfo.Title} [Version {AppInfo.AssemblyVersion}]{Environment.NewLine}");
            Console.WriteLine($"Length: {result.Length} / 100000");
            if (string.IsNullOrWhiteSpace(output)) {
                Clipboard.SetText(result);
                Console.WriteLine("Script copied to Clipboard.");
            } else {
                var outputPath = Path.GetFullPath(output);
                var outputDirectory = Path.GetDirectoryName(outputPath);
                if (outputDirectory != null) {
                    Directory.CreateDirectory(outputDirectory);
                }

                using (var writer = new StreamWriter(output)) {
                    await writer.WriteAsync(result);
                    Console.WriteLine($"Script written to file: {output}");
                }
            }
        }

        private static IEnumerable<CSharpSyntaxRewriter> GetRewriter(bool removeComments, bool removeDocumentation, bool minify) {
            var rewriter = new List<CSharpSyntaxRewriter>();
            if (removeComments || minify) {
                rewriter.Add(new NoCommentsSyntaxRewriter());
            }

            if (removeDocumentation || minify) {
                rewriter.Add(new NoDocumentationSyntaxRewriter());
            }

            if (minify) {
                rewriter.Add(new RemoveDefaultAccessModifier());
                rewriter.Add(new RemoveEmtpyStatementRewriter());
                rewriter.Add(new RemoveRegionRewriter());
                rewriter.Add(new CompressWhiteSpaceRewriter());
                rewriter.Add(new TypeNewLineRewriter());
            }
            return rewriter;
        }
        private static void Rewrite(ref ClassDeclarationSyntax mainClass, IEnumerable<CSharpSyntaxRewriter> rewriter) {
            mainClass = rewriter.Aggregate(mainClass, (current, syntaxRewriter) => (ClassDeclarationSyntax)syntaxRewriter.Visit(current));
        }

        private static async Task<IEnumerable<MemberDeclarationSyntax>> GetMemberAsync(SyntaxTree tree, CancellationToken cancellationToken) {
            var root = (CompilationUnitSyntax)await tree.GetRootAsync(cancellationToken);
            return root.DescendantNodes(x => x is CompilationUnitSyntax || x is NamespaceDeclarationSyntax).OfType<MemberDeclarationSyntax>().Where(x => x.GetType() != typeof(NamespaceDeclarationSyntax));
        }

        private static string UnindentAsMuchAsPossible(string text) {
            var lines = Regex.Split(text, "\r\n|\r|\n");
            var minDistance = lines.Where(line => line.Length > 0).Min(line => line.TakeWhile(char.IsWhiteSpace).Sum(c => c == '\t' ? IndentSize : 1));
            var result = string.Join(Environment.NewLine, lines.Select(line => line.Replace("\t", new string(' ', IndentSize))).Select(line => line.Substring(Math.Min(line.Length, minDistance))));
            return result;
        }

        private static void InjectTypes(ref ClassDeclarationSyntax mainClass, IEnumerable<MemberDeclarationSyntax> types) {
            // todo: sort nested member like: type > name.
            //mainClass.AddMembers(types.Where(x => x != mainClass).ToArray());
            mainClass = mainClass.AddMembers(types.ToArray());
        }

        private static ClassDeclarationSyntax GetMainClass(IEnumerable<MemberDeclarationSyntax> member) {
            var result = member.OfType<ClassDeclarationSyntax>().FirstOrDefault(x => x.Identifier.Text == MainClassName);
            if (result?.BaseList == null || !result.BaseList.ToString().Contains(MainBaseName)) {
                Console.WriteLine("No entry point found.");
                Console.WriteLine($"Create a '{MainClassName}' class that inherit from '{MainBaseName}'");
                return null;
            }
            return result;
        }

        private static OptionSet CreateOptions(Workspace workspace) {
            return workspace.Options
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
        }
    }
}