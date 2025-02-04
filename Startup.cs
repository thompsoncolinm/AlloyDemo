using AlloyDemo.Extensions;
using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using AlloyDemo.Features.RegisterPersonas;
using System.Net;


namespace AlloyDemo;

public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;

    public Startup(IWebHostEnvironment webHostingEnvironment)
    {
        _webHostingEnvironment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData(
                "DataDirectory",
                Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data")
            );

            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddAlloy()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Startup>()
            .AddHttpContextAccessor();

        // Required by Wangkanai.Detection
        services.AddDetection();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Required by Wangkanai.Detection
        app.UseDetection();
        app.UseSession();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

            // Replace HttpContext.Current with your own check using httpContextAccessor
            app.UseRegisterPersonas(() =>
            {
                var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
                return ipAddress != null && IPAddress.IsLoopback(ipAddress);
            });

        app.UseEndpoints(endpoints =>
        {

            endpoints.MapControllerRoute(
                name: "register-personas",
                pattern: "RegisterPersonas",
                defaults: new { controller = "RegisterPersonas", action = "Index" }
            );

            endpoints.MapContent();
        });
    }
}
