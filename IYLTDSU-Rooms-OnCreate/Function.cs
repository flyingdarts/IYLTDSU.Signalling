using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using IYLTDSU.Signalling.Shared;
using System.Net;
using System.Text.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace IYLTDSU.Rooms.OnCreate;
public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>

    private readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly string _tableName = Environment.GetEnvironmentVariable("TableName")!;
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
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

            var data = dataProperty.GetString()!.Split("#");

            var roomId = Guid.Parse(data[0]).ToString().ToLower();
            var playerId = Guid.Parse(data[1]).ToString().ToLower();
            var playerName = data[3];

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

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = "Room Created"
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
    }
}