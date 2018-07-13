using System;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class EfCoreGraphQLConventions
    {
        public static void RegisterInContainer(Action<Type, GraphType> registerInstance)
        {
            Scalars.RegisterInContainer(registerInstance);
            ArgumentGraphs.RegisterInContainer(registerInstance);
        }
    }
}