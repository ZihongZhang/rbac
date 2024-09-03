using System;

namespace rbac.Modals.Models;
/// <summary>
/// 软删除标记
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDeleted { get; set; }
}

/// <summary>
/// 租户id
/// </summary>
public interface ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public string? TenantId { get; set; }
}

/// <summary>
/// 机构Id过滤器
/// </summary>
public interface IOrgIdFilter
{
    /// <summary>
    /// 创建者部门Id
    /// </summary>
    long? CreateOrgId {get;set;}
}


