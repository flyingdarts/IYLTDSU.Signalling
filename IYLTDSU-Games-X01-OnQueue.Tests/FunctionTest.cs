using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

public class FunctionTest
{
    [Fact]
    public void TestToUpperFunction()
    {
        var entryPoint = typeof(Program).Assembly.EntryPoint!;
        entryPoint.Invoke(null, new object[] { Array.Empty<string>() });
        var upperCase = "hello world".ToUpper();

        Assert.Equal("HELLO WORLD", upperCase);
    }
    [Fact]
    public void TestToLowerFunction()
    {

        var upperCase = "hello world".ToUpper();

        Assert.Equal("HELLO WORLD", upperCase);
    }
}
