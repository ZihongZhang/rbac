using System;
using System.Configuration;
using System.Reflection;
using Autofac;
using rbac.CoreBusiness.Services;
using rbac.Infra;
using rbac.Infra.FunctionalInterfaces;
using rbac.Infra.Helper;
using rbac.Repository.Base;
using SqlSugar;

namespace rbac.StartupExtensions;

public class AutofacModuleRegister : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //注册仓储
        builder.RegisterGeneric(typeof(Repository<>)).InstancePerLifetimeScope();

         builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
        
        // builder.RegisterType<UserServices>().InstancePerLifetimeScope();

        var basePath = AppContext.BaseDirectory;

        var servicesDllFile = Path.Combine(basePath, "rbac.CoreBusiness.dll");

        //获得全部程序集
        var allAssembly = AssemblyHelper.GetAllLoadAssembly();

        // 获取 Service.dll 程序集服务，并注册
         var assemblyServices = Assembly.LoadFrom(servicesDllFile);

        //scoped生命周期注入   
        builder.RegisterAssemblyTypes(assemblyServices)
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IScoped)))
            .AsSelf()
            .InstancePerLifetimeScope();

        //单例生命周期注入
        builder.RegisterAssemblyTypes(assemblyServices)
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(ISingleton)))
            .AsSelf()
            .SingleInstance();
        
        //瞬时生命周期注入
        builder.RegisterAssemblyTypes(assemblyServices)
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(ITransient)))
            .AsSelf()
            .InstancePerDependency();            
    }
}
