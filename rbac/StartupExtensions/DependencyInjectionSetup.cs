using System;
using rbac.Infra;

namespace rbac.StartupExtensions;

public static class DependencyInjectionSetup
{
    public static void AddDependencySetup(this WebApplication builder)
    {
        var app=new AppSetting(builder.Configuration);
    }
}
