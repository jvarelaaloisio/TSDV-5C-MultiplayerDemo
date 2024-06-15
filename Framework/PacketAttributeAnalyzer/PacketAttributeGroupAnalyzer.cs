using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PacketAttributeAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PacketAttributeGroupAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AB0001";
    public const short GroupLength = 6;
    private const string Title = "PacketAttribute Group Length Validation";
    private const string MessageFormat = "The 'group' parameter must have a length of {0} characters.\n Current length is {1}";
    private const string Description = "Ensure PacketAttribute's group parameter is exactly 4 characters long.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Check if the attribute is PacketAttribute
        var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
        if (attributeSymbol?.ContainingType.Name != "PacketAttribute")
            return;

        // Find the 'group' argument
        var groupArgument = attributeSyntax.ArgumentList.Arguments
                                           .FirstOrDefault(arg => arg.NameColon?.Name.Identifier.Text == "group");

        if (groupArgument == null)
            return;

        // Check the length of the string literal
        var groupValue = context.SemanticModel.GetConstantValue(groupArgument.Expression);
        if (groupValue is { HasValue: true, Value: string groupString } && groupString.Length != GroupLength)
        {
            var rule = new DiagnosticDescriptor(Rule.Id, Rule.Title, string.Format(MessageFormat, GroupLength, groupString.Length), Rule.Category, Rule.DefaultSeverity, isEnabledByDefault: Rule.IsEnabledByDefault, description: Rule.Description);
            var diagnostic = Diagnostic.Create(rule, groupArgument.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}