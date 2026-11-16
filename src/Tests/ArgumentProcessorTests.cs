public class ArgumentProcessorTests
{
    // The in memory path assumed the key is named Id, so an entity keyed by anything else failed
    // on the ids argument, while the queryable path resolved the key from the model.
    [Test]
    public async Task Ids_use_key_name_from_model()
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

        await Assert.That(result.Select(_ => _.Property))
            .IsEquivalentTo((string?[]) ["one"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Ids_default_to_Id_for_the_hasId_overload()
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

        await Assert.That(result.Select(_ => _.Property))
            .IsEquivalentTo((string?[]) ["two"], CollectionOrdering.Matching);
    }

    // A null ids was dereferenced for its type name in the unsupported type error
    [Test]
    public async Task Null_ids_is_the_same_as_no_ids()
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

        await Assert.That(result.Select(_ => _.Property))
            .IsEquivalentTo((string?[]) ["one", "two"], CollectionOrdering.Matching);
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
