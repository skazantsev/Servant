using System;
using System.Net;

namespace Servant.Exceptions
{
    public class ServantApiValidationException : ServantApiException
    {
        public ServantApiValidationException()
            : base(HttpStatusCode.BadRequest)
        { }

        public ServantApiValidationException(string message)
            : base(HttpStatusCode.BadRequest, message)
        { }

        public ServantApiValidationException(string message, Exception innerException)
            : base(HttpStatusCode.BadRequest, message, innerException)
        { }
    }
}
