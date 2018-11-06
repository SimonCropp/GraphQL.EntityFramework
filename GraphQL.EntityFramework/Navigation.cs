using System;
using System.Diagnostics;

[DebuggerDisplay("PropertyName = {PropertyName}, PropertyType = {PropertyType}")]
class Navigation
{
    public Navigation(string propertyName, Type propertyType)
    {
        Guard.AgainstNullWhiteSpace(nameof(propertyName), propertyName);
        Guard.AgainstNull(nameof(propertyType), propertyType);
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public string PropertyName { get; }
    public Type PropertyType { get; }
}