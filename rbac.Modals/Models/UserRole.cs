using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_users_roles")]
public class UserRole : ModelBaseId
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public  string UserId { get; set; }
    
    /// <summary>
    /// 角色Id
    /// </summary>
    public  string RoleId { get; set; }
}
