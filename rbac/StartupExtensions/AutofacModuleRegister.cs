using System;
using System.Configuration;
using Autofac;
using rbac.CoreBusiness.Services;
using rbac.Infra;
using rbac.Repository.Base;
using SqlSugar;

namespace rbac.StartupExtensions;

public class AutofacModuleRegister : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(Repository<>)).InstancePerLifetimeScope();
        builder.RegisterType<UserServices>().InstancePerLifetimeScope();                
    }
}
