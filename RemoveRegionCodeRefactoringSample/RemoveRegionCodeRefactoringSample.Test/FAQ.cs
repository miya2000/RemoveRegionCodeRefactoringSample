using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RemoveRegionCodeRefactoringSample.Test
{
    [TestClass]
    public class FAQ
    {
        //[FAQ(32)]
        [TestMethod]
        public void DeleteRegionsUsingRewriter()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(@"
using System;
#region Program
class Program
{
    #region Main
    static void Main()
    {
    }
    #endregion
}
#endregion
#region Other
class C
{
}
#endregion");
            SyntaxNode oldRoot = tree.GetRoot();

            var expected = @"
using System;
class Program
{
        static void Main()
    {
    }
    }
class C
{
}
";
            CSharpSyntaxRewriter rewriter = new RegionRemover1();
            SyntaxNode newRoot = rewriter.Visit(oldRoot);
            newRoot = Formatter.Format(newRoot, SyntaxAnnotation.ElasticAnnotation, new AdhocWorkspace());
            Assert.AreEqual(expected, newRoot.ToFullString());
        }

        // Below CSharpSyntaxRewriter removes all #regions and #endregions from under the SyntaxNode being visited.
        public class RegionRemover1 : CSharpSyntaxRewriter
        {
            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
            {
                SyntaxTrivia updatedTrivia = base.VisitTrivia(trivia);
                if (trivia.Kind() == SyntaxKind.RegionDirectiveTrivia ||
                    trivia.Kind() == SyntaxKind.EndRegionDirectiveTrivia)
                {
                    // Remove the trivia entirely by returning default(SyntaxTrivia).
                    updatedTrivia = default(SyntaxTrivia);
                }

                return updatedTrivia;
            }
        }

    }
}
