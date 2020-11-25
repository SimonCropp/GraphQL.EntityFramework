using System;
using System.ComponentModel.DataAnnotations;
using GraphQL.EntityFramework;
using Xunit;

public class NullableTypesTests
{
    [Fact]
    public void Class()
    {
        var nullProperty = typeof(TargetClass).GetProperty("NullProperty")!;
        Assert.True(nullProperty.IsNullable());

        var property = typeof(TargetClass).GetProperty("Property")!;
        Assert.False(property.IsNullable());
    }

    [Fact]
    public void Record()
    {
        var nullProperty = typeof(TargetRecord).GetProperty("NullProperty")!;
        Assert.True(nullProperty.IsNullable());

        var property = typeof(TargetRecord).GetProperty("Property")!;
        Assert.False(property.IsNullable());
    }

    public class TargetClass
    {
        public string Property { get; } = null!;
        public string? NullProperty { get; }
    }

    public record TargetRecord(string Property, string? NullProperty);

    //Ensure NullableTypes.IsNullable does not interpret nullable string in #nullable disable as non nullable one
#nullable disable

    [Fact]
    public void ClassNullableDisable()
    {
        var nullProperty = typeof(TargetClassNullableDisable).GetProperty("NullProperty")!;
        Assert.True(nullProperty.IsNullable());
    }

    [Fact]
    public void RecordNullableDisable()
    {
        var nullProperty = typeof(TargetRecordNullableDisable).GetProperty("NullProperty")!;
        Assert.True(nullProperty.IsNullable());
    }

    public class TargetClassNullableDisable
    {
        [Required]
        public string NullProperty { get; }
    }

    public record TargetRecordNullableDisable(string NullProperty);
}