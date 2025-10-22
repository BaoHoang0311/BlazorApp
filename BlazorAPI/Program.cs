﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace BlazorAPI
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Value?.Split(';') ?? [];

            builder.Services.AddCors(p => p.AddPolicy("LivethereCMSPublicCors", builder =>
            {
                builder.WithOrigins(allowedOrigins).SetIsOriginAllowedToAllowWildcardSubdomains().AllowAnyHeader().AllowAnyMethod();
            }));

            builder.Services.AddControllers(options =>
            {
                //    options.ReturnHttpNotAcceptable = true;
            })
            .AddXmlDataContractSerializerFormatters();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Bao API 127 v1",
                    Version = "v1",
                    Description = "An API to demo upload document",
                    Contact = new OpenApiContact
                    {
                        Name = "nglehoangbao",
                        Email = "nglehoangbao@gmail.com",
                        Url = new Uri("https://twitter.com/jwalkner"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License ne",
                        Url = new Uri("https://example.com/license"),
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                #region Bearer Authen 
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                        "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                        "Example: \"12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header
                            },
                            new List<string>()
                        }
                    });
                #endregion
            });

            #region Bearer Auth
            //builder.Services.AddAuthentication(x =>
            //{
            //    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //.AddJwtBearer(x =>
            //{
            //     x.SaveToken = true;
            //     x.RequireHttpsMetadata = false;
            //     x.TokenValidationParameters = new TokenValidationParameters
            //     {
            //         ValidateIssuerSigningKey = true,
            //         IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("this_is_my_secret_keyyyyyyyyyyyyyyyyyyyyyyyyyyyy")),
            //         ValidateIssuer = false,
            //         ValidateAudience = false
            //     };

            //     #region Check Redis
            //     //x.Events = new JwtBearerEvents
            //     //{
            //     //    OnTokenValidated = async context =>
            //     //    {
            //     //        var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            //     //        var db = redis.GetDatabase();

            //     //        var token = context.SecurityToken.RawData;

            //     //        // Giả sử bạn lưu token hợp lệ vào Redis theo key: "valid_tokens:{token}"
            //     //        bool exists = await db.KeyExistsAsync($"valid:tokens:{token}");
            //     //        if (!exists)
            //     //        {
            //     //            context.Fail("Token không tồn tại hoặc đã bị thu hồi.");
            //     //        }
            //     //    },
            //     //    OnAuthenticationFailed = context =>
            //     //    {
            //     //        Console.WriteLine("JWT Authentication Failed: " + context.Exception.Message);
            //     //        return Task.CompletedTask;
            //     //    }
            //     //};
            //     #endregion
            //});
            #endregion

            #region Custome Auth 
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = "GoogleAccessToken";
            })
            .AddScheme<AuthenticationSchemeOptions, GoogleAccessTokenAuthenticationHandler>("GoogleAccessToken", null);
            #endregion

            builder.Services.AddHttpContextAccessor();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();


            builder.Services.AddHttpClient();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bao API 127 v1");
            });

            app.UseCors("LivethereCMSPublicCors");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
