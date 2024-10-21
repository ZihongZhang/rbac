using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rbac.CoreBusiness.Dtos.AiDtos;
using rbac.CoreBusiness.Services;

namespace rbac.Controllers
{
    public class AiController : BaseController
    {
        private readonly AiService _aiService;

        public AiController(AiService aiService)
        {
            _aiService = aiService;
        }
        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("get-message")]
        public async Task<ActionResult> getMessage(AiMessageDto message)
        {
            var a=await _aiService.getResponse(message.Message);
            return Ok(a);
        }
        /// <summary>
        /// 获取历史信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("get-history-message")]
        public async Task<ActionResult> getHistoryMessage()
        {
            var a=await _aiService.getHistoryMessageAsync();
            return Ok(a);
        }
        /// <summary>
        /// 删除当前用户历史记录
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("clear-histor-message")]
        public async Task<ActionResult> clearHistoryMessage()
        {
            var a=await _aiService.clearHistoryMessagAsync();
            return Ok(a);
        }
    }
}
