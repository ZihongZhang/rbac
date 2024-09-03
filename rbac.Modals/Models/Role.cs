using System;

using SqlSugar;

namespace rbac.Modals.Models;

[SugarTable("sys_roles")]
public class Role : EntityBase,ITenantIdFilter
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string? RoleName { get; set; }  

    /// <summary>
    /// 父角色ID
    /// </summary>
    public string? ParentRoleId { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription ="租户Id")]
    public string? TenantId { get; set ; }

    /// <summary>
    /// 与user导航属性
    /// </summary>
    [Navigate(typeof(UserRole),nameof(UserRole.RoleId),nameof(UserRole.UserId))]
    public List<User>? RoleList { get; set; }

    /// <summary>
    /// 与permission导航属性
    /// </summary>
    [Navigate(typeof(RolePermission),nameof(RolePermission.RoleId),nameof(RolePermission.PermissionId))]
    public List<Permission>? PermissionList { get; set; }
}
