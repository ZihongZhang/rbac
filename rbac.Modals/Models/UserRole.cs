using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_users_roles")]
public class UserRole 
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
}
