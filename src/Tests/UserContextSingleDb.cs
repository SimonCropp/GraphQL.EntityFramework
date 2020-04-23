using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class UserContextSingleDb<TDbcontext> : Dictionary<string, object> where TDbcontext: DbContext
    {
        public UserContextSingleDb(TDbcontext context)
        {
            DbContext = context;
        }

        public readonly TDbcontext DbContext;
    }
}