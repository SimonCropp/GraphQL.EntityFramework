using System;

namespace EfCoreGraphQL
{
    public class EfCoreGraphQLConfiguration
    {
        public EfCoreGraphQLConfiguration(Action<Type, object> registerInstance)
        {
            Scalars.RegisterInContainer(registerInstance);

        }
    }
}