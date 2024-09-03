using System;

using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_permissions")]
public class Permission : EntityBase,ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription ="租户Id")]
    public string? TenantId { get; set ; }
    
    /// <summary>
    /// 与role的导航属性
    /// </summary>
    [Navigate(typeof(RolePermission),nameof(RolePermission.PermissionId),nameof(RolePermission.RoleId))]
    public List<Role>? PermissionList { get; set; }
}
