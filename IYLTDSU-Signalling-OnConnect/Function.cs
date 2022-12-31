using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using IYLTDSU.Signalling.Shared;

var DynamoDbClient = new AmazonDynamoDBClient();
var TableName = Environment.GetEnvironmentVariable("TableName")!;
// The function handler that will be called for each Lambda event
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var connectionId = request.RequestContext.ConnectionId;

    var ddbRequest = new PutItemRequest
    {
        TableName = TableName,
        Item = new Dictionary<string, AttributeValue>
        {
            { Fields.ConnectionId, new AttributeValue{ S = connectionId}}
        }
    };

    await DynamoDbClient.PutItemAsync(ddbRequest);

    return new APIGatewayProxyResponse
    {
        StatusCode = 200,
        Body = "Connected"
    };
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();