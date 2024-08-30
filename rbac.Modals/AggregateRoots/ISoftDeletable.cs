using System;

namespace rbac.Modals.AggregateRoots;
/// <summary>
/// 软删除标记
/// </summary>
public interface ISoftDeletable
{
    public bool IsDeleted { get; set; }
}
