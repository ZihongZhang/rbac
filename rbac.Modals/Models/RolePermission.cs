using System;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_roles_permissions")]
public class RolePermission
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public string PermissionRoleId { get; set; } =Guid.NewGuid().ToString();
    /// <summary>
    /// 用户Id
    /// </summary>
    public string PermissionId { get; set; }
    /// <summary>
    /// 角色Id
    /// </summary>
    public string RoleId { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 创建者Id
    /// </summary>
    public DateTime CreateUser { get; set; }=DateTime.Now;
    /// <summary>
    /// 租户Id
    /// </summary>
    public string TenantId { get; set; }
    /// <summary>
    /// 是否软删除
    /// </summary>
    public bool IsDeleted { get; set; }

}
