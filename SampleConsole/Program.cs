
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
    .AddScoped<IFoo, Foo>()
    .AddScoped(typeof(IBar), _ => new Bar())
    .AddSingleton<IBaZ, BaZ>()
    .BuildServiceProvider()
;


IFoo foo=serviceProvider.GetRequiredService<IFoo>();

Console.WriteLine("Over");
public interface IFoo { }
public class Foo : IFoo
{
    public Foo() { }
}


public interface IBar { }
public class Bar : IBar
{
    public Bar() { }
}
public interface IBaZ { }
public class BaZ: IBaZ
{
    public BaZ() { }
}
