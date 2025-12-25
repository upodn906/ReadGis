using Reader.Abstraction.Clients.Exceptions._Common;

namespace Reader.Abstraction.Clients.Exceptions
{
    public class FailToAuthenticateException : GisClientException
    {
        public FailToAuthenticateException(string message) : base(message)
        {
        }
    }
}
