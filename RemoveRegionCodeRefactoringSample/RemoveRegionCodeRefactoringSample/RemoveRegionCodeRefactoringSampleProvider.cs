using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace RemoveRegionCodeRefactoringSample
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(RemoveRegionCodeRefactoringSampleProvider)), Shared]
    public class RemoveRegionCodeRefactoringSampleProvider : CodeRefactoringProvider
    {
        public const string ActionKeyRemoveRegion = nameof(RemoveRegionCodeRefactoringSampleProvider) + ".RemoveRegion";

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var trivia = root.FindTrivia(context.Span.Start);

            if (trivia.Kind() == SyntaxKind.RegionDirectiveTrivia)
            {
                var action = CodeAction.Create("Remove Region", c => RemoveRegionAsync(context.Document, trivia, c), ActionKeyRemoveRegion);
                context.RegisterRefactoring(action);
            }
        }

        private async Task<Document> RemoveRegionAsync(Document document, SyntaxTrivia region, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var remover = new SingleRegionRemover(region);

            var newRoot = remover.Visit(root);

            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }

    public class SingleRegionRemover : CSharpSyntaxRewriter
    {
        private SyntaxTrivia _region;
        private int _nests = -1;

        public SingleRegionRemover(SyntaxTrivia region)
        {
            _region = region;
        }

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.Kind() == SyntaxKind.RegionDirectiveTrivia)
            {
                if (trivia == _region)
                {
                    _nests = 0;
                     return SyntaxFactory.ElasticSpace;
                }
                else if (_nests >= 0)
                {
                    _nests++;
                }
            }
            else if (trivia.Kind() == SyntaxKind.EndRegionDirectiveTrivia)
            {
                if (_nests == 0)
                {
                    _nests = -1;
                    return SyntaxFactory.ElasticSpace;
                }
                else if (_nests >= 0)
                {
                    _nests--;
                }
            }

            return base.VisitTrivia(trivia);
        }
    }
}