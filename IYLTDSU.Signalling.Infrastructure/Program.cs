// See https://aka.ms/new-console-template for more information
using Amazon.CDK;
using IYLTDSU.Signalling.Infrastructure;

var app = new App();

var stack = new Stack(app, WickedSickService.GetIdentifierFor(nameof(Stack)), new StackProps
{
    StackName = "IYLTDSU-WebSocket-Stack",
    Env = new Amazon.CDK.Environment { 
        Account = System.Environment.GetEnvironmentVariable("AWS_ACCOUNT")!, 
        Region = System.Environment.GetEnvironmentVariable("AWS_REGION")! 
    }
});

new SignallingConstruct(stack, "Signalling");

app.Synth();