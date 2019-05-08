﻿using System;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService :
        IEfGraphQLService
    {
        GlobalFilters filters;

        internal IModel Model { get; }

        public EfGraphQLService(IModel model, GlobalFilters filters)
        {
            Guard.AgainstNull(nameof(model), model);
            Model = model;
            this.filters = filters;
            includeAppender = new IncludeAppender(NavigationReader.GetNavigationProperties(model));
        }

        IncludeAppender includeAppender;

        static Type MakeListGraphType(Type graphType)
        {
            var listGraphType = typeof(ListGraphType<>);
            return listGraphType.MakeGenericType(graphType);
        }
    }
}