using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK;
using Constructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYLTDSU.Signalling.Infrastructure;

class FunctionConstruct : Construct
{
    public FunctionConstruct(Construct scope, string functionName) : base(scope, functionName)
    {
        var repository = new Repository(scope, "Repository", new RepositoryProps
        {
            RemovalPolicy = RemovalPolicy.DESTROY,
            LifecycleRules = new ILifecycleRule[]
            {
                new LifecycleRule
                {
                    MaxImageCount = 2,
                    TagStatus = TagStatus.ANY
                }
            }
        });

        new Function(scope, "Function", new FunctionProps
        {
            FunctionName = functionName,
            Handler = Handler.FROM_IMAGE,
            Code = Code.FromEcrImage(Repository.FromRepositoryName(scope, "CodeFromRepo", repository.RepositoryName)),
            Runtime = Runtime.FROM_IMAGE,
            Timeout = Duration.Seconds(30),
            MemorySize = 256
        });

        new CfnOutput(scope, WickedSickService.GetIdentifierFor(nameof(CfnOutput)), new CfnOutputProps
        {
            ExportName = WickedSickService.GetIdentifierFor(nameof(CfnOutput)),
            Description = WickedSickService.COMPANY_SLOGAN,
            Value = repository.RepositoryArn
        });
    }
}