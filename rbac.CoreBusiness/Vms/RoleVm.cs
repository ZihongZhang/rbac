using System;
using rbac.Modals.Enum;

namespace rbac.CoreBusiness.Vms;

public class RoleVm
{
    public string? Id { get; set; }
    /// <summary>
    /// 角色名称
    /// </summary>
    public string? RoleName { get; set; }  

    /// <summary>
    /// 父角色ID
    /// </summary>
    public string? ParentRoleId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

}
