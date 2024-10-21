using System.Security.Claims;
using System.Text.Json.Serialization;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using rbac.Infra;
using rbac.Infra.Exceptions;
using rbac.Infra.FunctionalInterfaces;
using rbac.Modals.Models;
using rbac.Modals.Models.AiRelatedModel;
using RestSharp;
using SqlSugar;
using rbac.CoreBusiness.Qms.AiQms;
using rbac.Modals.Enum.AiRelatedEnum;
using rbac.CoreBusiness.Dtos.AiDtos;
using Mapster;

namespace rbac.CoreBusiness.Services;

public class AiService : IScoped
{
    public ISqlSugarClient _db { get; }
    public IHttpContextAccessor _httpContextAccessor { get; }
    public ILogger<AiService> _logger { get; }

    public AiService(ISqlSugarClient db, IHttpContextAccessor httpContextAccessor, ILogger<AiService> logger)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 向ai提出问题并将历史信息存到数据库
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    public async Task<string> getResponse(string message)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userId == null) throw new DomainException("没有当前登录用户信息");
        var history = await _db.Queryable<AiHistoryMessage>()
                               .Where(a => a.userId == userId)
                               .Includes(a => a.HistoryMessageList)
                               .FirstAsync();

        AiRequestBody requestBody = new();
        //判断是否有历史数据
        bool hasValue = false;

        //插入用户的输入信息
        if (history == null)
        {
            history = new AiHistoryMessage
                        {
                            userId = userId,
                            HistoryMessageList = new List<MessageEntry>()
                        };
            history.HistoryMessageList.AddRange(new List<MessageEntry>
            {
                new MessageEntry{ role = AiRoleEnum.user, content =  message,CreateTime = DateTime.Now, AiHistoryMessageId = history.Id}
            });
            // 设置请求体内容
            requestBody = new AiRequestBody
            {
                model = "deepseek-chat",
                messages = history.HistoryMessageList.Adapt<List<MessageEntryDto>>(),
                stream = false
            };
        }
        else
        {
            hasValue = true;
            history?.HistoryMessageList?.AddRange(new List<MessageEntry>
            {
                new MessageEntry{ role = AiRoleEnum.user, content =  message,CreateTime = DateTime.Now}
            });
            requestBody = new AiRequestBody
            {
                model = "deepseek-chat",
                messages = history?.HistoryMessageList?.OrderBy(a => a.CreateTime)
                                                      .ToList()
                                                      .Adapt<List<MessageEntryDto>>(),
                stream = false
            };
        }

        var content = await getAiHttpResponseAsync(AppSetting.GetValue("AiOption:url"), Method.Post, JsonConvert.SerializeObject(requestBody));
        if (content == "0")
        {
            throw new DomainException("ai沒有反應");
        }
        //获取想要的信息
        JObject jsonObject = JObject.Parse(content);
        JArray choicesArray = (JArray)jsonObject["choices"];
        JObject firstChoice = (JObject)choicesArray[0];
        JObject messageObject = (JObject)firstChoice["message"];
        string res = messageObject["content"].ToString();
        history?.HistoryMessageList?.Add(new MessageEntry{role = AiRoleEnum.assistant, content = res, CreateTime = DateTime.Now,AiHistoryMessageId = history.Id});

        bool insertStatus = false;
        if(hasValue == false)
        {
            insertStatus = await _db.InsertNav(history).Include(a => a.HistoryMessageList).ExecuteCommandAsync();
        }
        else
        {
            insertStatus = await _db.UpdateNav(history).Include(a => a.HistoryMessageList).ExecuteCommandAsync();
        }
        if (insertStatus != true)
        {
            throw new DomainException("信息插入失败");
        }


        return res;
    }
    /// <summary>
    /// 获取历史信息
    /// </summary>
    /// <returns></returns>
    public async Task<List<MessageEntryDto>> getHistoryMessageAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var history = await _db.Queryable<AiHistoryMessage>()
                               .Where(a => a.userId == userId)
                               .Includes(a => a.HistoryMessageList)
                               .FirstAsync();
        var res = history?.HistoryMessageList?.OrderBy(a => a.CreateTime).ToList().Adapt<List<MessageEntryDto>>();
        return res;
    }

    /// <summary>
    /// 清楚历史信息
    /// </summary>
    /// <returns></returns>
    public async Task<string> clearHistoryMessagAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var res = await _db.Updateable<AiHistoryMessage>()
                           .SetColumns(a => a.IsDeleted == true)
                           .Where(a => a.userId == userId && a.IsDeleted == false)
                           .ExecuteCommandAsync();
        return res != 0 ? "成功删除" : "当前用户没有数据";
    }

    public async Task<string> getAiHttpResponseAsync(string url, Method method, string JsonBody)
    {
        var client = new RestClient(url);
        var request = new RestRequest("", method);

        // 设置请求头部信息
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", "Bearer " + AppSetting.GetValue("AiOption:Key"));

        request.AddJsonBody(JsonBody);

        var response = await client.ExecuteAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new DomainException(response.Content + response.ErrorMessage);
        }
        return response?.Content ?? "0";
    }
}
