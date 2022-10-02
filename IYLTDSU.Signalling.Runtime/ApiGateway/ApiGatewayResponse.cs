using System.Net;
using IYLTDSU.Signalling.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IYLTDSU.Signalling.Runtime.ApiGateway;

public class ApiGatewayResponse
{
    public string Body { get; private set; }
    public HttpStatusCode StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public bool IsBase64Encoded { get; set; }

    public static ApiGatewayResponse FromSuccess<T>(HttpStatusCode statusCode, T toSerializeBody, IContractResolver? resolver = null)
    {
        return new ApiGatewayResponse
        {
            StatusCode = statusCode,
            Body = JsonConvert.SerializeObject(toSerializeBody, new JsonSerializerSettings
            {
                ContractResolver = resolver ?? new CamelCasePropertyNamesContractResolver()
            })
        };
    }

    public static ApiGatewayResponse FromFailure(HttpStatusCode statusCode, string errorCode, string? errorMessage = null)
    {
        return new ApiGatewayResponse
        {
            StatusCode = statusCode,
            Body = JsonConvert.SerializeObject(new ApiGatewayErrorResponse
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage ?? $"{DateTime.UtcNow.ToString(Constants.IsoDateFormat)}: Failure at ApiGateway"
            },
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            })
        };
    }
}