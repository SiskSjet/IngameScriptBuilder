using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IngameScriptBuilder.SyntaxRewriter {
    public class RemoveRegionRewriter : CSharpSyntaxRewriter {
        public override SyntaxNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node) {
            return null;
        }

        public override SyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node) {
            return null;
        }
    }
}