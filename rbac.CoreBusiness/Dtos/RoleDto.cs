using System;
using rbac.Modals.Enum;

namespace rbac.CoreBusiness.Dtos;

public class RoleDto
{
    /// <summary>
    /// 角色名
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

    /// <summary>
    /// 菜单ID的list
    /// </summary>
    /// <value></value>
    public List<string> MenuIdList { get; set; } = new List<string>();

}
