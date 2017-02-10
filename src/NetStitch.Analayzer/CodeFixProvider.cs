using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;

namespace NetStitch.Analayzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NetStitchCodeFixProvider)), Shared]
    public class NetStitchCodeFixProvider : CodeFixProvider
    {
        private const string title = "NetStitch CodeFix";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(NetStitchAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) as CompilationUnitSyntax;
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var targetNode = root.FindNode(context.Span);
            var method = targetNode as MethodDeclarationSyntax;
            if (method == null) return;

            var targetInterface = method.Parent as InterfaceDeclarationSyntax;
            if (targetInterface == null) return;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => FixSharedInterfaceAsync(context.Document, targetInterface, model, c),
                    equivalenceKey: title),
                context.Diagnostics.First());
        }

        private async Task<Document> FixSharedInterfaceAsync(Document document, InterfaceDeclarationSyntax interfaceType, SemanticModel model, CancellationToken cancellationToken)
        {

            var editor = await DocumentEditor.CreateAsync(document);

            var seq = interfaceType.ChildNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(x => new
            {
                targetMethod = x,
                methodSymbol = model.GetDeclaredSymbol(x),
                returnType = model.GetDeclaredSymbol(x)?.ReturnType as INamedTypeSymbol
            })
            .Where(x => {
                return !x.methodSymbol.GetAttributes().Any(attr => attr.AttributeClass.Name == "OperationAttribute");
            })
            .Where(x => x.returnType != null)
            .Select(x =>
            {
                if (x.returnType.ToDisplayString() == "void")
                    return new { x.targetMethod, newMethod = FixActionMethod(x.targetMethod, cancellationToken) };

                if (x.returnType.ToDisplayString() == "System.Threading.Tasks.Task")
                    return new { x.targetMethod, newMethod = FixAsyncFunctionMethod(x.targetMethod, cancellationToken) };

                if (x.returnType.IsGenericType)
                {
                    var genericType = x.returnType.ConstructUnboundGenericType();
                    if (genericType.ToDisplayString() == "System.Threading.Tasks.Task<>")
                        return new { x.targetMethod, newMethod = FixAsyncFunctionMethod(x.targetMethod, cancellationToken) };
                }

                return new { x.targetMethod, newMethod = FixFunctionMethod(x.targetMethod, cancellationToken) };

            });

            foreach (var item in seq)
            {
                editor.ReplaceNode(item.targetMethod, item.newMethod);
            }

            return editor.GetChangedDocument();

        }

        private MethodDeclarationSyntax FixActionMethod(MethodDeclarationSyntax targetMethod, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax updatedmethod = targetMethod;

            var name = SyntaxFactory.ParseName("NetStitch.Operation");
            var arguments = SyntaxFactory.ParseAttributeArgumentList($"(\"{Guid.NewGuid().ToString()}\")");
            var attribute = SyntaxFactory.Attribute(name, arguments);

            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            attributeList = attributeList.Add(attribute);
            var list = SyntaxFactory.AttributeList(attributeList);

            updatedmethod = updatedmethod.AddAttributeLists(list);

            /*
             * 
             * void Replace to
             * 
             * #if !___server___
             *     Task
             * #else
             *     void
             * #endif
             * 
             */
            updatedmethod = updatedmethod.ReplaceNode(
                updatedmethod.ReturnType,
                SyntaxFactory.IdentifierName(
                    SyntaxFactory.Identifier("Task")
                )
                .WithLeadingTrivia(
                    SyntaxFactory.Whitespace(" "),
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n")
                )
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(SyntaxFactory.ElseDirectiveTrivia(false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    "),
                    SyntaxFactory.DisabledText("void"),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(SyntaxFactory.EndIfDirectiveTrivia(false).WithTrailingTrivia(SyntaxFactory.EndOfLine("\r\n")))
                ));

            /*
             *
             * MethodName Replace to
             * 
             * #if !___server___
             *     MethodNameAsync
             * #else
             *     MethodName
             * #endif
             * 
             */
            updatedmethod = updatedmethod.ReplaceToken(
                updatedmethod.Identifier,
                (
                   updatedmethod.Identifier.Text.IsAsyncSuffixTarget() ?
                       SyntaxFactory.Identifier(updatedmethod.Identifier + "Async") :
                       updatedmethod.Identifier
                )
                .WithLeadingTrivia(
                    SyntaxFactory.Whitespace(" "),
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    ")
                )
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(SyntaxFactory.ElseDirectiveTrivia(false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    "),
                    SyntaxFactory.DisabledText(updatedmethod.Identifier.Text),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(
                        SyntaxFactory.EndIfDirectiveTrivia(false)
                        .WithTrailingTrivia(
                            SyntaxFactory.EndOfLine("\r\n"),
                            SyntaxFactory.Whitespace("    " + "    ")
                        ))
                ));

            if (updatedmethod.ParameterList.Parameters.Count == 0)
            {
                /*
                 * #if !___server___
                 *      System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
                 * #endif
                 */
                updatedmethod = updatedmethod.AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("cancellationToken")
                    .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    .WithTrailingTrivia(SyntaxFactory.Whitespace(" "))
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithType(
                    SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName))
                    .WithLeadingTrivia(SyntaxFactory.Whitespace("    " + "    "))
                .WithDefault(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.DefaultExpression(
                            SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName)
                        )
                        .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    )
                )
                .WithLeadingTrivia(
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    ")
                )
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(
                        SyntaxFactory.EndIfDirectiveTrivia(false)
                        .WithTrailingTrivia(
                            SyntaxFactory.EndOfLine("\r\n"),
                            SyntaxFactory.Whitespace("    " + "    ")
                        ))
                ));

            }
            else
            {
                /*
                 * #if !___server___
                 *     , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
                 * #endif
                 */
                updatedmethod = updatedmethod.AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("cancellationToken")
                    .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    .WithTrailingTrivia(SyntaxFactory.Whitespace(" "))
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithType(
                    SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName))
                    .WithLeadingTrivia(SyntaxFactory.Whitespace("    " + "    "))
                .WithDefault(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.DefaultExpression(
                            SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName)
                        )
                        .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    )
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(
                        SyntaxFactory.EndIfDirectiveTrivia(false)
                        .WithTrailingTrivia(
                            SyntaxFactory.EndOfLine("\r\n"),
                            SyntaxFactory.Whitespace("    " + "    ")
                        ))
                ));

                var commaToken = updatedmethod.ParameterList.ChildTokens().Where(x => x.IsKind(SyntaxKind.CommaToken)).Last();

                var newToken = commaToken.WithLeadingTrivia(
                    SyntaxFactory.Whitespace("\r\n"),
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    ")
                    );

                var newParameterList = updatedmethod.ParameterList.ReplaceToken(commaToken, newToken);

                updatedmethod = updatedmethod.ReplaceNode(updatedmethod.ParameterList, newParameterList);
            }

            return updatedmethod;
        }

        private MethodDeclarationSyntax FixAsyncFunctionMethod(MethodDeclarationSyntax targetMethod, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax updatedmethod = targetMethod;

            var name = SyntaxFactory.ParseName("NetStitch.Operation");
            var arguments = SyntaxFactory.ParseAttributeArgumentList($"(\"{Guid.NewGuid().ToString()}\")");
            var attribute = SyntaxFactory.Attribute(name, arguments);

            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            attributeList = attributeList.Add(attribute);
            var list = SyntaxFactory.AttributeList(attributeList);

            updatedmethod = updatedmethod.AddAttributeLists(list);

            if (updatedmethod.Identifier.Text.IsAsyncSuffixTarget())
                updatedmethod = updatedmethod.ReplaceToken(updatedmethod.Identifier, SyntaxFactory.Identifier(updatedmethod.Identifier + "Async"));

            if (updatedmethod.ParameterList.Parameters.Count == 0)
            {
                /*
                 * #if !___server___
                 *      System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
                 * #endif
                 */
                updatedmethod = updatedmethod.AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("cancellationToken")
                    .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    .WithTrailingTrivia(SyntaxFactory.Whitespace(" "))
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithType(
                    SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName))
                    .WithLeadingTrivia(SyntaxFactory.Whitespace("    " + "    "))
                .WithDefault(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.DefaultExpression(
                            SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName)
                        )
                        .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    )
                )
                .WithLeadingTrivia(
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    ")
                )
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(
                        SyntaxFactory.EndIfDirectiveTrivia(false)
                        .WithTrailingTrivia(
                            SyntaxFactory.EndOfLine("\r\n"),
                            SyntaxFactory.Whitespace("    " + "    ")
                        ))
                ));

            }
            else
            {
                /*
                 * #if !___server___
                 *     , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
                 * #endif
                 */
                updatedmethod = updatedmethod.AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("cancellationToken")
                    .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    .WithTrailingTrivia(SyntaxFactory.Whitespace(" "))
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithType(
                    SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName))
                    .WithLeadingTrivia(SyntaxFactory.Whitespace("    " + "    "))
                .WithDefault(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.DefaultExpression(
                            SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName)
                        )
                        .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    )
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(
                        SyntaxFactory.EndIfDirectiveTrivia(false)
                        .WithTrailingTrivia(
                            SyntaxFactory.EndOfLine("\r\n"),
                            SyntaxFactory.Whitespace("    " + "    ")
                        ))
                ));

                var commaToken = updatedmethod.ParameterList.ChildTokens().Where(x => x.IsKind(SyntaxKind.CommaToken)).Last();

                var newToken = commaToken.WithLeadingTrivia(
                    SyntaxFactory.Whitespace("\r\n"),
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    ")
                    );

                var newParameterList = updatedmethod.ParameterList.ReplaceToken(commaToken, newToken);

                updatedmethod = updatedmethod.ReplaceNode(updatedmethod.ParameterList, newParameterList);

            }

            return updatedmethod;

        }

        private MethodDeclarationSyntax FixFunctionMethod(MethodDeclarationSyntax targetMethod, CancellationToken cancellationToken)
        {

            MethodDeclarationSyntax updatedmethod = targetMethod;

            var name = SyntaxFactory.ParseName("NetStitch.Operation");
            var arguments = SyntaxFactory.ParseAttributeArgumentList($"(\"{Guid.NewGuid().ToString()}\")");
            var attribute = SyntaxFactory.Attribute(name, arguments);

            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            attributeList = attributeList.Add(attribute);
            var list = SyntaxFactory.AttributeList(attributeList);

            updatedmethod = updatedmethod.AddAttributeLists(list);

            /*
             * 
             * int Replace to
             * 
             * #if !___server___
             *     Task<
             * #endif
             *     int
             * #if !___server___
             *     > 
             * #endif
             * 
             */
            updatedmethod = updatedmethod.ReplaceNode(
                updatedmethod.ReturnType,
                SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier("Task"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.Token(SyntaxKind.LessThanToken), //<
                        SyntaxFactory.SeparatedList(new[] {
                            targetMethod.ReturnType
                            .WithLeadingTrivia(
                                SyntaxFactory.Trivia(SyntaxFactory.EndIfDirectiveTrivia(false)),
                                SyntaxFactory.EndOfLine("\r\n"),
                                SyntaxFactory.Whitespace("    " + "    ")
                            )
                        }),
                        SyntaxFactory.Token(SyntaxKind.GreaterThanToken)
                            .WithLeadingTrivia(
                                //SyntaxFactory.EndOfLine("\r\n"),
                                SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                                SyntaxFactory.EndOfLine("\r\n"),
                                SyntaxFactory.Whitespace("    " + "    ")
                            )
                            .WithTrailingTrivia(
                                SyntaxFactory.EndOfLine("\r\n"),
                                SyntaxFactory.Trivia(SyntaxFactory.EndIfDirectiveTrivia(false).WithTrailingTrivia(SyntaxFactory.EndOfLine("\r\n")))
                            )
                    )
                )
                .WithLeadingTrivia(
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n")
                ));

            /*
             *
             * MethodName Replace to
             * 
             * #if !___server___
             *     MethodNameAsync
             * #else
             *     MethodName
             * #endif
             * 
             */
            updatedmethod = updatedmethod.ReplaceToken(
                updatedmethod.Identifier,
                (
                   updatedmethod.Identifier.Text.IsAsyncSuffixTarget() ?
                       SyntaxFactory.Identifier(updatedmethod.Identifier + "Async") :
                       updatedmethod.Identifier
                )
                .WithLeadingTrivia(
                    SyntaxFactory.Whitespace(" "),
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    ")
                )
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(SyntaxFactory.ElseDirectiveTrivia(false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    "),
                    SyntaxFactory.DisabledText(updatedmethod.Identifier.Text),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(
                        SyntaxFactory.EndIfDirectiveTrivia(false)
                        .WithTrailingTrivia(
                            SyntaxFactory.EndOfLine("\r\n"),
                            SyntaxFactory.Whitespace("    " + "    ")
                        ))
                ));

            if (updatedmethod.ParameterList.Parameters.Count == 0)
            {
                /*
                 * #if !___server___
                 *      System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
                 * #endif
                 */
                updatedmethod = updatedmethod.AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("cancellationToken")
                    .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    .WithTrailingTrivia(SyntaxFactory.Whitespace(" "))
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithType(
                    SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName))
                    .WithLeadingTrivia(SyntaxFactory.Whitespace("    " + "    "))
                .WithDefault(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.DefaultExpression(
                            SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName)
                        )
                        .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    )
                )
                .WithLeadingTrivia(
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    ")
                )
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(
                        SyntaxFactory.EndIfDirectiveTrivia(false)
                        .WithTrailingTrivia(
                            SyntaxFactory.EndOfLine("\r\n"),
                            SyntaxFactory.Whitespace("    " + "    ")
                        ))
                ));

            }
            else
            {
                /*
                 * #if !___server___
                 *     , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
                 * #endif
                 */
                updatedmethod = updatedmethod.AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("cancellationToken")
                    .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    .WithTrailingTrivia(SyntaxFactory.Whitespace(" "))
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithType(
                    SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName))
                    .WithLeadingTrivia(SyntaxFactory.Whitespace("    " + "    "))
                .WithDefault(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.DefaultExpression(
                            SyntaxFactory.ParseTypeName(typeof(CancellationToken).FullName)
                        )
                        .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                    )
                )
                .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                .WithTrailingTrivia(
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Trivia(
                        SyntaxFactory.EndIfDirectiveTrivia(false)
                        .WithTrailingTrivia(
                            SyntaxFactory.EndOfLine("\r\n"),
                            SyntaxFactory.Whitespace("    " + "    ")
                        ))
                ));

                var commaToken = updatedmethod.ParameterList.ChildTokens().Where(x => x.IsKind(SyntaxKind.CommaToken)).Last();

                var newToken = commaToken.WithLeadingTrivia(
                    SyntaxFactory.Whitespace("\r\n"),
                    SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName("!___server___"), false, false, false)),
                    SyntaxFactory.EndOfLine("\r\n"),
                    SyntaxFactory.Whitespace("    " + "    ")
                    );

                var newParameterList = updatedmethod.ParameterList.ReplaceToken(commaToken, newToken);

                updatedmethod = updatedmethod.ReplaceNode(updatedmethod.ParameterList, newParameterList);
            }

            return updatedmethod;

        }
    }
    internal static class StringEx
    {
        internal static bool IsAsyncSuffixTarget(this string methodName) => methodName.Length < 5 || methodName.Substring(methodName.Length - 5, 5) != "Async";
    }
}