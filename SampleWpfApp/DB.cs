using FreeSql;
using System;
using System.Diagnostics;
using System.Threading;

namespace SampleWpfApp
{
    public class DB
    {
        private DB()
        {
        }
        private static Lazy<IFreeSql> _db => new Lazy<IFreeSql>(() => new FreeSqlBuilder()
                    .UseAutoSyncStructure(true)
                    .UseMonitorCommand(cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText))
                    .UseConnectionString(DataType.Sqlite, "Data Source=|DataDirectory|\\SampleApp1.db;")
                    .Build());

        public static IFreeSql Instance => _db.Value;
    }
}
