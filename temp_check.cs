using System;
using System.Reflection;
using ModelContextProtocol;

var type = typeof(McpException);
Console.WriteLine($"Type: {type.FullName}");
Console.WriteLine($"Base: {type.BaseType?.FullName}");
foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
{
    var parms = string.Join(", ", Array.ConvertAll(ctor.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"));
    Console.WriteLine($"  ctor({parms})");
}
