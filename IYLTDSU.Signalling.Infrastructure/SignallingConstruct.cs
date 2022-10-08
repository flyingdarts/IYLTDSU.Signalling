using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2.Alpha;
using Amazon.CDK.AWS.Apigatewayv2.Integrations.Alpha;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace IYLTDSU.Signalling.Infrastructure;

class SignallingConstruct : Construct
{
    public Function OnConnect { get; }
    public Function OnDisconnect { get; }
    public Function OnDefault { get; }
    public SignallingConstruct(Construct scope, string id) : base(scope, id)
    {
        var table = new Table(scope, "Table", new TableProps
        {
            TableName = "SignallingTable",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            Encryption = TableEncryption.AWS_MANAGED,
            PartitionKey = new Attribute { Type = AttributeType.STRING, Name = "ConnectionId" },
        });

        OnConnect = new Function(scope, "OnConnectFunction", new FunctionProps
        {
            FunctionName = "Signalling-OnConnect",
            Handler = "Signalling.OnConnect",
            Code = Code.FromAsset("lambda.zip"),
            Runtime = Runtime.DOTNET_6,
            Timeout = Duration.Seconds(30),
            MemorySize = 256,
            Environment = new Dictionary<string, string>
            {
                {
                    "TableName", table.TableName
                }
            }
        });
        table.GrantFullAccess(OnConnect);

        OnDisconnect = new Function(scope, "OnDisconnectFunction", new FunctionProps
        {
            FunctionName = "Signalling-OnDisconnect",
            Handler = "Signalling.OnDisconnect",
            Code = Code.FromAsset("lambda.zip"),
            Runtime = Runtime.DOTNET_6,
            Timeout = Duration.Seconds(30),
            MemorySize = 256,
            Environment = new Dictionary<string, string>
            {
                {
                    "TableName", table.TableName
                }
            }
        });
        table.GrantFullAccess(OnDisconnect);

        OnDefault = new Function(scope, "OnDefaultFunction", new FunctionProps
        {
            FunctionName = "Signalling-OnDefault",
            Handler = "Signalling.OnDefault",
            Code = Code.FromAsset("lambda.zip"),
            Runtime = Runtime.DOTNET_6,
            Timeout = Duration.Seconds(30),
            MemorySize = 256,
            Environment = new Dictionary<string, string>
            {
                {
                    "TableName", table.TableName
                }
            }
        });
        table.GrantFullAccess(OnDefault);

        new WebSocketApi(scope, "SocketApi", new WebSocketApiProps
        {
            ApiName = "SignallingApi",
            ConnectRouteOptions = new WebSocketRouteOptions
            {
                Integration = new WebSocketLambdaIntegration("ConnectIntegration", OnConnect)
            },
            DefaultRouteOptions = new WebSocketRouteOptions
            {
                Integration = new WebSocketLambdaIntegration("DefaultIntegration", OnDefault)
            },
            DisconnectRouteOptions = new WebSocketRouteOptions
            {
                Integration = new WebSocketLambdaIntegration("DisconnectIntegration", OnDisconnect)
            }
        });
    }
}