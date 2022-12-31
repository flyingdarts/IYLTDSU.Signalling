using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Xunit;
using Amazon.Lambda.TestUtilities;

namespace IYLTDSU.Games.X01.OnQueue.Tests;

public class FunctionTest
{
    [Fact]
    public async void EnsureOnlyValidIdentifierCanQueue()
    {

        var function = new Function();
        var context = new TestLambdaContext();
        var response = await function.FunctionHandler(new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                ConnectionId = "test"
            }
        }, context);

        Assert.False(response.StatusCode == (int)HttpStatusCode.OK);
    }
}