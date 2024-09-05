using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace rbac.StartupExtensions;

public static class AuthenticationSetup
{
    // public static void AddAuthentication(this IServiceCollection services)
    // {
    //     services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //         .AddJwtBearer(opt=>
    //         {
    //             opt.TokenValidationParameters =new TokenValidationParameters
    //             {
    //                 ValidateIssuer=false,
    //                 ValidateAudience=false,
    //                 ValidateLifetime=
    //                     new SymmetricSecurityKey(Encoding.UTF8.GetBytes())
    //             };
    //         });
    // }
}
