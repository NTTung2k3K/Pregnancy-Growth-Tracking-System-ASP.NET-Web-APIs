using BabyCare.Core.APIResponse;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BabyCare.Contract.Services.Interface;

namespace BabyCare.Services.Service
{
    public class AIWebsiteService : IAIWebsiteService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey = "AIzaSyAL9QvA5ZmZC0QkkWjzSZsPKFBj07YfZi4";

        public AIWebsiteService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResult<string>> GetWebsiteAIResponseAsync(string question)
        {
            var websiteInfo = @"
Hệ thống theo dõi thai kỳ cung cấp thông tin và hỗ trợ phụ nữ mang thai với các chức năng:
- Theo dõi và cập nhật chỉ số thai nhi theo tuần (cân nặng, chiều cao, nhịp tim, ...).
- Xem biểu đồ tăng trưởng của thai nhi và so sánh với tiêu chuẩn.
- Cảnh báo khi phát hiện dấu hiệu bất thường trong sự phát triển của thai nhi.
- Nhắc nhở lịch khám thai, xét nghiệm quan trọng, lịch tiêm phòng.
- Tư vấn chế độ ăn uống, dinh dưỡng cho mẹ bầu trong từng giai đoạn.
- Đặt lịch hẹn khám với bác sĩ trực tuyến.
- Cung cấp thông tin về các triệu chứng phổ biến trong thai kỳ.
- Hỗ trợ mẹ bầu chia sẻ kinh nghiệm, giao lưu với cộng đồng mẹ bầu.
- Hướng dẫn cách chuẩn bị cho quá trình sinh nở và chăm sóc trẻ sơ sinh.

Các câu hỏi hợp lệ cần liên quan đến **thai kỳ, thai nhi, mẹ bầu, đặt lịch hẹn và các vấn đề liên quan**.";

            var prompt = $@"
Bạn là một chuyên gia về thai kỳ và phần mềm theo dõi thai kỳ.

Câu hỏi của người dùng: {question}

Hãy trả lời một cách chính xác, đầy đủ dựa trên thông tin sau:
{websiteInfo}

Nếu câu hỏi không liên quan đến chủ đề thai kỳ, vui lòng trả lời:
'Câu hỏi không liên quan đến thai kỳ hoặc hệ thống theo dõi thai kỳ.'";

            return await SendRequestToAI(prompt);
        }

        private async Task<ApiResult<string>> SendRequestToAI(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new object[] { new { text = prompt } } }
                }
            };

            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiErrorResult<string>($"API request failed: {response.StatusCode}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                JObject jsonResponse = JObject.Parse(responseContent);
                string generatedText = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                return new ApiSuccessResult<string>(generatedText ?? "Không có phản hồi từ AI.");
            }
            catch (JsonException)
            {
                return new ApiErrorResult<string>("Lỗi xử lý phản hồi từ AI.");
            }
        }
    }
}
