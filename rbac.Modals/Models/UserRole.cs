using System;
using System.ComponentModel.DataAnnotations;
using rbac.Modals.AggregateRoots;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_users_roles")]
public class UserRole : ISoftDeletable
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public  string UserRoleId { get; set; }=Guid.NewGuid().ToString();
    /// <summary>
    /// 用户Id
    /// </summary>
    public  string UserId { get; set; }
    /// <summary>
    /// 角色Id
    /// </summary>
    public  string RoleId { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public  DateTime CreateTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 创建者Id
    /// </summary>
    public  string CreateUser { get; set; }
    /// <summary>
    /// 租户Id
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

}
