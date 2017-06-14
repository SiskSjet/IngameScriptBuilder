using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IngameScriptBuilder.SyntaxRewriter {
    public class RemoveEmtpyStatementRewriter : CSharpSyntaxRewriter {
        public override SyntaxNode VisitEmptyStatement(EmptyStatementSyntax node) {
            return null;
        }
    }
}