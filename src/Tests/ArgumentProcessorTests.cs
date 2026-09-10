using GraphQL.Execution;

public class ArgumentProcessorTests
{
    // The in memory path assumed the key is named Id, so an entity keyed by anything else failed
    // on the ids argument, while the queryable path resolved the key from the model.
    [Fact]
    public void Ids_use_key_name_from_model()
    {
        var entity1 = new NamedIdEntity
        {
            Property = "one"
        };
        var entity2 = new NamedIdEntity
        {
            Property = "two"
        };
        var context = BuildContext(entity1.NamedId);

        var result = new List<NamedIdEntity>
            {
                entity1,
                entity2
            }
            .ApplyGraphQlArguments(["NamedId"], context, false)
            .ToList();

        Assert.Equal(["one"], result.Select(_ => _.Property));
    }

    [Fact]
    public void Ids_default_to_Id_for_the_hasId_overload()
    {
        var entity1 = new ParentEntity
        {
            Property = "one"
        };
        var entity2 = new ParentEntity
        {
            Property = "two"
        };
        var context = BuildContext(entity2.Id);

        var result = new List<ParentEntity>
            {
                entity1,
                entity2
            }
            .ApplyGraphQlArguments(true, context, false)
            .ToList();

        Assert.Equal(["two"], result.Select(_ => _.Property));
    }

    // A null ids was dereferenced for its type name in the unsupported type error
    [Fact]
    public void Null_ids_is_the_same_as_no_ids()
    {
        var context = BuildContext(null);

        var result = new List<ParentEntity>
            {
                new()
                {
                    Property = "one"
                },
                new()
                {
                    Property = "two"
                }
            }
            .ApplyGraphQlArguments(true, context, false)
            .ToList();

        Assert.Equal(["one", "two"], result.Select(_ => _.Property));
    }

    static ResolveFieldContext BuildContext(Guid? id) =>
        new()
        {
            Arguments = new Dictionary<string, ArgumentValue>
            {
                ["ids"] = new(
                    id is null
                        ? null
                        : new object[]
                        {
                            id.ToString()!
                        },
                    ArgumentSource.Literal)
            },
            FieldDefinition = new()
            {
                Name = "entities"
            },
            Errors = []
        };
}
