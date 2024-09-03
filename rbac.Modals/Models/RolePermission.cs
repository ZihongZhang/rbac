using System;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_roles_permissions")]
public class RolePermission :EntityBase
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public string PermissionId { get; set; }
    
    /// <summary>
    /// 角色Id
    /// </summary>
    public string RoleId { get; set; }
}
