using System;
using rbac.Modals.Models.AiRelatedModel;
using SqlSugar;

namespace rbac.Modals.Models;

[SugarTable("ai_history_message")]
public class AiHistoryMessage : ModelBaseId, ISoftDeletable
{
    /// <summary>
    /// 历史信息
    /// </summary>
    [Navigate(NavigateType.OneToMany,nameof(MessageEntry.AiHistoryMessageId))]
    public List<MessageEntry>? HistoryMessageList { get; set; }
    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// 对应用户id
    /// </summary>
    public string userId { get; set; }
}
