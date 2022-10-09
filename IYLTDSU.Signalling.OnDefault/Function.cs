using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Net;
using System.Text.Json;
using System.Text;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;

var DynamoDbClient = new AmazonDynamoDBClient();
var TableName = Environment.GetEnvironmentVariable("TableName")!;
var WebSocketApiUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
var ConnectionIdField = "ConnectionId";
var ApiGatewayManagementApiClientFactory = (Func<string, AmazonApiGatewayManagementApiClient>)((endpoint) =>
{
    return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
    {
        ServiceURL = endpoint
    });
});
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    try
    {
        // The body will look something like this: {"message":"sendmessage", "data":"What are you doing?"}
        JsonDocument message = JsonDocument.Parse(request.Body);

        // Grab the data from the JSON body which is the message to broadcasted.
        JsonElement dataProperty;
        if (!message.RootElement.TryGetProperty("data", out dataProperty) || dataProperty.GetString() == null)
        {
            context.Logger.LogInformation("Failed to find data element in JSON document");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        var data = dataProperty.GetString() ?? "";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // List all of the current connections. In a more advanced use case the table could be used to grab a group of connection ids for a chat group.
        var scanRequest = new ScanRequest
        {
            TableName = TableName,
            ProjectionExpression = ConnectionIdField
        };

        var scanResponse = await DynamoDbClient.ScanAsync(scanRequest);

        // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
        var apiClient = ApiGatewayManagementApiClientFactory(WebSocketApiUrl);

        // Loop through all of the connections and broadcast the message out to the connections.
        var count = 0;
        foreach (var item in scanResponse.Items)
        {
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = item[ConnectionIdField].S,
                Data = stream
            };

            try
            {
                context.Logger.LogInformation($"Post to connection {count}: {postConnectionRequest.ConnectionId}");
                stream.Position = 0;
                await apiClient.PostToConnectionAsync(postConnectionRequest);
                count++;
            }
            catch (AmazonServiceException e)
            {
                // API Gateway returns a status of 410 GONE then the connection is no
                // longer available. If this happens, delete the identifier
                // from our DynamoDB table.
                if (e.StatusCode == HttpStatusCode.Gone)
                {
                    var ddbDeleteRequest = new DeleteItemRequest
                    {
                        TableName = TableName,
                        Key = new Dictionary<string, AttributeValue>
                            {
                                {ConnectionIdField, new AttributeValue {S = postConnectionRequest.ConnectionId}}
                            }
                    };

                    context.Logger.LogInformation($"Deleting gone connection: {postConnectionRequest.ConnectionId}");
                    await DynamoDbClient.DeleteItemAsync(ddbDeleteRequest);
                }
                else
                {
                    context.Logger.LogInformation($"Error posting message to {postConnectionRequest.ConnectionId}: {e.Message}");
                    context.Logger.LogInformation(e.StackTrace);
                }
            }
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "Data sent to " + count + " connection" + (count == 1 ? "" : "s")
        };
    }
    catch (Exception e)
    {
        context.Logger.LogInformation("Error disconnecting: " + e.Message);
        context.Logger.LogInformation(e.StackTrace);
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Body = $"Failed to send message: {e.Message}"
        };
    }
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();