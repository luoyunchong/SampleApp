using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using SampleApi.Auth;
using Serilog;

namespace SampleApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IStorageUserService, StorageUserService>();

        var jwtSettings = JwtSettings.FromConfiguration(configuration);
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = jwtSettings.TokenValidationParameters);

        return services;
    }


    #region Swagger
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        //Swagger重写PascalCase，改成小写开头模式
        services.AddSwaggerGen(options =>
        {
            try
            {
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Assembly.GetName().Name}.xml"), true);
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
            }
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SampleApp - HTTP API",
                Version = "v1",
                Description = "The SampleApp Microservice HTTP API. This is a Data-Driven/CRUD microservice sample"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference()
                            {
                                Id =  "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "JWT授权(数据将在请求头中进行传输) 参数结构: \"Authorization: Bearer {token}\"",
                Name = "Authorization", //jwt默认的参数名称
                In = ParameterLocation.Header, //jwt默认存放Authorization信息的位置(请求头中)
                Type = SecuritySchemeType.ApiKey

            });

        });
        services.AddEndpointsApiExplorer();

        return services;

    }
    #endregion
}
