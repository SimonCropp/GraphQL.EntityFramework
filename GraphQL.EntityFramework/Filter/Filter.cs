using System.Threading;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework
{
    public delegate bool Filter<in TReturn>(TReturn input);
    public delegate Task<Filter<TReturn>> FilterBuilder<TReturn>(object userContext, CancellationToken token);
}