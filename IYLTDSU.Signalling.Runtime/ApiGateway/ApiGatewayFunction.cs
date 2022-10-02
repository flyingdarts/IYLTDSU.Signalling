using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using IYLTDSU.Signalling.Runtime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYLTDSU.Signalling.Runtime.ApiGateway
{
    public abstract class ApiGatewayFunction : BaseFunction
    {

        public async Task<ApiGatewayResponse> FunctionHandler(APIGatewayProxyRequest evnt, ILambdaContext context)
        {
            return await ProcessMessages(evnt, context);
        }
        public abstract Task<ApiGatewayResponse> ProcessMessages(APIGatewayProxyRequest message, ILambdaContext context);
    }
}
