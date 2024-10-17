using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("get-message")]
        public async Task<ActionResult> getMessage(string message)
        {
            var a=await _aiService.getResponse(message);
            return Ok(a);
        }
        /// <summary>
        /// 获取历史信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("get-histor-message")]
        public async Task<ActionResult> getHistoryMessage()
        {
            var a=await _aiService.getHistoryMessagAsync();
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
