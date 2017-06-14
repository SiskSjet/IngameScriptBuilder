using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IngameScriptBuilder.SyntaxRewriter {
    public class RemoveDefaultAccessModifier : CSharpSyntaxRewriter {
        private static SyntaxTokenList RemoveDefaultAccessModifier2(SyntaxTokenList modifiers) {
            var newModifiers = modifiers.Where(x => !x.IsKind(SyntaxKind.InternalKeyword) && !x.IsKind(SyntaxKind.PrivateKeyword));
            var syntaxModifiers = SyntaxTokenList.Create(new SyntaxToken());
            return syntaxModifiers.AddRange(newModifiers);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitConstructorDeclaration(node);
        }

        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitDelegateDeclaration(node);
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitEnumDeclaration(node);
        }

        public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitEventDeclaration(node);
        }

        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitEventFieldDeclaration(node);
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitInterfaceDeclaration(node);
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitMethodDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node) {
            node = node.WithModifiers(RemoveDefaultAccessModifier2(node.Modifiers));
            return base.VisitStructDeclaration(node);
        }
    }
}