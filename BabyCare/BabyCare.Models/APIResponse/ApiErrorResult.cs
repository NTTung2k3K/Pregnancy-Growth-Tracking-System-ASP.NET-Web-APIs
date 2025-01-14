using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Models.APIResponse
{

    public class ValidationError
    {
        public string Code { get; set; }
        public int? Minimum { get; set; }
        public int? Maxnimum { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Field { get; set; }
    }
    public class ApiErrorResult<T> : ApiResult<T>
    {

        public List<ValidationError> Errors { get; set; }
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

        public ApiErrorResult(string message, List<ValidationError> errors)
        {
            StatusCode = HttpStatusCode.UnprocessableEntity;
            Message = message;
            IsSuccessed = false;
            Errors = errors;
        }
        public ApiErrorResult(string message, List<ValidationError> errors, HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Message = message;
            IsSuccessed = false;
            Errors = errors;
        }
    }
}
