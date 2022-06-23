using System;
using System.Threading;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;

namespace FreeSql
{
    public class MultiFreeSql : MultiFreeSql<string>
    {
        public MultiFreeSql(TimeSpan timeSpan) : base(timeSpan)
        {
        }

        public MultiFreeSql(IdleBus<string, IFreeSql> idleBus) : base(idleBus)
        {
        }
    }
    public class MultiFreeSql<TDBKey> : BaseDbProvider, IFreeSql
    {
        internal TDBKey _dbkeyMaster;
        internal AsyncLocal<TDBKey> _dbkeyCurrent = new AsyncLocal<TDBKey>();
        BaseDbProvider _ormMaster => _ib.Get(_dbkeyMaster) as BaseDbProvider;
        BaseDbProvider _ormCurrent => _ib.Get(Equals(_dbkeyCurrent.Value, default(TDBKey)) ? _dbkeyMaster : _dbkeyCurrent.Value) as BaseDbProvider;
        internal IdleBus<TDBKey, IFreeSql> _ib;

        public MultiFreeSql(TimeSpan timeSpan)
        {
            _ib = new IdleBus<TDBKey, IFreeSql>(timeSpan);
            _ib.Notice += (_, __) => { };
        }

        public MultiFreeSql(IdleBus<TDBKey, IFreeSql> idleBus)
        {
            _ib = idleBus;
        }

        public override IAdo Ado => _ormCurrent.Ado;
        public override IAop Aop => _ormCurrent.Aop;
        public override ICodeFirst CodeFirst => _ormCurrent.CodeFirst;
        public override IDbFirst DbFirst => _ormCurrent.DbFirst;
        public override GlobalFilter GlobalFilter => _ormCurrent.GlobalFilter;
        public override void Dispose() => _ib.Dispose();

        public override CommonExpression InternalCommonExpression => _ormCurrent.InternalCommonExpression;
        public override CommonUtils InternalCommonUtils => _ormCurrent.InternalCommonUtils;

        public override ISelect<T1> CreateSelectProvider<T1>(object dywhere) => _ormCurrent.CreateSelectProvider<T1>(dywhere);
        public override IDelete<T1> CreateDeleteProvider<T1>(object dywhere) => _ormCurrent.CreateDeleteProvider<T1>(dywhere);
        public override IUpdate<T1> CreateUpdateProvider<T1>(object dywhere) => _ormCurrent.CreateUpdateProvider<T1>(dywhere);
        public override IInsert<T1> CreateInsertProvider<T1>() => _ormCurrent.CreateInsertProvider<T1>();
        public override IInsertOrUpdate<T1> CreateInsertOrUpdateProvider<T1>() => _ormCurrent.CreateInsertOrUpdateProvider<T1>();
    }
}
