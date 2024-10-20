using System;
using rbac.Modals.Enum.AiRelatedEnum;
using SqlSugar;

namespace rbac.Modals.Models.AiRelatedModel;
[SugarTable("ai_history_message_entry")]
public class MessageEntry: ModelBaseId
{
    /// <summary>
    /// 发送信息角色，请求人或者ai
    /// </summary>
    public AiRoleEnum role { get; set; }
    /// <summary>
    /// 发送内容
    /// </summary>
    public string? content { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsNullable = true, IsOnlyIgnoreUpdate = true, InsertServerTime = true)]
    public DateTime? CreateTime { get; set; }
    /// <summary>
    /// 对应ai信息
    /// </summary>
    public string? AiHistoryMessageId { get; set;}

}
