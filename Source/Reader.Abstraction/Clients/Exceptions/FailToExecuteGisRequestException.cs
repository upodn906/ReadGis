using Reader.Abstraction.Clients.Exceptions._Common;

namespace Reader.Abstraction.Clients.Exceptions;

public class FailToExecuteGisRequestException : GisClientException
{
    public FailToExecuteGisRequestException(string response, int statusCode) :
        base($"Fail to execute request with status code {statusCode} and response {response}.")
    {
    }
    public FailToExecuteGisRequestException(string message) :
        base(message)
    {
    }
}