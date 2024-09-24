using System;
using rbac.Modals.Enum;
using rbac.Modals.Models;

namespace rbac.CoreBusiness.Vms;

public class MenuVm
{
    /// <summary>
    /// id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（1目录 2菜单 3按钮）
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 路由名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string? Component { get; set; }

    /// <summary>
    /// 重定向
    /// </summary>
    public string? Redirect { get; set; }

    /// <summary>
    /// 权限标识
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 是否内嵌
    /// </summary>
    public bool IsIframe { get; set; }=false;

    /// <summary>
    /// 外链链接
    /// </summary>
    public string? OutLink { get; set; }
    
    /// <summary>
    /// 状态
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 菜单子项
    /// </summary>
    public List<Menu> Children { get; set; } 

}
