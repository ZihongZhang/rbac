using System;
using rbac.Modals.AggregateRoots;
using SqlSugar;

namespace rbac.Modals.Models;

[SugarTable("sys_roles")]
public class Role : ISoftDeletable
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public string RoleId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 角色名称
    /// </summary>
    public string? RoleName { get; set; }
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
    /// 父角色ID
    /// </summary>
    public string? ParentRoleId { get; set; } 
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
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;
}
