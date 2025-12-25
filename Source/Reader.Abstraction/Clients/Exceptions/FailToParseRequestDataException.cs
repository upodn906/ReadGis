using Reader.Abstraction.Clients.Exceptions._Common;

namespace Reader.Abstraction.Clients.Exceptions
{
    public class FailToParseRequestDataException : GisClientException
    {
        public FailToParseRequestDataException(string message) : base(message)
        {
        }
    }
}
