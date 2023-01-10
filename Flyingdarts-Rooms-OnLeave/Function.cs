using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var connectionId = request.RequestContext.ConnectionId;

    return new APIGatewayProxyResponse
    {
        StatusCode = 200,
        Body = "Player has left the room"
    };
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();