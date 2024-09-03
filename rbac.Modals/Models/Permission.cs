using System;
using rbac.Modals.AggregateRoots;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_permissions")]
public class Permission : ISoftDeletable
{
    /// <summary>
    /// 权限Id
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public string PermissionId { get; set; } = Guid.NewGuid().ToString();
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
    public  string TenantId { get; set; }
    /// <summary>
    /// 是否软删除
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// 更新时间 
    /// </summary>
    public DateTime UpdateTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 与role的导航属性
    /// </summary>

    [Navigate(typeof(RolePermission),nameof(RolePermission.PermissionId),nameof(RolePermission.RoleId))]
    public List<Role>? PermissionList { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;
}
