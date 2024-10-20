using System;
using rbac.CoreBusiness.Dtos.AiDtos;
using rbac.Modals.Models.AiRelatedModel;

namespace rbac.CoreBusiness.Qms.AiQms;

public class AiRequestBody
{
    /// <summary>
    /// 模型类型
    /// </summary>
    public string? model { get; set; }
    /// <summary>
    /// 用户之前使用的历史数据
    /// </summary>
    public  List<MessageEntryDto>? messages { get; set; }
    /// <summary>
    /// 是否使用流模式
    /// </summary>
    public bool stream { get; set; }
}
