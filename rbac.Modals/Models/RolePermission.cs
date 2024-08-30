using System;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_roles_permissions")]
public class RolePermission
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid PermissionRoleId { get; set; }
    /// <summary>
    /// 用户Id
    /// </summary>
    public Guid PermissionId { get; set; }
    /// <summary>
    /// 角色Id
    /// </summary>
    public Guid RoleId { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public Guid CreateTime { get; set; }
    /// <summary>
    /// 创建者Id
    /// </summary>
    public Guid CreateUser { get; set; }
    /// <summary>
    /// 租户Id
    /// </summary>
    public Guid TenantId { get; set; }
    /// <summary>
    /// 是否软删除
    /// </summary>
    public bool IsDeleted { get; set; }

}
