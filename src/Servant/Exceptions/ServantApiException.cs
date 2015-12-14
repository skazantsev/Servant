using System;
using System.Net;

namespace Servant.Exceptions
{
    public class ServantApiException : Exception
    {
        public ServantApiException()
            : this (HttpStatusCode.InternalServerError)
        { }

        public ServantApiException(string message)
            : this(HttpStatusCode.InternalServerError, message)
        { }

        public ServantApiException(string message, Exception innerException)
            : this(HttpStatusCode.InternalServerError, message, innerException)
        { }

        public ServantApiException(HttpStatusCode statusCode)
            : base("An unspecified error has occured in Servant.")
        {
            StatusCode = statusCode;
        }

        public ServantApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public ServantApiException(HttpStatusCode statusCode, string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}
