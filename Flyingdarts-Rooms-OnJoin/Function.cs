using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Flyingdarts.Signalling.Shared;
using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

var _dynamoDbClient = new AmazonDynamoDBClient();
var _tableName = Environment.GetEnvironmentVariable("TableName")!;
var _webSocketApiUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
Func<string, AmazonApiGatewayManagementApiClient> _apiGatewayManagementApiClientFactory = (endpoint) =>
    new AmazonApiGatewayManagementApiClient(
        new AmazonApiGatewayManagementApiConfig
        {
            ServiceURL = endpoint
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

        var requestData = dataProperty.GetString()!.Split("#");

        var roomId = Guid.Parse(requestData[0]).ToString().ToLower();
        var playerId = Guid.Parse(requestData[1]).ToString().ToLower();
        var playerName = requestData[3];

        var putItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
                {
                    { Fields.ConnectionId, new AttributeValue { S = connectionId } },
                    { Fields.RoomId, new AttributeValue { S = roomId } },
                    { Fields.PlayerId, new AttributeValue { S = playerId } },
                    { Fields.PlayerName, new AttributeValue { S = playerName } }
                }
        };

        await _dynamoDbClient.PutItemAsync(putItemRequest);

        // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
        var apiClient = _apiGatewayManagementApiClientFactory(_webSocketApiUrl);

        var data = JsonSerializer.Serialize(new
        {
            action = "room/joined",
            message = playerId
        });

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // List all of the current connections. In a more advanced use case the table could be used to grab a group of connection ids for a chat group.
        var scanRequest = new ScanRequest
        {
            TableName = _tableName,
            ProjectionExpression = $"{Fields.ConnectionId},{Fields.RoomId},{Fields.PlayerId}"
        };

        var scanResponse = await _dynamoDbClient.ScanAsync(scanRequest);

        var connectedClientsInRoom =
            scanResponse.Items.Where(x => x[Fields.RoomId].S == roomId && x[Fields.PlayerId].S != playerId);

        if (connectedClientsInRoom.Any())
        {
            var returnToClientData = JsonSerializer.Serialize(new
            {
                action = "lobby/joined",
                message = connectedClientsInRoom.Select(x => x[Fields.PlayerId].S).ToArray()
            });

            var returnToClientDataStream = new MemoryStream(Encoding.UTF8.GetBytes(returnToClientData));

            var returnToClientRequest = new PostToConnectionRequest
            {
                ConnectionId = connectionId,
                Data = returnToClientDataStream
            };

            await apiClient.PostToConnectionAsync(returnToClientRequest);
        }

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
                                { Fields.ConnectionId, new AttributeValue { S = postConnectionRequest.ConnectionId } }
                            }
                    };

                    context.Logger.LogInformation(
                        $"Deleting gone connection: {postConnectionRequest.ConnectionId}");
                    await _dynamoDbClient.DeleteItemAsync(ddbDeleteRequest);
                }
                else
                {
                    context.Logger.LogInformation(
                        $"Error posting message to {postConnectionRequest.ConnectionId}: {e.Message}");
                    context.Logger.LogInformation(e.StackTrace);
                }
            }
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.Created,
            Body = "Room Joined"
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