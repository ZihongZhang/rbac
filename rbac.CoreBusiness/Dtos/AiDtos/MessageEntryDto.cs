using System;

namespace rbac.CoreBusiness.Dtos.AiDtos;

public class MessageEntryDto
{   
    /// <summary>
    /// 发送信息角色，请求人或者ai
    /// </summary>
    public string role { get; set; }
    /// <summary>
    /// 发送内容
    /// </summary>
    public string? content { get; set; }
}
