using System.Text;
using System.ComponentModel;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using rbac.Infra;
using rbac.Repository.Base;
using rbac.StartupExtensions;
using Serilog;
using FreeScheduler;

namespace rbac
{

    public class Program
    {
        public static void Main(string[] args)
        {

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                //初始化静态类
                BaseServiceSetup.Initialize(builder.Configuration);

                // 使用 Autofac 作为服务提供程序工厂
                builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

                //配置autofac模块
                builder.Host.ConfigureContainer<ContainerBuilder>(ContainerBuilder =>
                {
                    //使用autofac注册配置静态类
                    ContainerBuilder.Register(c => new AppSetting(builder.Configuration))
                        .AsSelf()
                        .SingleInstance();
                    //替换原生工厂
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

                //添加筛选器
                builder.Services.AddFilterSetup();
                
                //增加定时任务
                builder.Services.AddFreeSchedulerScheduler();

                //使用serilog替换原生log
                Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(AppSetting.Configuration)
                            .WriteTo.Console()
                            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                            .CreateLogger();
                builder.Services.AddSerilog();

                builder.Services.AddAndConfigMapster();

                //从配置文件中读取跨域请求
                builder.Services.AddCorsPolicy();

                var app = builder.Build();

                // Configure the HTTP request pipeline.

                app.UseSwagger();
                app.UseSwaggerUI();

                
           
                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.UseCors("AllowSpecificOrigin");

                app.MapControllers();

                app.UseFreeSchedulerUI("/freescheduler/");

                app.Run();
            }
            catch (System.Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
