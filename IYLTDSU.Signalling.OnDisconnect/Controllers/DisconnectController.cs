using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using IYLTDSU.Signalling.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IYLTDSU.Signalling.OnDisconnect.Controllers;

[ApiController]
public class DisconnectController : ControllerBase
{
    private IAmazonDynamoDB DynamoDbClient { get; }
    private ApplicationOptions ApplicationOptions { get; }
    public DisconnectController(AmazonDynamoDBClient dynamoDbClient, IOptions<ApplicationOptions> options)
    {
        DynamoDbClient = dynamoDbClient;
        ApplicationOptions = options.Value;
    }

    [HttpPost]
    [Route("disconnect")]
    [ProducesResponseType(201, Type = typeof(APIGatewayProxyResponse))]
    [ProducesResponseType(500, Type = typeof(APIGatewayProxyResponse))]
    public async Task<APIGatewayProxyResponse> OnDisconnectHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogInformation($"ConnectionId: {connectionId}");

            var ddbRequest = new DeleteItemRequest
            {
                TableName = ApplicationOptions.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { ApplicationOptions.ConnectionIdField, new AttributeValue { S = connectionId } }
                }
            };

            await DynamoDbClient.DeleteItemAsync(ddbRequest);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Disconnected."
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("Error disconnecting: " + e.Message);
            context.Logger.LogInformation(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }
}