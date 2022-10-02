namespace IYLTDSU.Signalling.Runtime.ApiGateway;

/// <summary>
/// The response of the websocket request
/// </summary>
public class ApiGatewayErrorResponse
{
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}