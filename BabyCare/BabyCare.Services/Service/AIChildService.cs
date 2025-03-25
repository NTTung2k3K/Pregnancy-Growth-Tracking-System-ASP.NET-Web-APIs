using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Core.APIResponse;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BabyCare.Contract.Services.Interface;

namespace BabyCare.Services.Service
{
    public class AIChildService : IAIChildService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string _apiKey = "AIzaSyAL9QvA5ZmZC0QkkWjzSZsPKFBj07YfZi4";

        public AIChildService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _contextAccessor = contextAccessor;
        }

        public async Task<ApiResult<string>> GetAIResponseAsync(string question, Guid userId, int childId)
        {
            var child = await _unitOfWork.GetRepository<Child>().Entities
                .Where(c => c.UserId == userId && c.Id == childId)
                .Include(c => c.FetalGrowthRecords)
                .ThenInclude(r => r.FetalGrowthStandard)
                .FirstOrDefaultAsync();

            if (child == null)
            {
                return new ApiErrorResult<string>("Không tìm thấy thông tin của trẻ.");
            }

            var records = child.FetalGrowthRecords.Select(record => new
            {
                record.WeekOfPregnancy,
                record.Weight,
                record.Height,
                record.HeadCircumference,
                record.AbdominalCircumference,
                record.FetalHeartRate,
                record.HealthCondition,
                Standard = record.FetalGrowthStandard != null ? new
                {
                    record.FetalGrowthStandard.MinWeight,
                    record.FetalGrowthStandard.MaxWeight,
                    record.FetalGrowthStandard.AverageWeight,
                    record.FetalGrowthStandard.MinHeight,
                    record.FetalGrowthStandard.MaxHeight,
                    record.FetalGrowthStandard.AverageHeight,
                    record.FetalGrowthStandard.HeadCircumference,
                    record.FetalGrowthStandard.AbdominalCircumference
                } : null
            }).ToList();

            var prompt = $@"
Dựa trên dữ liệu tăng trưởng của thai nhi:
{JsonConvert.SerializeObject(records, Formatting.Indented)}

Câu hỏi: {question}

Vui lòng trả lời dựa trên thông tin có sẵn, nếu không liên quan thì trả lời 'Câu hỏi không liên quan đến dữ liệu thai nhi.'.";

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
