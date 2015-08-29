using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace RemoveRegionCodeRefactoringSample.Test
{
    [TestClass]
    public class RemoveRegionCodeRefactoringSampleTest
    {
        CodeRefactoringVerifier Verifier { get; set; }
        CodeRefactoringVerifier NewVerifier() => new CodeRefactoringVerifier(new RemoveRegionCodeRefactoringSampleProvider());

        [TestInitialize]
        public void Initialize()
        {
            Verifier = NewVerifier();
        }
        [TestCleanup]
        public void Cleanup()
        {
        }

        #region RemoveReginn01
        [TestMethod]
        public void RemoveReginn01()
        {
            var test = @"
using System;

namespace ConsoleApplication1
{
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
    #endregion
}";

            var expected = @"
using System;

namespace ConsoleApplication1
{
    #region Program
    class Program
    {
        static void Main()
        {
        }
    }
    #endregion
    #region Other
    class C
    {
    }
    #endregion
}";

            var actual = Verifier.GetRefactoringResult(test, test.IndexOf("#region Main"));
            actual.Is(expected);
        }
        #endregion
        #region RemoveReginn02
        [TestMethod]
        public void RemoveReginn02()
        {
            var test = @"
using System;

#region ConsoleApplication1
namespace ConsoleApplication1
{
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
    #endregion
}
#endregion
";

            var expected = @"
using System;

#region ConsoleApplication1
namespace ConsoleApplication1
{
    class Program
    {
        #region Main
        static void Main()
        {
        }
        #endregion
    }
    #region Other
    class C
    {
    }
    #endregion
}
#endregion
";

            var actual = Verifier.GetRefactoringResult(test, test.IndexOf("#region Program"));
            actual.Is(expected);
        }
        #endregion

    }
}
