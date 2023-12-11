using Humanizer;
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
            fsql.CodeFirst.SyncStructure<test>();
            string sql = " select id, name from test";
            var d=fsql.Select<testdto>().WithSql(sql).ToList(p => new testdto { id = p.id, name = p.name });
        }
    }
    public class testdto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string remark { get; set; }
    }
    public class test
    {
        public int id { get; set; }
        public string name { get; set; }
        public string remark { get; set; }
    }
}
