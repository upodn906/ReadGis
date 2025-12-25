using Reader.Abstraction.Clients.Exceptions._Common;

namespace Reader.Abstraction.Clients.Exceptions
{
    public class GisServerErrorException : GisClientException
    {
        public GisServerErrorException(string message) : base(message)
        {
        }
    }
}
