using System;
using rbac.Modals.Enum;
using rbac.Modals.Models;

namespace rbac.CoreBusiness.Dtos;

public class UserDto
{
    /// <summary>
    /// 登录用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 登录密码 已使用MD5加密
    /// </summary>
    public required string Password { get; set; }  // 注意：实际应用中应对密码进行哈希处理

    /// <summary>
    /// 用户邮箱不可为空
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// 用户手机
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;
    
    /// <summary>
    /// 租户id
    /// </summary>
    public string TenantId { get; set; } = "0" ;

    /// <summary>
    /// 角色Id的list
    /// </summary>
    public List<string> RoleIdList  { get; set; } = new List<string> (); 
}
