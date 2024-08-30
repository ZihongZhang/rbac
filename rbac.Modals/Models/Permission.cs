using System;
using rbac.Modals.AggregateRoots;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_permissions")]
public class Permission : ISoftDeletable
{
    public Guid PermissionId { get; set; } = Guid.NewGuid();
    /// <summary>
    /// 权限名称
    /// </summary>
    public string? PermissionName { get; set; }
    /// <summary>
    /// 是否开启
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// 租户id
    /// </summary>
    public Guid TenantId { get; set; }
    /// <summary>
    /// 是否软删除
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// 更新时间 
    /// </summary>
    public DateTime UpdateTime { get; set; }
    /// <summary>
    /// 与role的导航属性
    /// </summary>

    [Navigate(typeof(RolePermission),nameof(RolePermission.PermissionId),nameof(RolePermission.RoleId))]
    public List<Role>? PermissionList { get; set; }

}
