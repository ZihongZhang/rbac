using System;
using rbac.Modals.AggregateRoots;
using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_tenants")]
public class Tenant : ISoftDeletable
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public Guid TenantId { get; set; } =  Guid.NewGuid();
    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// 创建人
    /// </summary>
    public Guid CreateUser { get; set; }
    
}
