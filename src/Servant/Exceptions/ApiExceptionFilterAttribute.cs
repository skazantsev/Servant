using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Servant.Exceptions
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;
            if (exception is ServantException || exception is ServantApiException)
            {
                var apiException = actionExecutedContext.Exception as ServantApiException;
                var statusCode = apiException?.StatusCode ?? HttpStatusCode.InternalServerError;
                actionExecutedContext.Response =
                    actionExecutedContext.Request.CreateErrorResponse(statusCode, exception);
            }
        }
    }
}
