using Schism.Lib.Core.Internal;

namespace Schism.Tests.Core;
internal class TestInterfaceEmitter
{
    [Test]
    public async Task TestObjRetTypeAndTwoParams()
    {
        InterfaceEmitter emitter = new();
        ITestEmit obj = emitter.CreateType<ITestEmit>(val =>
        {
            if (val.MoveNext() is Test1Req req && val.MoveNext() is Test1Req2 req2)
            {
                return Task.FromResult<object?>(new Test1Res() { Data = req.Hello + " " + req2.Hello });
            }
            return Task.FromResult<object?>(null);
        });
        Test1Res result = await obj.Test1(new Test1Req() { Hello = "hi" }, new Test1Req2() { Hello = "world" });
        Console.WriteLine(result.Data);
    }
    [Test]
    public async Task TestVoidRetTypeAndOneParam()
    {
        InterfaceEmitter emitter = new();
        ITestEmit obj = emitter.CreateType<ITestEmit>(val =>
        {
            if (val.MoveNext() is Test1Req req)
            {
                Console.WriteLine(req.Hello);
            }
            return Task.FromResult<object?>(null);
        });
        await obj.Test2(new Test1Req() { Hello = "hi" });
    }
}

public interface ITestEmit
{
    Task<Test1Res> Test1(Test1Req req, Test1Req2 req2);
    Task Test2(Test1Req req);
}

public record Test1Req
{
    public string Hello { get; set; } = "";
}
public record Test1Req2
{
    public string Hello { get; set; } = "";
}
public record Test1Res
{
    public string Data { get; set; } = "";
}