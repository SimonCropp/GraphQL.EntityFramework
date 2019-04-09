using System;
using System.Threading;

public static class SimpleSequentialGuid
{
    static int seed;

    public static Guid NewGuid()
    {
        var value = Interlocked.Increment(ref seed);
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}