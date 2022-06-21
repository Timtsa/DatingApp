using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extansions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentityCore<AppUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredUniqueChars = 0;
                opt.Password.RequireDigit = false;

            })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleValidator<RoleValidator<AppRole>>()
            .AddEntityFrameworkStores<DataContext>();


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                       .AddJwtBearer(options =>
                       {
                           options.TokenValidationParameters = new TokenValidationParameters
                           {
                               ValidateIssuerSigningKey = true,
                               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                               ValidateIssuer = false,
                               ValidateAudience = false
                           };

                           options.Events = new JwtBearerEvents
                           {
                            OnMessageReceived=context=>{
                                var accessToken =context.Request.Query["access_token"];
                                var path  =context.HttpContext.Request.Path;
                                if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                                {
                                    context.Token =accessToken;
                                }
                                 return Task.CompletedTask;
                            }
                           };
                       });
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("RequireAdminRole",policy=>{
                    policy.RequireRole("Admin");
                });
                opt.AddPolicy("PhotoModerateRole",policy=>{
                    policy.RequireRole(new [] {"Admin","Moderator"});
                } );
            });

            return services;
        }
    }
}