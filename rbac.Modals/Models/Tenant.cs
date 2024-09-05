using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using rbac.Modals.Enum;
using SqlSugar;

namespace rbac.Modals.Models;

/// <summary>
/// 系统租户表
/// </summary>
[SugarTable("sys_tenants","系统租户表")]
public class Tenant : ModelBase
{
    /// <summary>
    /// 租户名
    /// </summary>
    [SugarColumn(ColumnDescription = "租户名")]
    public string? TenantName { get; set; }
    
    /// <summary>
    /// 租户类型
    /// </summary>
    [SugarColumn(ColumnDescription = "租户类型")]
    public TenantTypeEnum TenantType { get; set; }=TenantTypeEnum.Id;

    /// <summary>
    /// 创建作者
    /// </summary>
    [SugarColumn(ColumnDescription="创建作者" ,IsNullable =true) ]
    public string? CreateUser { get; set; }

    /// <summary>
    /// 主机，暂时不会用到
    /// </summary>
    [SugarColumn(ColumnDescription = "主机", Length = 128,IsNullable =true)]
    [MaxLength(128)]
    public string? Host { get; set; }

    /// <summary>
    /// 数据库类型，暂时不会用到
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库类型",IsNullable =true)]
    public SqlSugar.DbType DbType { get; set; }

    /// <summary>
    /// 数据库连接
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库连接", Length = 256,IsNullable =true)]
    [MaxLength(256)]
    public string? Connection { get; set; }

    /// <summary>
    /// 从库连接/读写分离
    /// </summary>
    [SugarColumn(ColumnDescription = "从库连接/读写分离", ColumnDataType = StaticConfig.CodeFirst_BigString,IsNullable =true)]
    public string? SlaveConnections { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 128,IsNullable = true)]
    [MaxLength(128)]
    public string? Remark { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;    
}
