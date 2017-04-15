using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace IngameScriptBuilder {
    public class NoCommentsSyntaxRewriter : CSharpSyntaxRewriter {
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia) {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                return default(SyntaxTrivia);
            }
            return base.VisitTrivia(trivia);
        }
    }
}