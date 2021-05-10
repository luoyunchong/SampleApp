using System;
using Xunit;

namespace SampleAppTests
{
    public class UnitTest1
    {
        IFreeSql fsql;
        public UnitTest1(IFreeSql fsql)
        {
            this.fsql = fsql;
        }
        [Fact]
        public void Test1()
        {

        }
    }
}
