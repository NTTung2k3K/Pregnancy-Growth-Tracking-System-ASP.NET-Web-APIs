using System.Net;

namespace BabyCare.Core.APIResponse
{
    public class ApiSuccessResult<T> : ApiResult<T>
    {

        public ApiSuccessResult(T resultObj)
        {
            IsSuccessed = true;
            StatusCode = System.Net.HttpStatusCode.OK;
            ResultObj = resultObj;
        }
        public ApiSuccessResult(T resultObj, HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            IsSuccessed = true;
            ResultObj = resultObj;
        }
        public ApiSuccessResult(T resultObj, string message)
        {
            StatusCode = System.Net.HttpStatusCode.OK;
            IsSuccessed = true;
            Message = message;
            ResultObj = resultObj;
        }
        public ApiSuccessResult(T resultObj, string message, HttpStatusCode statusCode)
        {
            StatusCode=statusCode;
            IsSuccessed = true;
            Message = message;
            ResultObj = resultObj;
        }

        public ApiSuccessResult(string message)
        {
            StatusCode = HttpStatusCode.OK;
            Message = message;
            IsSuccessed = true;
        }

        public ApiSuccessResult(string message, HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Message = message;
            IsSuccessed = true;
        }
    }
}
