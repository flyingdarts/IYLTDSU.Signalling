using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using IYLTDSU.Signalling.Shared;
using System.Net;
using System.Text;
using System.Text.Json;

AmazonDynamoDBClient _dynamoDbClient = new();
string _tableName = Environment.GetEnvironmentVariable("TableName")!;
string _webSocketApiUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
Func<string, AmazonApiGatewayManagementApiClient> _apiGatewayManagementApiClientFactory = (Func<string, AmazonApiGatewayManagementApiClient>)((endpoint) =>
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
        var connectionId = request.RequestContext.ConnectionId;
        JsonDocument message = JsonDocument.Parse(request.Body);
        JsonElement dataProperty;
        if (!message.RootElement.TryGetProperty("message", out dataProperty) || dataProperty.GetString() == null)
        {
            context.Logger.LogInformation("Failed to find data element in JSON document");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        var roomId = Guid.Parse(dataProperty.GetString()!.Split("#")[0]);
        var playerId = Guid.Parse(dataProperty.GetString()!.Split("#")[1]);
        var score = int.Parse(dataProperty.GetString()!.Split("#")[2]);
        var input = int.Parse(dataProperty.GetString()!.Split("#")[3]);

        var putItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { Fields.ConnectionId, new AttributeValue{ S = connectionId } },
                { Fields.RoomId, new AttributeValue{ S = roomId.ToString().ToLower() } },
                { Fields.PlayerId, new AttributeValue{ S = playerId.ToString().ToLower() } },
                { Fields.CurrentScore, new AttributeValue{ N = score.ToString() }},
                { Fields.LastInput, new AttributeValue{ N = input.ToString() } }
            }
        };

        await _dynamoDbClient.PutItemAsync(putItemRequest);

        // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
        var apiClient = _apiGatewayManagementApiClientFactory(_webSocketApiUrl);

        var data = JsonSerializer.Serialize(new
        {
            action = "x01/score-updated",
            message = $"{playerId}#{input}"
        });

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        // List all of the current connections. In a more advanced use case the table could be used to grab a group of connection ids for a chat group.
        var scanRequest = new ScanRequest
        {
            TableName = _tableName,
            ProjectionExpression = $"{Fields.ConnectionId},{Fields.RoomId}"
        };

        var scanResponse = await _dynamoDbClient.ScanAsync(scanRequest);

        var connectedClientsInRoom = scanResponse.Items.Where(x => x[Fields.RoomId].S == roomId.ToString().ToLower());

        // Loop through all of the connections and broadcast the message out to the connections.
        var count = 0;
        foreach (var item in connectedClientsInRoom)
        {
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = item[Fields.ConnectionId].S,
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
                        TableName = _tableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            {Fields.ConnectionId, new AttributeValue {S = postConnectionRequest.ConnectionId}}
                        }
                    };

                    context.Logger.LogInformation($"Deleting gone connection: {postConnectionRequest.ConnectionId}");
                    await _dynamoDbClient.DeleteItemAsync(ddbDeleteRequest);
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
            StatusCode = (int)HttpStatusCode.Created,
            Body = "X01 Score Input"
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