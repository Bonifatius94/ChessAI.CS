using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Chess.UnitTest
{
    public abstract class TestBase
    {
        #region Init

        protected readonly ITestOutputHelper output;

        public TestBase(ITestOutputHelper output)
        {
            this.output = output;
        }

        #endregion Init
    }
}