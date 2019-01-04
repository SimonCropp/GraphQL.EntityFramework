using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AsyncEnumerator<T> :
    IAsyncEnumerator<T>
{
    IEnumerator<T> inner;

    public AsyncEnumerator(IEnumerator<T> inner)
    {
        this.inner = inner;
    }

    public void Dispose()
    {
        inner.Dispose();
    }

    public T Current => inner.Current;

    public Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        return Task.FromResult(inner.MoveNext());
    }
}