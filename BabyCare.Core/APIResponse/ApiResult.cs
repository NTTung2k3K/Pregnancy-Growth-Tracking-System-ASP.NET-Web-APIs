using System.Net;

namespace BabyCare.Core.APIResponse
{
    public class ApiResult<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsSuccessed { get; set; }
        public T ResultObj { get; set; }

    }
}
