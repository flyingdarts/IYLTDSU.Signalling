using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Xunit;
using Amazon.Lambda.TestUtilities;

namespace IYLTDSU.Games.X01.OnScore.Tests;

public class FunctionTest
{
    [Fact]
    public async void EnsureOnlyValidIdentifierCanScoreX01()
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

        Assert.True(response.StatusCode == (int)HttpStatusCode.OK);
    }
}