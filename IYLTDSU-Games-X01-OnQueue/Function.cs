using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var connectionId = request.RequestContext.ConnectionId;

    return new APIGatewayProxyResponse
    {
        StatusCode = 200,
        Body = "Player has left the room"
    };
};