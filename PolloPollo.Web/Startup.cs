using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PolloPollo.Entities;
using PolloPollo.Repository;
using PolloPollo.Web.Security;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PolloPollo.Shared;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using PolloPollo.Repository.Utils;
using AspNetCoreRateLimit;

namespace PolloPollo.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public string OpenIdConnectConstants { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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

            services.AddScoped<IPolloPolloContext, PolloPolloContext>();
            services.AddScoped<IImageWriter, ImageWriter>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
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
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "PolloPollo API",
                    Description = "The API for the PolloPollo.org website",
                    License = new License
                    {
                        Name = "Licensed under the MIT License",
                        Url = "https://github.com/pollopollo-org/back-end/blob/develop/LICENSE"
                    },
                    Contact = new Contact
                    {
                        Name = "Github repository",
                        Url = "https://github.com/pollopollo-org/back-end"
                    }

                });

                if (Environment.IsDevelopment())
                {
                    // Security definition and security requirement should only be present in dev environment
                    c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Please enter JWT with Bearer into field. Example: \"Bearer {token}\"",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey"
                    });

                    c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
                    { "Bearer", new string[]{} },
                    });
                }
            });

            // https://github.com/aspnet/Hosting/issues/793
            // the IHttpContextAccessor service is not registered by default.
            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
