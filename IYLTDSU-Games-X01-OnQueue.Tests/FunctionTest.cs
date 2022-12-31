using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

namespace IYLTDSU_Games_X01_OnQueue.Tests;

public class FunctionTest
{
    [Fact]
    public void TestToUpperFunction()
    {

        var upperCase = "hello world".ToUpper();

        Assert.Equal("HELLO WORLD", upperCase);
    }
}
