using System;
using System.ComponentModel.DataAnnotations;
using rbac.Modals.Enum;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_users")]
public class User : ModelBase,ITenantIdFilter
{
    /// <summary>
    /// 登录用户名
    /// </summary>
    public  string Username { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    public  string Password { get; set; }  // 注意：实际应用中应对密码进行哈希处理

    /// <summary>
    /// 用户邮箱不可为空
    /// </summary>
    public  string Email { get; set; }

    /// <summary>
    /// 用户手机
    /// </summary>
    [SugarColumn(IsNullable = true)]//可以为NULL
    public string Mobile { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256,IsNullable =true)]
    [MaxLength(256)]
    public string? Remark { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription ="租户Id")]
    public string? TenantId { get; set ; }
    
    /// <summary>
    /// 与role导航属性
    /// </summary>
    [Navigate(typeof(UserRole),nameof(UserRole.UserId),nameof(UserRole.RoleId))]
    public List<Role>? RoleList { get; set; }
}
