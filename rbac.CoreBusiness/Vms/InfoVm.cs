using System;
using rbac.Modals.Enum;

namespace rbac.CoreBusiness.Vms;

public class InfoVm
{
     /// <summary>
    /// 用户id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 登录用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 用户邮箱不可为空
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// 用户手机
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;
    
    /// <summary>
    /// 租户id
    /// </summary>
    public string TenantId { get; set; } = "0" ; 

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<RoleVm> RoleList { get; set; } = new List<RoleVm>();

}
