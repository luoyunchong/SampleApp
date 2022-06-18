using System;

namespace FreeSql;

public class StaticDB : StaticDB<string> { }

public abstract class StaticDB<DBKey>
{
    protected static Lazy<IFreeSql> multiFreeSql = new Lazy<IFreeSql>(() => new MultiFreeSql());
    public static IFreeSql Instance => multiFreeSql.Value;
}