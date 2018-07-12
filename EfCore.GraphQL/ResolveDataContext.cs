using GraphQL.Types;

namespace EfCoreGraphQL
{
    public delegate TDataContext ResolveDataContext<in T, out TDataContext>(T arg) where T : ResolveFieldContext;
    public delegate TDataContext ResolveDataContext<out TDataContext>(ResolveFieldContext<object> arg);
}