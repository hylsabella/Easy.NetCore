using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.CommonServiceLocator;
using Easy.Common.NetCore.Enums;
using Easy.Common.NetCore.Filters;
using Easy.Common.NetCore.IoC;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Easy.Common.NetCore.Startup
{
    public class AppStartup
    {
        private static bool _isStart;

        public void Start()
        {
            if (EasyIocContainer.Container == null) throw new Exception("请先加载IoC容器");

            _isStart = true;
        }

        public static bool IsStart()
        {
            return _isStart;
        }

        public static void EasyConfigureServices(IServiceCollection services, WebType webType, Action<CookieAuthenticationOptions> cookieOptionsAction = null, params Type[] filters)
        {
            if (webType == WebType.Mvc)
            {
                if (cookieOptionsAction == null) throw new Exception("请先设置Cookie选项！");

                services.AddControllersWithViews().AddRazorRuntimeCompilation();

                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptionsAction);
            }
            else if (webType == WebType.WebApi)
            {
                services.AddControllers();
            }

            var mvcBuilder = services.AddMvc(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
                options.Filters.Add<ModelValidatorFilter>();

                if (webType == WebType.Mvc)
                {
                    options.Filters.Add<IPMvcFilter>();
                }
                else if (webType == WebType.WebApi)
                {
                    options.Filters.Add<IPWebApiFilter>();
                }

                if (filters != null && filters.Length > 0)
                {
                    foreach (var filter in filters)
                    {
                        options.Filters.Add(filter);
                    }
                }
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            })
            .AddControllersAsServices();//注册Controllers到IOC

            mvcBuilder.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //模型验证
            mvcBuilder.AddFluentValidation(x =>
            {
                //如果设置为false，那么FluentValidation是唯一执行的验证库
                x.RunDefaultMvcValidationAfterFluentValidationExecutes = true;

                //自动注册
                var typeInCurrAppDomain = Assembly.GetEntryAssembly().GetTypes().First();
                x.RegisterValidatorsFromAssemblyContaining(typeInCurrAppDomain, lifetime: ServiceLifetime.Singleton);
            });

            //注册gzip压缩
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
            });

            services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
        }

        public static void EasyConfigureContainer(ContainerBuilder builder, IConfiguration configuration, WebType webType, Assembly assembly)
        {
            string nlogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Config/nlog.config");

            new AppStartup()
                .UseNLog(nlogFilePath)
                .InitMachineConfig()
                .InitLimitAttack(webType, assembly)
                .InitMEF()
                .RegConfig(configuration, builder)
                .RegRedisCache(builder)
                .RegExtraIoC(builder);
        }

        public static void EasyConfigure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, WebType webType, bool isHttpsRedirect = false, string mvcDefaultRoute = "{controller=Home}/{action=Index}/{id?}", string webApiName = "")
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (!env.IsDevelopment() && webType == WebType.Mvc)
            {
                string errorRedirect = configuration?["appSettings:ErrorRedirect"];
                errorRedirect = !string.IsNullOrWhiteSpace(errorRedirect) ? errorRedirect.Trim() : "/Home/Error";
                app.UseExceptionHandler(errorRedirect);
            }

            if (isHttpsRedirect)
            {
                app.UseHttpsRedirection();
            }

            //配置中间件以转接 X-Forwarded-For 和 X-Forwarded-Proto 标头
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                   ForwardedHeaders.XForwardedProto
            });

            app.UseRouting();
            app.UseAuthentication();//身份认证
            app.UseAuthorization();//授权
            app.UseResponseCompression();//添加gzip压缩中间件

            if (webType == WebType.Mvc)
            {
                app.UseStaticFiles();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: mvcDefaultRoute?.Trim() ?? "{controller=Home}/{action=Index}/{id?}");
                });
            }
            else if (webType == WebType.WebApi)
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", async context =>
                    {
                        await context.Response.WriteAsync($"WebApi is OK! {webApiName}");
                    });

                    endpoints.MapControllers();
                });
            }

            var autofacContainer = app.ApplicationServices.GetAutofacRoot();
            var serviceLocator = new AutofacServiceLocator(autofacContainer);

            new AppStartup()
                .InitIoC(serviceLocator)
                .CheckRedis()
                .Start();
        }
    }
}