using Amazon.Lambda.Core;
using System.Net;
using System.Text.Json;
using System.Text;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;
using IYLTDSU.Signalling.Shared;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace IYLTDSU.Signalling.OnDefault;
public class Function
{
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly string _tableName = Environment.GetEnvironmentVariable("TableName")!;
    private readonly string _webSocketApiUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
    private readonly Func<string, AmazonApiGatewayManagementApiClient> _apiGatewayManagementApiClientFactory = (Func<string, AmazonApiGatewayManagementApiClient>)((endpoint) =>
    {
        return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
        {
            ServiceURL = endpoint
        });
    });

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            // The body will look something like this: {"message":"sendmessage", "data":"What are you doing?"}
            JsonDocument message = JsonDocument.Parse(request.Body);

            // Grab the data from the JSON body which is the message to broadcasted.
            JsonElement dataProperty;
            if (!message.RootElement.TryGetProperty("message", out dataProperty) || dataProperty.GetString() == null)
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
                TableName = _tableName,
                ProjectionExpression = Fields.ConnectionId
            };

            var scanResponse = await _dynamoDbClient.ScanAsync(scanRequest);

            // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
            var apiClient = _apiGatewayManagementApiClientFactory(_webSocketApiUrl);

            // Loop through all of the connections and broadcast the message out to the connections.
            var count = 0;
            foreach (var item in scanResponse.Items)
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
    }
};