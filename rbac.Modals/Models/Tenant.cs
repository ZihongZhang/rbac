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
    [SugarColumn(IsPrimaryKey = true)]
    public string TenantId { get; set; } =  Guid.NewGuid().ToString();
    /// <summary>
    /// 租户名
    /// </summary>
    public string TenantName { get; set; }
    /// <summary>
    /// 是否被禁用
    /// </summary>
    public bool IsEnabled { get; set; }=true;
    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }=DateTime.Now;
    /// <summary>
    /// 创建人
    /// </summary>
    public  string CreateUser { get; set; }
    /// <summary>
    /// 更新时间 
    /// </summary>
    public DateTime UpdateTime { get; set; }=DateTime.Now;
    
}
