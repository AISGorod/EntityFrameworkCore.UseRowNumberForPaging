using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace EntityFrameworkCore.UseRowNumberForPaging
{
    public class SqlServer2008QueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
    {
        private readonly QueryTranslationPostprocessorDependencies _dependencies;
        private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;
        public SqlServer2008QueryTranslationPostprocessorFactory(QueryTranslationPostprocessorDependencies dependencies, RelationalQueryTranslationPostprocessorDependencies relationalDependencies)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
        }

        public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
            => new SqlServer2008QueryTranslationPostprocessor(
                _dependencies,
                _relationalDependencies,
#if USE_EF_CORE_9
                (RelationalQueryCompilationContext)queryCompilationContext
#elif USE_EF_CORE_8
                queryCompilationContext
#endif
            );
        public class SqlServer2008QueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
        {
            public SqlServer2008QueryTranslationPostprocessor(
                QueryTranslationPostprocessorDependencies dependencies,
                RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
#if USE_EF_CORE_9
                RelationalQueryCompilationContext queryCompilationContext
#elif USE_EF_CORE_8
                QueryCompilationContext queryCompilationContext
#endif
            )
            : base(dependencies, relationalDependencies, queryCompilationContext)
            {}

            public override Expression Process(Expression query)
            {
                query = base.Process(query);
#if USE_EF_CORE_9
                query = new Offset2RowNumberConvertVisitor(query, RelationalDependencies.SqlExpressionFactory, RelationalQueryCompilationContext.SqlAliasManager).Visit(query);
#elif USE_EF_CORE_8
                query = new Offset2RowNumberConvertVisitor(query, RelationalDependencies.SqlExpressionFactory).Visit(query);
#endif
                return query;
            }
        }
    }
}