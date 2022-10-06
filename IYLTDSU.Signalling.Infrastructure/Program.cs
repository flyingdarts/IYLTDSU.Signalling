// See https://aka.ms/new-console-template for more information
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2.Alpha;
using Amazon.CDK.AWS.Apigatewayv2.Integrations.Alpha;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SSM;
using IYLTDSU.Signalling.Core;
using System.Net.WebSockets;
using System.Security;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.Pipelines;
using Constructs;using IYLTDSU.Signalling.Infrastructure;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;
// ReSharper disable InconsistentNaming


var app = new App();

var stack = new Stack(app, WickedSickService.GetIdentifierFor(nameof(Stack)), new StackProps
{
    StackName = "IYLTDSU-Stacks-Development-WebSockets",
    Env = new Amazon.CDK.Environment { Account = "561462764262", Region = "eu-west-1" }
});

new FunctionConstruct(stack, "OnConnect");
new FunctionConstruct(stack, "Disconnect");
new FunctionConstruct(stack, "OnMessage");

app.Synth();

static class WickedSickService
{
    public const string DOTNET_ENVIRONMENT = "Development";
    public const string COMPANY_NAME = "Flyingdarts";
    public const string COMPANY_SLOGAN = "If you love the darts, stand up!"; // TODO: DescriptionAspect
    public const string COMPANY_SHORT = "IYLTDSU"; // TODO: ResourceIdentifierAspect

    public static string GetIdentifierFor(string nameOfResource)
    {
        return $"{COMPANY_NAME}-{DOTNET_ENVIRONMENT}-{nameOfResource}";
    }
}

public struct Endpoints
{
    public Endpoints() { }
    public string OnConnect { get; set; } = "OnConnect";
    public string OnDisconnect { get; set; } = "OnDisconnect";
    public string OnMessage { get; set; } = "OnMessage";
}

// var table = new Table(stack, "Table", new TableProps
// {
//     BillingMode = BillingMode.PAY_PER_REQUEST,
//     Encryption = TableEncryption.AWS_MANAGED,
//     PartitionKey = new Attribute { Type = AttributeType.STRING, Name = "ConnectionId" },
// });
//
// var stringBuilder = new System.Text.StringBuilder();
// stringBuilder.Append($"/{Constants.Prefix}");
// stringBuilder.Append($"/{Constants.Flyingdarts}");
// stringBuilder.Append($"/{System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
// stringBuilder.Append($"/{nameof(WebSocketApi)}");
// stringBuilder.Append("/DynamoDb");
//
// var stringParam = new StringParameter(stack, "StringParam", new StringParameterProps
// {
//     ParameterName = stringBuilder.ToString(),
//     StringValue = table.TableName
// });
//













// OnConnectFunction.Role!.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AWSLambdaBasicExecutionRole"));
// table.GrantFullAccess(OnConnectFunction);
//
// var OnDisconnectFunction = new Function(stack, "OnDisconnectFunction", new FunctionProps
// {
//     FunctionName = "OnDisconnect",
//     Handler = "IYLTDSU.WebSocketApi.OnDisconnect::IYLTDSU.WebSocketApi.OnDisconnect:Functions::OnDisconnectHandler",
//     Code = Code.FromDockerBuild("src/IYLTDSU.WebSocketApi.OnDisconnect"),
//     Runtime = Runtime.FROM_IMAGE,
//     Timeout = Duration.Seconds(30),
//     MemorySize = 256,
//     Environment = new Dictionary<string, string>
//      {
//          { "TABLE_NAME", table.TableName }
//      }
// });
// OnDisconnectFunction.Role!.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AWSLambdaBasicExecutionRole"));
// table.GrantFullAccess(OnDisconnectFunction);
//
// var OnMessageFunction = new Function(stack, "OnMessageFunction", new FunctionProps
// {
//     FunctionName = "OnMessage",
//     Handler = "IYLTDSU.WebSocketApi.OnMessage::IYLTDSU.WebSocketApi.OnMessage:Functions::OnMessageHandler",
//     Code = Code.FromDockerBuild("src/IYLTDSU.WebSocketApi.OnMessage"),
//     Runtime = Runtime.FROM_IMAGE,
//     Timeout = Duration.Seconds(30),
//     MemorySize = 256,
//     Environment = new Dictionary<string, string>
//      {
//          { "TABLE_NAME", table.TableName }
//      }
// });
// OnMessageFunction.Role!.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AWSLambdaBasicExecutionRole"));
// table.GrantFullAccess(OnMessageFunction);
//
// var SignallingWebSocketApi = new WebSocketApi(stack, "WebSocketApi", new WebSocketApiProps
// {
//     ApiName = $"{Constants.Flyingdarts}{nameof(WebSocketApi)}"
// });
//
// var OnConnectIntegration = new WebSocketLambdaIntegration("OnConnectIntegration", OnConnectFunction);
// SignallingWebSocketApi.AddRoute("connect", new WebSocketRouteOptions { Integration = OnConnectIntegration });
//
// var OnDisconnectIntegration = new WebSocketLambdaIntegration("OnDisconnectIntegration", OnDisconnectFunction);
// SignallingWebSocketApi.AddRoute("disconnect", new WebSocketRouteOptions { Integration = OnDisconnectIntegration });
//
// var OnMessageIntegration = new WebSocketLambdaIntegration("OnMessageIntegration", OnMessageFunction);
// SignallingWebSocketApi.AddRoute("message", new WebSocketRouteOptions { Integration = OnMessageIntegration });
//
// var stage = new WebSocketStage(stack, "WebSocketStage", new WebSocketStageProps
// {
//     WebSocketApi = SignallingWebSocketApi,
//     StageName = "Develop"
// });
//
// //Docus below show possibility to use JwtAuthorizer
// // https://docs.aws.amazon.com/cdk/api/v2/dotnet/api/Amazon.CDK.AWS.Apigatewayv2.Authorizers.Alpha.html
//
// var Principal = new ServicePrincipal("apigateway.amazonaws.com");
// OnConnectFunction.GrantInvoke(Principal);
// OnConnectFunction.AddPermission("OnConnectInvocation", new Permission
// {
//     Principal = Principal
// });
//
// OnDisconnectFunction.GrantInvoke(Principal);
// OnDisconnectFunction.AddPermission("OnDisconnectInvocation", new Permission
// {
//     Principal = Principal
// });
//
// OnMessageFunction.GrantInvoke(Principal);
// OnMessageFunction.AddPermission("OnMessageInvocation", new Permission
// {
//     Principal = Principal
// });
