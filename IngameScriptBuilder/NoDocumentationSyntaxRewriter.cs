using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace IngameScriptBuilder {
    public class NoDocumentationSyntaxRewriter : CSharpSyntaxRewriter {
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia) {
            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) || trivia.IsKind(SyntaxKind.EndOfDocumentationCommentToken) || trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)) {
                return default(SyntaxTrivia);
            }
            return base.VisitTrivia(trivia);
        }
    }
}