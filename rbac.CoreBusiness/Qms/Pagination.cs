using System;

namespace rbac.CoreBusiness.Qms;

public class Pagination
{
    /// <summary>
    /// 页码
    /// </summary>
    public int PageNum { get; set; }

    /// <summary>
    /// 显示数据条数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否默认使用升序排列
    /// </summary>
    public bool IsAscending { get; set; } = true;
}
