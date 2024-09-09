
using System.ComponentModel;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using rbac.Infra;
using rbac.Repository.Base;
using rbac.StartupExtensions;

namespace rbac
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 使用 Autofac 作为服务提供程序工厂
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            //配置autofac模块
            builder.Host.ConfigureContainer<ContainerBuilder>(ContainerBuilder=>
            {
                //使用autofac注册配置静态类
                ContainerBuilder.Register(c => new AppSetting(builder.Configuration))
                    .AsSelf()
                    .SingleInstance();

                ContainerBuilder.RegisterModule(new AutofacModuleRegister());
            });

            builder.Services.AddControllers();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //设置Token格式
            builder.Services.AddAuthenticationSetup();

            //设置swagger权限认证
            builder.Services.AddSwaggerGenSetup();

            //将配置文件装到静态类中，从而在静态类中可以获取静态文件 
            builder.Services.AddSingleton(new AppSetting(builder.Configuration));

            //添加sqlsugar设置并创建数据库
            builder.Services.AddSqlsugarSetup();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
