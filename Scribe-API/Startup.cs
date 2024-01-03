using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Timers;
using System;
using Microsoft.AspNetCore.Http.Features;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using GSAS_Web.Data;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Stores;
using tusdotnet.Models.Configuration;
using tusdotnet.Interfaces;
using Microsoft.AspNetCore.StaticFiles;

namespace GSAS_Web
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "CorsPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(c =>
            {
                c.AddPolicy(MyAllowSpecificOrigins, builder =>
                {//.WithOrigins("https://www.microsoft.com", "https://www.microsoftonline.com")
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                });
            });
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<GSAS_Context>()
            .AddDefaultTokenProviders();



            services.AddDbContext<GSAS_Context>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("GSAS_Context")));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                });
            services.AddAuthorization(options =>
            {
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, GSAS_Context context)
        {
            //app.UseCors(MyAllowSpecificOrigins);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            string uploadDir = Configuration["UploadDirectory"];
            app.UseTus(httpContext => new DefaultTusConfiguration
            {
                // c:\tusfiles is where to store files
                Store = new TusDiskStore(uploadDir),
                // On what url should we listen for uploads?
                UrlPath = "/api/uploadFile",
                Events = new Events 
                {
                    OnFileCompleteAsync = async eventContext =>
                    {
                        ITusFile file = await eventContext.GetFileAsync();
                    }
                    

                }
            });
            app.UseHttpsRedirection();
            StaticFileOptions option = new StaticFileOptions();
            FileExtensionContentTypeProvider contentTypeProvider = (FileExtensionContentTypeProvider)option.ContentTypeProvider ??
            new FileExtensionContentTypeProvider();

            contentTypeProvider.Mappings.Add(".unityweb", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".mem", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".data", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".memgz", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".datagz", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".unity3dgz", "application/octet-stream");
            contentTypeProvider.Mappings.Add(".jsgz", "application/x-javascript; charset=UTF-8");
            option.ContentTypeProvider = contentTypeProvider;
            app.UseStaticFiles(option);
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles(option);
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
            //var provider = new FileExtensionContentTypeProvider();
            //provider.Mappings.Remove(".unityweb");
            //provider.Mappings.Add(".unityweb", "application/octet-stream");
            //provider.Mappings.Remove(".mem");
            //provider.Mappings.Add(".mem", "application/octet-stream");
            //provider.Mappings.Remove(".data");
            //provider.Mappings.Add(".data", "application/octet-stream");
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    ContentTypeProvider = provider
            //});
        }
    }
}
