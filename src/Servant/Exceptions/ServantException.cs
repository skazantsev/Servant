using System;
using System.Runtime.Serialization;

namespace Servant.Exceptions
{
    public class ServantException : Exception
    {
        public ServantException()
            :base("An unspecified error has occured in Servant.")
        { }

        public ServantException(string message)
            : base(message)
        { }

        public ServantException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public ServantException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
