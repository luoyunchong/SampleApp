using System;
using FreeSql.DataAnnotations;
using Xunit;

namespace SampleAppTests
{
    public class FreesqlTest
    {
        [Column(IsPrimary = true)]
        public Guid FreesqlTestID { get; set; }

        public string Name { get; set; }

        public FreesqlTestDetail FreesqlTestDetail { get; set; }
    }

    public class FreesqlTestDetail
    {
        [Column(IsPrimary = true)]
        public Guid DetailId { get; set; }

        public string kecheng33 { get; set; }
        public FreesqlTest FreesqlTest { get; set; }

    }
    public class FsqlTest
    {
        IFreeSql fsql;
        public FsqlTest(IFreeSql fsql)
        {
            this.fsql = fsql;
        }
        [Fact]
        public void Test1()
        {
            fsql.Insert<FreesqlTest>(new FreesqlTest()
            {
                Name = "fff"
            }).ExecuteAffrows();
        }

        [Fact]
        public void FreesqlTestDetail()
        {
            fsql.Insert<FreesqlTestDetail>(new FreesqlTestDetail() { kecheng33 = "fff" }).ExecuteAffrows();
        }

        [Fact]
        public void repository_add()
        {
            var repoA = fsql.GetGuidRepository<FreesqlTest>();
            var repoB = fsql.GetGuidRepository<FreesqlTestDetail>();

            var a = new FreesqlTest()
            {
                Name = "fff"
            };
            FreesqlTest ff = repoA.Insert(a);
            var b = new FreesqlTestDetail() { DetailId = a.FreesqlTestID, kecheng33 = "fff" };
            repoB.Insert(b);
        }

        [Fact]
        public void repository_add_unitofwork()
        {
            using (var uow = fsql.CreateUnitOfWork())
            {
                var repoA = fsql.GetRepository<FreesqlTest>();
                var repoB = fsql.GetRepository<FreesqlTestDetail>();

                var a = new FreesqlTest()
                {
                    Name = "fff"
                };
                FreesqlTest ff = repoA.Insert(a);
                var b = new FreesqlTestDetail() { DetailId = a.FreesqlTestID, kecheng33 = "fff" };
                repoB.Insert(b);

                uow.Commit();
            }
        }
    }
}
