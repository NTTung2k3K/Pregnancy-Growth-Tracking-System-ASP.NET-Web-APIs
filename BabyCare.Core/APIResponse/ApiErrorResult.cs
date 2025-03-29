using System.Net;

namespace BabyCare.Core.APIResponse
{

    public class ApiErrorResult<T> : ApiResult<T>
    {

        public List<string> Errors { get; set; }
        public ApiErrorResult(string message)
        {
            StatusCode = HttpStatusCode.BadRequest;
            Message = message;
            IsSuccessed = false;
        }
        public ApiErrorResult(string message, HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Message = message;
            IsSuccessed = false;
        }

        public ApiErrorResult(string message, List<string> errors)
        {
            StatusCode = HttpStatusCode.UnprocessableEntity;
            Message = message;
            IsSuccessed = false;
            Errors = errors;
        }
        public ApiErrorResult(string message, List<string> errors, HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Message = message;
            IsSuccessed = false;
            Errors = errors;
        }
    }
}
