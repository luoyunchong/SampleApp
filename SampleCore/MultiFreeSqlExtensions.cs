using System;

namespace FreeSql
{
    public static class MultiFreeSqlExtensions
    {
        private static IFreeSql ChangeDatabaseByKey<TDBKey>(this IFreeSql fsql, TDBKey dbkey)
        {
            var multiFsql = fsql as MultiFreeSql<TDBKey>;
            if (multiFsql == null) throw new Exception("fsql 类型不是 MultiFreeSql<TDBKey>");
            multiFsql._dbkeyCurrent.Value = dbkey;
            return multiFsql;
        }

        public static IDisposable Change<TDBKey>(this IFreeSql fsql, TDBKey dbkey)
        {
            var multiFsql = fsql as MultiFreeSql<TDBKey>;
            if (multiFsql == null) throw new Exception("fsql 类型不是 MultiFreeSql<TDBKey>");
            var olddbkey = multiFsql._dbkeyCurrent.Value;
            multiFsql.ChangeDatabaseByKey(dbkey);
            return new DBChangeDisposable(() => multiFsql.ChangeDatabaseByKey(olddbkey));
        }

        public static IFreeSql Register<TDBKey>(this IFreeSql fsql, TDBKey dbkey, Func<IFreeSql> create)
        {
            var multiFsql = fsql as MultiFreeSql<TDBKey>;
            if (multiFsql == null) throw new Exception("fsql 类型不是 MultiFreeSql<TDBKey>");
            if (multiFsql._ib.TryRegister(dbkey, create))
                if (multiFsql._ib.GetKeys().Length == 1)
                    multiFsql._dbkeyMaster = dbkey;
            return multiFsql;
        }
    }

    class DBChangeDisposable : IDisposable
    {
        Action _cancel;
        public DBChangeDisposable(Action cancel) => _cancel = cancel;
        public void Dispose() => _cancel?.Invoke();
    }
}
