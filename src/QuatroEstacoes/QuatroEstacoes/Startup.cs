using Blazor.WeatherWidget;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using QuatroEstacoes.Data;
using QuatroEstacoes.Service.Imp;
using QuatroEstacoes.Service;

namespace QuatroEstacoes
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWeatherWidgetServices(Configuration);

            // Add services to the container.
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
            services.AddMudServices();
            services.AddSingleton<IMessageService, MessageService>();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            var messageservice = app.ApplicationServices.GetService<IMessageService>();

            ServerMQTT.InicializaMQTT(messageservice);
        }
    }
}
