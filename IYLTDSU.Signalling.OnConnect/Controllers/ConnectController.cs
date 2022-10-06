using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using IYLTDSU.Signalling.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IYLTDSU.Signalling.OnConnect.Controllers;

[ApiController]
[Route("[controller]")]
public class ConnectController : ControllerBase
{
    private IAmazonDynamoDB DynamoDbClient { get; }
    private ApplicationOptions ApplicationOptions { get; }
    public ConnectController(AmazonDynamoDBClient dynamoDbClient, IOptions<ApplicationOptions> options)
    {
        DynamoDbClient = dynamoDbClient;
        ApplicationOptions = options.Value;
    }

    [HttpPost]
    [ProducesResponseType(200, Type = typeof(APIGatewayProxyResponse))]
    [ProducesResponseType(500, Type = typeof(APIGatewayProxyResponse))]
    async Task<APIGatewayProxyResponse> OnConnectHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;

            var ddbRequest = new PutItemRequest
            {
                TableName = ApplicationOptions.TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { ApplicationOptions.ConnectionIdField, new AttributeValue{ S = connectionId}}
                }
            };

            await DynamoDbClient.PutItemAsync(ddbRequest);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Connected."
            };
        }
        catch (Exception e)
        {
            context.Logger.LogInformation("Error connecting: " + e.Message);
            context.Logger.LogInformation(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to connect: {e.Message}"
            };
        }
    }
}