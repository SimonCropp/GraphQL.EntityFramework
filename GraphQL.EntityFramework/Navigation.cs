using System;
using System.Diagnostics;

[DebuggerDisplay("PropertyName = {PropertyName}, PropertyType = {PropertyType}")]
class Navigation
{
    public string PropertyName;
    public Type PropertyType;
}