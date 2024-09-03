using System;

using SqlSugar;

namespace rbac.Modals.Models;
[SugarTable("sys_tenants")]
public class Tenant : EntityBase
{
    /// <summary>
    /// 租户名
    /// </summary>
    public string TenantName { get; set; }    
}
