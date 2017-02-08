using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetStitch.Analayzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NetStitchAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NetStitchAnalayzer";
        private const string Category = "Usage";

        internal static readonly DiagnosticDescriptor InterfaceMustPublicAccesibility = new DiagnosticDescriptor(
            id: DiagnosticId + "_" + nameof(InterfaceMustPublicAccesibility),
            title: "Interface must public",
            category: Category,
            messageFormat: "Interface must public {0}", 
            description: "Interface must public",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static DiagnosticDescriptor AddOperationAttributeToMethod = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add Operation Attribute",
            category: Category, 
            messageFormat: "Add Operation Attribute To Method",
            description: "Add Operation Attribute To Method",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
            get
            {
                return ImmutableArray.Create(
                    AddOperationAttributeToMethod,
                    InterfaceMustPublicAccesibility
                    );
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InterfaceDeclaration, SyntaxKind.MethodDeclaration);
        }

        static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var model = context.SemanticModel;

            var interfaceSyntax = context.Node as InterfaceDeclarationSyntax;
            if (interfaceSyntax == null) return;

            var interfaceSymbol = model.GetDeclaredSymbol(interfaceSyntax);
            if (interfaceSymbol == null) return;
            if (!interfaceSymbol.GetAttributes().Any(x => x.AttributeClass.Name == "NetStitchContractAttribute"))
                return;

            if (interfaceSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                var diagnostic = Diagnostic.Create(InterfaceMustPublicAccesibility, interfaceSymbol.Locations[0], interfaceSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            var array = interfaceSyntax.ChildNodes().OfType<MethodDeclarationSyntax>().ToArray();

            foreach (var item in array)
            {
                var declaredSymbol = model.GetDeclaredSymbol(item);
                if (declaredSymbol == null) return;
              
                if (!declaredSymbol.GetAttributes().Any(x => x.AttributeClass.Name == "OperationAttribute"))
                {
                    //OperationAttribute
                    var diagnostic = Diagnostic.Create(AddOperationAttributeToMethod, declaredSymbol.Locations[0], declaredSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
