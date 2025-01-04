using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<Environment>))]
public class Environment : Enumeration
{
  public static Environment Development = new() { Value = nameof(Development) };
  public static Environment Test = new() { Value = nameof(Test) };
  public static Environment Staging = new() { Value = nameof(Staging) };
  public static Environment Production = new() { Value = nameof(Production) };

  public static implicit operator string(Environment environment)
  {
    return environment.Value;
  }
}
