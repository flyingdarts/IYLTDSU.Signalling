// See https://aka.ms/new-console-template for more information
using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2.Alpha;
using Amazon.CDK.AWS.Apigatewayv2.Integrations.Alpha;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using IYLTDSU.Signalling.Infrastructure;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

// ReSharper disable InconsistentNaming
var app = new App();

var stack = new Stack(app, WickedSickService.GetIdentifierFor(nameof(Stack)), new StackProps
{
    StackName = "IYLTDSU-Stacks-Development-WebSockets",
    Env = new Amazon.CDK.Environment { Account = "561462764262", Region = "eu-west-1" }
});

new SignallingConstruct(stack, "Signalling");

// var Principal = new ServicePrincipal("apigateway.amazonaws.com");
// var Role = new Role(stack, "WebSocketRole", new RoleProps
// {
//     RoleName = "websockets-role",
//     AssumedBy = Principal
// });
// func.OnConnect.GrantInvoke(Role);

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

app.Synth();