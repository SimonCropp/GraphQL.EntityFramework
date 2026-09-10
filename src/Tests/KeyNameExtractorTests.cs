using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class KeyNameExtractorTests
{
    // Entities were skipped when their assembly name started with System, meant for the
    // Dictionary<string, object> join entity of a shadow many to many. A user assembly named
    // that way lost every key, so ids disappeared from the schema.
    [Fact]
    public void Entity_in_assembly_named_like_system_keeps_its_key()
    {
        var entityType = BuildEntityType("Systemic.Domain", "Entity");

        var builder = SqlServerConventionSetBuilder.CreateModelBuilder();
        builder.Entity(entityType);
        var model = builder.FinalizeModel();

        var keyNames = model.GetKeyNames();
        Assert.Equal(["Id"], keyNames[entityType]);
    }

    [Fact]
    public void Shadow_many_to_many_join_entity_is_skipped()
    {
        var builder = SqlServerConventionSetBuilder.CreateModelBuilder();
        builder.Entity<Left>()
            .HasMany(_ => _.Rights)
            .WithMany(_ => _.Lefts)
            .UsingEntity("LeftRight");
        var model = builder.FinalizeModel();

        var keyNames = model.GetKeyNames();
        Assert.Equal(["Id"], keyNames[typeof(Left)]);
        Assert.Equal(["Id"], keyNames[typeof(Right)]);
        Assert.DoesNotContain(typeof(Dictionary<string, object>), keyNames.Keys);
    }

    static Type BuildEntityType(string assemblyName, string typeName)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new(assemblyName), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule(assemblyName);
        var type = module.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class);
        var field = type.DefineField("id", typeof(Guid), FieldAttributes.Private);
        var property = type.DefineProperty("Id", PropertyAttributes.None, typeof(Guid), null);
        var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        var getter = type.DefineMethod("get_Id", attributes, typeof(Guid), Type.EmptyTypes);
        var getterIl = getter.GetILGenerator();
        getterIl.Emit(OpCodes.Ldarg_0);
        getterIl.Emit(OpCodes.Ldfld, field);
        getterIl.Emit(OpCodes.Ret);

        var setter = type.DefineMethod("set_Id", attributes, null, [typeof(Guid)]);
        var setterIl = setter.GetILGenerator();
        setterIl.Emit(OpCodes.Ldarg_0);
        setterIl.Emit(OpCodes.Ldarg_1);
        setterIl.Emit(OpCodes.Stfld, field);
        setterIl.Emit(OpCodes.Ret);

        property.SetGetMethod(getter);
        property.SetSetMethod(setter);
        return type.CreateType();
    }

    public class Left
    {
        public Guid Id { get; set; }
        public IList<Right> Rights { get; set; } = [];
    }

    public class Right
    {
        public Guid Id { get; set; }
        public IList<Left> Lefts { get; set; } = [];
    }
}
