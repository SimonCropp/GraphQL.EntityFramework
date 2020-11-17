using System;
using System.Threading.Tasks;
using GraphQL.Types;
using GraphQL.Types.Relay;

namespace GraphQL.Builders
{
    public static class PaginationBuilder
    {
        public static PaginationBuilder<TSourceType> Create<TNodeType, TSourceType>()
            where TNodeType : IGraphType
            => PaginationBuilder<TSourceType>.Create<TNodeType>();


        public static PaginationBuilder<TSourceType> Create<TNodeType, TPaginationType, TSourceType>()
            where TNodeType : IGraphType
            where TPaginationType : PaginationType<TNodeType>
            => PaginationBuilder<TSourceType>.Create<TNodeType, TPaginationType>();
    }

    public class PaginationBuilder<TSourceType>
    {
        private int? _row;
        private int? _page;

        public FieldType FieldType { get; protected set; }

        private PaginationBuilder(
            FieldType fieldType,
            int? page,
            int? row)
        {
            _page = page;
            _row = row;
            FieldType = fieldType;
            
            Argument<IntGraphType, int?>("page",
                "Get page");
            Argument<IntGraphType, int?>("row",
                "Specifies the number of row");


        }

        public static PaginationBuilder<TSourceType> Create<TNodeType>(string name = "default")
            where TNodeType : IGraphType
            => Create<TNodeType, PaginationType<TNodeType>>(name);

        public static PaginationBuilder<TSourceType> Create<TNodeType, TPaginationType>(string name = "default")
            where TNodeType : IGraphType
            where TPaginationType : PaginationType<TNodeType>
        {
            var fieldType = new FieldType
            {
                Name = name,
                Type = typeof(TPaginationType),
                Arguments = new QueryArguments(),
            };
            return new PaginationBuilder<TSourceType>(fieldType,  null, null);
        }

        public PaginationBuilder<TSourceType> Name(string name)
        {
            FieldType.Name = name;
            return this;
        }

        public PaginationBuilder<TSourceType> Description(string description)
        {
            FieldType.Description = description;
            return this;
        }

        public PaginationBuilder<TSourceType> DeprecationReason(string deprecationReason)
        {
            FieldType.DeprecationReason = deprecationReason;
            return this;
        }
        
        
        public PaginationBuilder<TSourceType> Page(int page)
        {
            _page = page;
            return this;
        }


        public PaginationBuilder<TSourceType> Row(int row)
        {
            _row = row;
            return this;
        }

        public PaginationBuilder<TSourceType> ReturnAll()
        {
            _row = null;
            return this;
        }

        public PaginationBuilder<TSourceType> Argument<TArgumentGraphType>(string name, string description)
            where TArgumentGraphType : IGraphType
        {
            FieldType.Arguments.Add(new QueryArgument(typeof(TArgumentGraphType))
            {
                Name = name,
                Description = description,
            });
            return this;
        }

        public PaginationBuilder<TSourceType> Argument<TArgumentGraphType, TArgumentType>(string name, string description,
            TArgumentType defaultValue = default)
            where TArgumentGraphType : IGraphType
        {
            FieldType.Arguments.Add(new QueryArgument(typeof(TArgumentGraphType))
            {
                Name = name,
                Description = description,
                DefaultValue = defaultValue,
            });
            return this;
        }

        public void Resolve(Func<IResolvePaginationContext<TSourceType>, object> resolver)
        {
            FieldType.Resolver = new Resolvers.FuncFieldResolver<object>(context =>
            {
                var args = new ResolvePaginationContext<TSourceType>(context, _page, _row);
                CheckForErrors(args);
                return resolver(args);
            });
        }

        public void ResolveAsync(Func<IResolvePaginationContext<TSourceType>, Task<object>> resolver)
        {
            FieldType.Resolver = new Resolvers.AsyncFieldResolver<object>(context =>
            {
                var args = new ResolvePaginationContext<TSourceType>(context, _page, _row);
                CheckForErrors(args);
                return resolver(args);
            });
        }

        private void CheckForErrors(IResolvePaginationContext<TSourceType> args)
        {
            if (args.Page < 1 )
            {
                throw new ArgumentException("page cannot less than 1");
            }
        }
    }
}