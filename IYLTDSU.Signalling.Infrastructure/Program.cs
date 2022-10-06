// See https://aka.ms/new-console-template for more information
using Amazon.CDK;
using IYLTDSU.Signalling.Infrastructure;

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