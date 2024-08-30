using System;
using rbac.Modals.AggregateRoots;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_users")]
public class User : ISoftDeletable
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public Guid UserId { get; set; } = Guid.NewGuid();
    /// <summary>
    /// 租户Id
    /// </summary>
    public Guid TenentId { get; set; }
    /// <summary>
    /// 登录用户名
    /// </summary>
    public string Username { get; set; }= null!;
    /// <summary>
    /// 登录密码
    /// </summary>
    public string Password { get; set; } = null!; // 注意：实际应用中应对密码进行哈希处理
    /// <summary>
    /// 用户邮箱不可为空
    /// </summary>
    public string Email { get; set; }= null!;
    /// <summary>
    /// 用户手机
    /// </summary>
    public string? mobile { get; set; }
    /// <summary>
    /// 是否被禁用
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// 是否被软删除
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime Updateime { get; set; }
    /// <summary>
    /// 更新人员
    /// </summary>
    public Guid UpadateUser { get; set; }
    /// <summary>
    /// 与role导航属性
    /// </summary>
    [Navigate(typeof(UserRole),nameof(UserRole.UserId),nameof(UserRole.RoleId))]
    public List<Role>? RoleList { get; set; }
}
