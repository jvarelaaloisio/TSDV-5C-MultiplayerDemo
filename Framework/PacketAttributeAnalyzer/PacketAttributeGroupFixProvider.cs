using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace PacketAttributeAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PacketAttributeGroupFixProvider)), Shared]
    public class PacketAttributeGroupFixProvider : CodeFixProvider
    {
        private const string Title = "Crop group length to be only the first {0} characters";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(PacketAttributeGroupAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var attributeArgument = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<AttributeArgumentSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: string.Format(Title, PacketAttributeGroupAnalyzer.GroupLength), 
                    createChangedDocument: c => FixGroupLength(context.Document, attributeArgument, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> FixGroupLength(Document document, AttributeArgumentSyntax attributeArgument, CancellationToken cancellationToken)
        {
            var oldLiteral = (LiteralExpressionSyntax)attributeArgument.Expression;
            var newLiteralValue = oldLiteral.Token.ValueText.Substring(0, PacketAttributeGroupAnalyzer.GroupLength); // Exclude the starting quote
            var newLiteral = oldLiteral.WithToken(SyntaxFactory.Literal("\"" + newLiteralValue + "\"", newLiteralValue));

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.ReplaceNode(oldLiteral, newLiteral);

            return editor.GetChangedDocument();
        }

    }
}