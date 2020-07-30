//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Reactive.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using GraphQL.EntityFramework;
//using GraphQL.Execution;
//using GraphQL.Language.AST;
//using GraphQL.Resolvers;
//using GraphQL.Subscription;
//using GraphQL;
//using GraphQL.Types;
//using Microsoft.EntityFrameworkCore;
//using ExecutionContext = GraphQL.Execution.ExecutionContext;

//public class Subscription :
//    ObjectGraphType<object>
//{
//    public Subscription(Func<SampleDbContext> contextFactory)
//    {
//        AddField(new EventStreamFieldType
//        {
//            Name = "companyChanged",
//            Type = typeof(CompanyGraph),
//            Resolver = new FuncFieldResolver<Company>(context => (Company)context.Source),
//            Subscriber = new EventStreamResolver<Company>(context => Subscribe(context, contextFactory))
//        });
//    }

//    static IObservable<Company> Subscribe(IResolveEventStreamContext context, Func<SampleDbContext> contextFactory)
//    {
//        long lastId = 0;
//        var inner = Observable.Using(
//            token => Task.FromResult(contextFactory()),
//            async (dbContext, token) =>
//            {
//                try
//                {
//                    var companies = await GetCompanies(context, dbContext, lastId, token: token);

//                    if (companies.Any())
//                    {
//                        lastId = companies.Max(transaction => transaction.Id);
//                    }

//                    return companies.ToObservable();
//                }
//                catch (OperationCanceledException)
//                {
//                    Trace.Write("Companies subscription has been cancelled.");
//                    return Observable.Empty<Company>();
//                }
//            });

//        return Observable.Interval(TimeSpan.FromSeconds(1)).SelectMany(_ => inner);
//    }

//    static Task<List<Company>> GetCompanies(
//        IResolveEventStreamContext context,
//        SampleDbContext dbContext,
//        long lastId,
//        int take = 1,
//        CancellationToken token = default)
//    {
//        var returnType = dbContext.Companies;

//        var document = new GraphQLDocumentBuilder().Build(SupposePersistedQuery());

//        var fieldContext = ResolveFieldContext(dbContext, token, document, context.Schema);

//        var withArguments = returnType.ApplyGraphQlArguments(fieldContext);

//        var greaterThanLastIdAndPaged = withArguments
//            .Where(transaction => transaction.Id > lastId)
//            .Take(take);

//        return greaterThanLastIdAndPaged.ToListAsync(token);
//    }

//    static string SupposePersistedQuery()
//    {
//        return @"{
//            companies
//            {
//                id
//            }
//        }";
//    }

//    static IResolveFieldContext ResolveFieldContext(
//        SampleDbContext dbContext,
//        CancellationToken token,
//        Document document,
//        ISchema schema)
//    {
//        var operation = document.Operations.FirstOrDefault();
//        var variableValues = ExecutionHelper.GetVariableValues(document, schema, operation?.Variables, null);
//        var executionContext = new ExecutionContext
//        {
//            Document = document,
//            Schema = schema,
//            UserContext = new UserContext(dbContext),
//            Variables = variableValues,
//            Fragments = document.Fragments,
//            CancellationToken = token,
//            Listeners = new List<IDocumentExecutionListener>(),
//            Operation = operation,
//            ThrowOnUnhandledException = true
//        };

//        var operationRootType = ExecutionHelper.GetOperationRootType(
//            executionContext.Document,
//            executionContext.Schema,
//            executionContext.Operation);

//        var node = ExecutionStrategy.BuildExecutionRootNode(executionContext, operationRootType);

//        return GetContext(executionContext, node.SubFields["companies"]);
//    }

//    static ResolveFieldContext GetContext(ExecutionContext context, ExecutionNode node)
//    {
//        var argumentValues = ExecutionHelper.GetArgumentValues(
//            context.Schema,
//            node.FieldDefinition.Arguments,
//            node.Field.Arguments,
//            context.Variables);
//        var dictionary = ExecutionHelper.SubFieldsFor(context, node.FieldDefinition.ResolvedType, node.Field);
//        return new ResolveFieldContext
//        {
//            FieldName = node.Field.Name,
//            FieldAst = node.Field,
//            FieldDefinition = node.FieldDefinition,
//            ReturnType = node.FieldDefinition.ResolvedType,
//            ParentType = node.GetParentType(context.Schema),
//            Arguments = argumentValues,
//            Source = node.Source,
//            Schema = context.Schema,
//            Document = context.Document,
//            Fragments = context.Fragments,
//            RootValue = context.RootValue,
//            UserContext = context.UserContext,
//            Operation = context.Operation,
//            Variables = context.Variables,
//            CancellationToken = context.CancellationToken,
//            Metrics = context.Metrics,
//            Errors = context.Errors,
//            Path = node.Path,
//            SubFields = dictionary
//        };
//    }
//}