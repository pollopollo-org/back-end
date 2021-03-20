using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PolloPollo.Entities;
using PolloPollo.Web.Security;
using Swashbuckle.AspNetCore.SwaggerGen;
using PolloPollo.Shared;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AspNetCoreRateLimit;
using PolloPollo.Services.Utils;
using PolloPollo.Services;
using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace PolloPollo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public string OpenIdConnectConstants { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddMvcCore()
    .AddApiExplorer();
            services.AddOptions();
            services.AddDbContext<PolloPolloContext>(options => options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
            services.AddSingleton(_ => new HttpClient(handler) { BaseAddress = new UriBuilder("http", "localhost", 8004).Uri });
            services.AddScoped<IPolloPolloContext, PolloPolloContext>();
            services.AddScoped<IEmailClient, EmailClient>();
            services.AddScoped<IImageWriter, ImageWriter>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<IDonorRepository, DonorRepository>();
            var appSettingsSection = Configuration.GetSection("Authentication");
            services.Configure<SecurityConfig>(appSettingsSection);
            services.AddCors();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = !Environment.IsDevelopment();
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = Configuration["Authentication:Secret"].ToSymmetricSecurityKey(),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "PolloPollo API",
                    Description = "The API for the PolloPollo.org website",
                    License = new OpenApiLicense
                    {
                        Name = "Licensed under the MIT License",
                        Url = new Uri("https://github.com/pollopollo-org/back-end/blob/develop/LICENSE")
                    },
                    Contact = new OpenApiContact
                    {
                        Name = "Github repository",
                        Url = new Uri("https://github.com/pollopollo-org/back-end")
                    }

                });

                if (Environment.IsDevelopment())
                {
                    // Security definition and security requirement should only be present in dev environment
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                        {
                        new OpenApiSecurityScheme{
                        Reference = new OpenApiReference{
                        Id = "Bearer", //The name of the previously defined security scheme.
                        Type = ReferenceType.SecurityScheme
                        }
                    },new List<string>()
                }
                });
                }
            });

            // https://github.com/aspnet/Hosting/issues/793
            // the IHttpContextAccessor service is not registered by default.
            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // inject counter and rules distributed cache stores
            services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var swaggerPath = "/swagger/v1/swagger.json";
            var swaggerName = "PolloPollo API V1";

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(swaggerPath, swaggerName);

                    // Sets swagger documentation to domain root
                    // domain/index.html
                    c.RoutePrefix = string.Empty;
                });
            }
            else
            {
                app.UseHsts();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(swaggerPath, swaggerName);

                    // Sets swagger documentation to domain root
                    // domain/index.html
                    c.RoutePrefix = string.Empty;

                    // Disables Try It Out for production 
                    c.SupportedSubmitMethods();
                });
            }


            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "static")),
                RequestPath = "/static"
            });

            app.UseSwagger();

            app.UseIpRateLimiting();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
