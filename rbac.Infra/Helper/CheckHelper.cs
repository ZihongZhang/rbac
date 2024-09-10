using System;
using rbac.Infra.Exceptions;

namespace rbac.Infra.Helper;

public static class CheckHelper
{
    public static T NotNull<T>(T? obj) where T : class
    {
        if (obj == null) throw new DomainException("传入为空值");
        return obj;
    }

    public static T NotNull<T>(T? obj, string objName) where T : class
    {
        if (obj == null) throw new DomainException($"传入的{objName}为空值");
        return obj;
    }
}
