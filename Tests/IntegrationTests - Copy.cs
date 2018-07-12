using System;
using System.Threading.Tasks;
using EfCoreGraphQL;
using GraphQL;
using GraphQL.Types;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class WhereExpressionTests : TestBase
{
    [Fact]
    public async Task Foo()
    {
        var query = new Query();
            new TestEntity {Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Property = "Value"};

            var schema = new Schema(new FuncDependencyResolver(x =>
            {
                if (x == typeof(Query))
                {
                    return query;
                }

                if (x == typeof(TestEntityType))
                {
                    return new TestEntityType();
                }

                if (typeof(ScalarGraphType).IsAssignableFrom(x))
                {
                    return Scalar.Build(x);
                }

                throw new Exception($"Could not resolve {x.FullName}");
            }));

            var documentExecuter = new DocumentExecuter();

            var executionOptions = new ExecutionOptions
            {
                Schema = schema,
                Query = "{ testEntities { id property } }",
            };

            var result = await documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);
            ObjectApprover.VerifyWithJson(result.Data);
        }

    public WhereExpressionTests(ITestOutputHelper output) : base(output)
    {
    }
}

    public class Schema : GraphQL.Types.Schema
    {
        public Schema(IDependencyResolver resolver) :
            base(resolver)
        {
            Query = resolver.Resolve<Query>();
        }
    }

    public class TestEntity
    {
        public Guid Id { get; set; }
        public string Property { get; set; }
    }

    public class Query : ObjectGraphType
    {
        public Query()
        {
            Field<ListGraphType<TestEntityType>>(
                "testEntities",
                resolve: context =>
                {
                    var dataContext = resolveDataContext(context);
                    return dataContext.TestEntities.ToListAsync();
                });
        }
    }

    public class TestEntityType : ObjectGraphType<TestEntity>
    {
        public TestEntityType()
        {
            Field(x => x.Id);
            Field(x => x.Property);
        }
    }

}