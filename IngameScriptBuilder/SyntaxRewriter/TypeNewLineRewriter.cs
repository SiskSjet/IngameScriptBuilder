using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IngameScriptBuilder.SyntaxRewriter {
    internal class TypeNewLineRewriter : CSharpSyntaxRewriter {
        private static void AddLineBreak<T>(ref T node) where T : SyntaxNode {
            var trivia = SyntaxTriviaList.Create(SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n"));
            node = node.WithTrailingTrivia(trivia);
            node = node.WithLeadingTrivia(SyntaxTriviaList.Empty);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node) {
            AddLineBreak(ref node);
            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node) {
            AddLineBreak(ref node);
            return base.VisitDelegateDeclaration(node);
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node) {
            AddLineBreak(ref node);
            return base.VisitEnumDeclaration(node);
        }

        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
            AddLineBreak(ref node);
            return base.VisitInterfaceDeclaration(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node) {
            AddLineBreak(ref node);
            return base.VisitStructDeclaration(node);
        }
    }
}