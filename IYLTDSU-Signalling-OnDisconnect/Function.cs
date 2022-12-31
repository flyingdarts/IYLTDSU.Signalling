using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using IYLTDSU.Signalling.Shared;

public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private readonly AmazonDynamoDBClient _dynamoDbClient = new();
    private readonly string _tableName = Environment.GetEnvironmentVariable("TableName")!;

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var connectionId = request.RequestContext.ConnectionId;
        context.Logger.LogInformation($"ConnectionId: {connectionId}");

        var ddbRequest = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { Fields.ConnectionId, new AttributeValue { S = connectionId } }
            }
        };

        await _dynamoDbClient.DeleteItemAsync(ddbRequest);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = "Disconnected."
        };
    }
};