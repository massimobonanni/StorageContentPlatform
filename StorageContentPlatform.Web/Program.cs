using StorageContentPlatform.Web.Interfaces;
using StorageContentPlatform.Web.Services;

namespace StorageContentPlatform.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile(
                     "appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile(
                    "appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                config.AddJsonFile(
                    "appsettings.local.json", optional: true, reloadOnChange: true);
                config.AddJsonFile(
                    "appsettings.local.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            });
            
            // Add services to the container.
            builder.Services.AddScoped<IContentsService, StorageContentsService>();
            builder.Services.AddScoped<IStatisticsService, StorageStatisticsService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}