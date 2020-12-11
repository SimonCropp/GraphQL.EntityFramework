﻿using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class GlobalFilterSnippets
{
    #region add-filter

    public class MyEntity
    {
        public string? Property { get; set; }
    }

    #endregion

    public void Add(ServiceCollection services)
    {
        #region add-filter

        var filters = new Filters();
        filters.Add<MyEntity>(
            (_, item) => item.Property != "Ignore");
        EfGraphQLConventions.RegisterInContainer<MyDbContext>(
            services,
            resolveFilters: _ => filters);

        #endregion
    }

    public class MyDbContext :
        DbContext
    {
    }
}