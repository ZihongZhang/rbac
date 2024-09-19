using System;
using rbac.Modals.Enum;
using SqlSugar;

namespace rbac.Modals.Models;

[SugarTable("sys_roles")]
public class Role : ModelBase
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string? RoleName { get; set; }  

    /// <summary>
    /// 父角色ID
    /// </summary>
    public string? ParentRoleId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 与user导航属性
    /// </summary>
    [Navigate(typeof(UserRole),nameof(UserRole.RoleId),nameof(UserRole.UserId))]
    public List<User> UserList { get; set; }

    /// <summary>
    /// 与Menu的导航属性
    /// </summary>
    [Navigate(typeof(RoleMenu),nameof(RoleMenu.RoleId),nameof(RoleMenu.MenuId))]
    public List<Menu> MenuList{ get; set; }
}
