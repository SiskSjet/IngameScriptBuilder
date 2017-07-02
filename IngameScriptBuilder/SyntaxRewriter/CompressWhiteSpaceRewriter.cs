using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IngameScriptBuilder.SyntaxRewriter {
    internal class CompressWhiteSpaceRewriter : CSharpSyntaxRewriter {
        private static void Compress(ref SyntaxToken token) {
            if (token.HasLeadingTrivia) {
                var trivia = token.LeadingTrivia;
                RemoveWhiteSpacesAndLineBreaks(ref trivia);
                token = token.WithLeadingTrivia(trivia);
            }
            if (token.HasTrailingTrivia) {
                var trivia = token.TrailingTrivia;
                RemoveWhiteSpacesAndLineBreaks(ref trivia);
                token = token.WithTrailingTrivia(trivia);
            }
        }

        private static void RemoveLineBreaks(ref SyntaxTriviaList trivia) {
            var old = trivia.Where(x => !x.IsKind(SyntaxKind.EndOfLineTrivia));
            var newTrivia = SyntaxTriviaList.Create(new SyntaxTrivia());

            trivia = newTrivia.AddRange(old);
        }

        private static void RemoveTrivia<T>(ref T node) where T : SyntaxNode {
            node = node.WithLeadingTrivia(SyntaxTriviaList.Empty);
            node = node.WithTrailingTrivia(SyntaxTriviaList.Empty);
        }

        private static void RemoveWhiteSpaces(ref SyntaxTriviaList trivia) {
            var old = trivia.Where(x => !x.IsKind(SyntaxKind.WhitespaceTrivia));
            var newTrivia = SyntaxTriviaList.Create(new SyntaxTrivia());

            trivia = newTrivia.AddRange(old);
        }

        private static void RemoveWhiteSpacesAndLineBreaks(ref SyntaxTriviaList trivia) {
            var hasWhiteSpaces = trivia.Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
            RemoveLineBreaks(ref trivia);
            RemoveWhiteSpaces(ref trivia);
            if (hasWhiteSpaces) {
                trivia = trivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "));
            }
        }

        public override SyntaxNode Visit(SyntaxNode node) {
            if (node is MemberDeclarationSyntax) {
                RemoveTrivia(ref node);
            } else if (node is StatementSyntax) {
                RemoveTrivia(ref node);
            }
            return base.Visit(node);
        }

        public override SyntaxToken VisitToken(SyntaxToken token) {
            var parent = token.Parent;
            switch (token.Kind()) {
                case SyntaxKind.SemicolonToken:
                case SyntaxKind.ColonToken:
                case SyntaxKind.DotToken:
                case SyntaxKind.CommaToken:
                case SyntaxKind.OpenBraceToken:
                case SyntaxKind.CloseBraceToken:
                case SyntaxKind.OpenBracketToken:
                case SyntaxKind.OpenParenToken:
                case SyntaxKind.CloseParenToken:
                case SyntaxKind.CloseBracketToken:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.GreaterThanEqualsToken:
                case SyntaxKind.GreaterThanGreaterThanToken:
                case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                case SyntaxKind.LessThanToken:
                case SyntaxKind.LessThanEqualsToken:
                case SyntaxKind.LessThanLessThanToken:
                case SyntaxKind.LessThanLessThanEqualsToken:
                case SyntaxKind.EqualsToken:
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.EqualsGreaterThanToken:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.IfKeyword:
                    token = token.WithLeadingTrivia(SyntaxTriviaList.Empty);
                    token = token.WithTrailingTrivia(SyntaxTriviaList.Empty);
                    break;
                case SyntaxKind.IdentifierToken:
                    if (parent is MemberDeclarationSyntax) {
                        token = token.WithTrailingTrivia(SyntaxTriviaList.Empty);
                    } else if (parent is VariableDeclaratorSyntax) {
                        token = token.WithTrailingTrivia(SyntaxTriviaList.Empty);
                    } else if (parent is ParameterSyntax) {
                        token = token.WithTrailingTrivia(SyntaxTriviaList.Empty);
                    } else if (parent is IdentifierNameSyntax && (parent.Parent is SimpleBaseTypeSyntax || parent.Parent is MemberAccessExpressionSyntax)) {
                        token = token.WithTrailingTrivia(SyntaxTriviaList.Empty);
                    }
                    break;
                default:
                    Compress(ref token);
                    break;
            }
            return base.VisitToken(token);
        }
    }
}