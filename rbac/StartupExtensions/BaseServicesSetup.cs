using System;
using System.Reflection;
using System.Text;
using FreeScheduler;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using rbac.Configs;
using rbac.Filters;
using rbac.Infra;
using Serilog;

namespace rbac.StartupExtensions;

public static class BaseServiceSetup
{
    public static IConfiguration _configuration { get; private set; }

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    /// <summary>
    /// 添加鉴权功能
    /// </summary>
    /// <param name="services"></param>
    public static void AddAuthenticationSetup(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.GetValue("JWTSettings:TokenKey")))
                };
            });
    }

    /// <summary>
    /// 添加swagger验证
    /// </summary>
    /// <param name="services"></param>
    public static void AddSwaggerGenSetup(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
            {
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Put Bearer+your token in the box below",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme,Array.Empty<string>()
                    }
                });
            });
    }

    /// <summary>
    /// 添加筛选器
    /// </summary>
    /// <param name="services"></param>
    public static void AddFilterSetup(this IServiceCollection services)
    {
        services.AddControllers(opt =>
        {
            opt.Filters.Add<GlobalExceptionFilter>();
            opt.Filters.Add<GlobalResultFilter>();
        });
    }

    /// <summary>
    /// 添加serilog
    /// </summary>
    /// <param name="services"></param>
    public static void AddSerilog(this IServiceCollection services)
    {
        services.AddLogging(
            loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog();
                }
        );
    }

    /// <summary>
    /// 设置和添加mapster
    /// </summary>
    /// <param name="services"></param>
    public static void AddAndConfigMapster(this IServiceCollection services)
    {
        services.AddMapster();
        MapsterConfig.Configure();
    }
    /// <summary>
    /// 添加跨域功能
    /// </summary>
    /// <param name="services"></param>
    public static void AddCorsPolicy(this IServiceCollection services)
    {
        // var origin = AppSetting.GetValue("AllowedHost");
        var origin = _configuration.GetSection("AllowedHost")
        .Get<string[]>();
        services.AddCors(options =>
        {
            // options.AddPolicy("AllowSpecificOrigin",
            //     builder => builder.WithOrigins(origin)
            //                       .AllowAnyHeader()
            //                       .AllowAnyMethod()
            //                       .AllowCredentials()
            //     );
            options.AddPolicy("AllowSpecificOrigin",
                builder => builder.SetIsOriginAllowed(_ => true)
                                  .AllowAnyHeader()
                                  .AllowAnyMethod()
                                  .AllowCredentials()
                );
        });

    }

    /// <summary>
    /// 添加freescheduler支持
    /// </summary>
    /// <param name="services"></param>
    public static void AddFreeSchedulerScheduler(this IServiceCollection services)
    {
        Scheduler scheduler = new FreeSchedulerBuilder()
            .OnExecuting(task =>
            {
                Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] {task.Topic} 被执行");
                switch (task.Topic)
                {
                    case "武林大会": Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] 已经 被执行"); break;
                    case "攻城活动": Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] {task.Topic} 被执行"); break;
                }
            })
            .Build();
        services.AddSingleton(scheduler);
    }

    // /// <summary>
    // /// 
    // /// </summary>
    // /// <param name="services"></param>
    // public static void AddQuartz(this IServiceCollection services)
    // {
    //     services.AddQuartz(q =>
    //         {
    //         	//使用持久化存储
    //             q.UsePersistentStore(s =>
    //             {
    //                 s.UseProperties = true;
    //                 s.RetryInterval = TimeSpan.FromSeconds(15);
    //                 s.UsePostgres();
    //                 s.UseJsonSerializer();
    //                 s.UseClustering(c =>
    //                 {
    //                     c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
    //                     c.CheckinInterval = TimeSpan.FromSeconds(10);
    //                 });
    //             });
    //             q.UseMicrosoftDependencyInjectionJobFactory();
    //             q.UseDefaultThreadPool(tp =>
    //             {
    //                 tp.MaxConcurrency = 10;
    //             });

	// 			//添加一个循环任务
    //             JobDataMap jobDataMap = new JobDataMap();
    //             jobDataMap.Put("haha", "hahahaha");
    //             q.ScheduleJob<FirstQuartzJob>(trigger => trigger
    //             .WithIdentity("Combined Configuration Trigger")
    //             .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(7)))
    //             .WithCronSchedule("0/1 * * * * ?")
    //             .WithDescription("my awesome trigger configured for a job with single call")
    //             .UsingJobData(jobDataMap)
    //             );
    //         });
    // }




}
