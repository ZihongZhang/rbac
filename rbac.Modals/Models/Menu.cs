using System;
using System.ComponentModel.DataAnnotations;
using rbac.Modals.Enum;
using SqlSugar;

namespace rbac.Modals.Models;

public class Menu : ModelBase
{
    /// <summary>
    /// 父Id
    /// </summary>
    [SugarColumn(ColumnDescription = "父Id",IsNullable =true)]
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（1目录 2菜单 3按钮）
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单类型")]
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 路由名称
    /// </summary>
    [SugarColumn(ColumnDescription = "路由名称", Length = 64,IsNullable =true)]
    [MaxLength(64)]
    public string? Name { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    [SugarColumn(ColumnDescription = "路由地址", Length = 128,IsNullable =true)]
    [MaxLength(128)]
    public string? Path { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    [SugarColumn(ColumnDescription = "组件路径", Length = 128,IsNullable =true)]
    [MaxLength(128)]
    public string? Component { get; set; }

    /// <summary>
    /// 重定向
    /// </summary>
    [SugarColumn(ColumnDescription = "重定向", Length = 128,IsNullable =true)]
    [MaxLength(128)]
    public string? Redirect { get; set; }

    /// <summary>
    /// 权限标识
    /// </summary>
    [SugarColumn(ColumnDescription = "权限标识", Length = 128,IsNullable =true)]
    [MaxLength(128)]
    public string? Permission { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单名称", Length = 64,IsNullable =true)]
    [Required, MaxLength(64)]
    public virtual string Title { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [SugarColumn(ColumnDescription = "图标", Length = 128,IsNullable =true)]
    [MaxLength(128)]
    public string? Icon { get; set; }

    /// <summary>
    /// 是否内嵌
    /// </summary>
    [SugarColumn(ColumnDescription = "是否内嵌")]
    public bool IsIframe { get; set; }=false;

    /// <summary>
    /// 外链链接
    /// </summary>
    [SugarColumn(ColumnDescription = "外链链接", Length = 256,IsNullable =true)]
    [MaxLength(256)]
    public string? OutLink { get; set; }
    
    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256,IsNullable =true)]
    [MaxLength(256)]
    public string? Remark { get; set; }

    /// <summary>
    /// 菜单子项
    /// </summary>
    [SugarColumn(IsIgnore = true,IsNullable =true)]
    public List<Menu> Children { get; set; } = new List<Menu>();

    /// <summary>
    /// 与role导航属性
    /// </summary>
    [Navigate(typeof(UserRole),nameof(UserRole.UserId),nameof(UserRole.RoleId))]
    public List<Role>? RoleList { get; set; }
}
