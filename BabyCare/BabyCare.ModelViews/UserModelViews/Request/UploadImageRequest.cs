using Microsoft.AspNetCore.Http;

namespace BabyCare.ModelViews.UserModelViews.Request
{
    public class UploadImageRequest
    {
        public IFormFile Image { get; set; }
    }
}
