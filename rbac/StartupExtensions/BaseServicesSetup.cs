using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using rbac.Filters;
using rbac.Infra;
using Serilog;

namespace rbac.StartupExtensions;

public static class BaseServiceSetup
{
    /// <summary>
    /// 添加鉴权功能
    /// </summary>
    /// <param name="services"></param>
    public static void AddAuthenticationSetup(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt=>
            {
                opt.TokenValidationParameters =new TokenValidationParameters
                {
                    ValidateIssuer=false,
                    ValidateAudience=false,
                    ValidateLifetime= true,
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

    
}
