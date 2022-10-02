using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using IYLTDSU.Signalling.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Text;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;

namespace IYLTDSU.Signalling.OnMessage.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(201, Type = typeof(APIGatewayProxyResponse))]
[ProducesResponseType(500, Type = typeof(APIGatewayProxyResponse))]
public class MessageController : ControllerBase
{
    private readonly ApplicationOptions ApplicationOptions;
    public IAmazonDynamoDB DynamoDbClient { get; } = new AmazonDynamoDBClient();
    private Func<string, IAmazonApiGatewayManagementApi> ApiGatewayManagementApiClientFactory { get; }

    public MessageController(AmazonDynamoDBClient client, IOptions<ApplicationOptions> options)
    {
        DynamoDbClient = client;
        ApplicationOptions = options.Value;
    }

    public async Task<APIGatewayProxyResponse> SendMessageHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            // Construct the API Gateway endpoint that incoming message will be broadcasted to.
            var domainName = request.RequestContext.DomainName;
            var stage = request.RequestContext.Stage;
            var endpoint = $"https://{domainName}/{stage}";
            context.Logger.LogInformation($"API Gateway management endpoint: {endpoint}");

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
                TableName = ApplicationOptions.TableName,
                ProjectionExpression = ApplicationOptions.ConnectionIdField
            };

            var scanResponse = await DynamoDbClient.ScanAsync(scanRequest);

            // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
            var apiClient = ApiGatewayManagementApiClientFactory(endpoint);

            // Loop through all of the connections and broadcast the message out to the connections.
            var count = 0;
            foreach (var item in scanResponse.Items)
            {
                var postConnectionRequest = new PostToConnectionRequest
                {
                    ConnectionId = item[ApplicationOptions.ConnectionIdField].S,
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
                            TableName = ApplicationOptions.TableName,
                            Key = new Dictionary<string, AttributeValue>
                            {
                                {ApplicationOptions.ConnectionIdField, new AttributeValue {S = postConnectionRequest.ConnectionId}}
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
    }
}