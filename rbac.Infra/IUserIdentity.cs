using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using rbac.Infra.Exceptions;

namespace rbac.Infra;

public interface IUserIdentity
{
    /// <summary>
    /// 用户id
    /// </summary>
    public string UserId { get; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public string TenantId { get; }
}

public class UserIdentity : IUserIdentity
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserIdentity> _logger;

    public UserIdentity(IHttpContextAccessor accessor, ILogger<UserIdentity> logger)
    {
        _httpContextAccessor = accessor;
        _logger = logger;
    }


    private  UserInfo GetUserInfo()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        var tenantId = _httpContextAccessor.HttpContext?.User.FindFirst("tenantId")?.Value;
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(tenantId))
        {
            _logger.LogError("获取用户信息失败");
            throw new DomainException("获取用户信息失败");
        }

        return new UserInfo{UserId = userId, UserName = userName, Email = email, TenantId = tenantId};
    }
    public string UserId => GetUserInfo().UserId ;
    public string UserName  => GetUserInfo().UserName;
    public string Email  => GetUserInfo().Email;
    public string TenantId  => GetUserInfo().TenantId;
}


public class UserInfo
{
    /// <summary>
    /// 用户id
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public string TenantId { get; set; }
}
