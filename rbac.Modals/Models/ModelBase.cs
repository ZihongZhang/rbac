using System;
using SqlSugar;

namespace rbac.Modals.Models;

/// <summary>
/// 作为主键，框架实体基类Id
/// </summary>
public abstract class ModelBaseId
{
    /// <summary>
    /// guid形式Id
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键Id", IsPrimaryKey = true, IsIdentity = false)]
    public virtual string Id { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// 框架实体基类（根据createtime创建索引）
/// </summary>
[SugarIndex("index_{table}_CT", nameof(CreateTime), OrderByType.Asc)]
[SugarIndex("index_{table}_ID", nameof(Id), OrderByType.Asc)]
public abstract class ModelBase : ModelBaseId, ISoftDeletable
{
    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsNullable = true, IsOnlyIgnoreUpdate = true, InsertServerTime = true)]
    public virtual DateTime CreateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间",  UpdateServerTime = true)]
    public virtual DateTime? UpdateTime { get; set; }=DateTime.Now;

    /// <summary>
    /// 创建者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true)]
    public virtual string? CreateUserId { get; set; }

    ///// <summary>
    ///// 创建者
    ///// </summary>
    //[Newtonsoft.Json.JsonIgnore]
    //[System.Text.Json.Serialization.JsonIgnore]
    //[Navigate(NavigateType.OneToOne, nameof(CreateUserId))]
    //public virtual SysUser CreateUser { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者姓名", Length = 64, IsOnlyIgnoreUpdate = true,IsNullable =true)]
    public virtual string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者Id",IsNullable =true)]
    public virtual string? UpdateUserId { get; set; }

    ///// <summary>
    ///// 修改者
    ///// </summary>
    //[Newtonsoft.Json.JsonIgnore]
    //[System.Text.Json.Serialization.JsonIgnore]
    //[Navigate(NavigateType.OneToOne, nameof(UpdateUserId))]
    //public virtual SysUser UpdateUser { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者姓名", Length = 64,IsNullable =true)]
    public virtual string? UpdateUserName { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除")]
    public virtual bool IsDeleted { get; set; } = false;


    /// <summary>
    /// 并发锁
    /// </summary>
    [SugarColumn(IsEnableUpdateVersionValidation = true,IsNullable = true)]
    public string Ver { get; set; }
}