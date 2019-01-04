using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class AsyncEnumerable<T> :
    EnumerableQuery<T>,
    IAsyncEnumerable<T>,
    IQueryable<T>
{
    public AsyncEnumerable(IEnumerable<T> enumerable) :
        base(enumerable) { }

    public AsyncEnumerable(Expression expression) :
        base(expression) { }

    public IAsyncEnumerator<T> GetEnumerator()
    {
        return new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new AsyncQueryProvider<T>(this);
}