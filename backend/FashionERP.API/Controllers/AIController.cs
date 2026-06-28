using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.AI;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    public class AIController : BaseController
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>Chatbot tư vấn sản phẩm/khuyến mãi cho khách hàng (không yêu cầu đăng nhập)</summary>
        [HttpPost("chatbot")]
        [AllowAnonymous]
        public async Task<IActionResult> Chat([FromBody] ChatbotRequestDto request)
        {
            // Nếu có JWT hợp lệ thì lấy UserId để ghi log, không bắt buộc đăng nhập
            Guid? userId = User?.Identity?.IsAuthenticated == true ? CurrentUserId : null;
            var result = await _aiService.ChatAsync(request, userId);
            return Ok(result);
        }

        /// <summary>Gợi ý size dựa trên số đo cơ thể khách hàng</summary>
        [HttpPost("size-recommend")]
        [AllowAnonymous]
        public async Task<IActionResult> RecommendSize([FromBody] SizeRecommendRequestDto request)
        {
            Guid? userId = User?.Identity?.IsAuthenticated == true ? CurrentUserId : null;
            var result = await _aiService.RecommendSizeAsync(request, userId);
            return Ok(result, "Gợi ý size thành công");
        }

        /// <summary>Dự báo nhu cầu/tồn kho cho một variant cụ thể</summary>
        [HttpPost("inventory-forecast")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Forecast([FromBody] InventoryForecastRequestDto request)
        {
            var result = await _aiService.ForecastAsync(request, CurrentUserId);
            return Ok(result, "Dự báo tồn kho thành công");
        }

        /// <summary>Phân tích xu hướng bán hàng theo khoảng thời gian</summary>
        [HttpGet("trend-analysis")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetTrendAnalysis(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string? category = null)
        {
            var request = new TrendAnalysisRequestDto(from, to, category);
            var result = await _aiService.GetTrendAnalysisAsync(request, CurrentUserId);
            return Ok(result, "Phân tích xu hướng thành công");
        }
    }
}