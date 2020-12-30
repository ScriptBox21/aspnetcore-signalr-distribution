using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using AzureSignalRChatApp.Hubs;

namespace AzureSignalRChatApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //CORS 추가
            services.AddCors(options =>
            {
                options.AddPolicy("AzureSignaRChatAppCORSPolicy",
                builder =>
                {
                    //접속대상 URL CORS 허용
                    builder.WithOrigins("http://localhost:62269", "https://localhost:44364/", "http://chat20.msoftware.co.kr")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            services.AddControllersWithViews();

            ///HttpContext DI처리
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //AzureSignalR() 연결정보조회
            string azureSignalRConString = Configuration.GetConnectionString("AzureSignalRConnection");

            //AddSignalR().AddAzureSignalR() 서비스추가
            services.AddSignalR().AddAzureSignalR(azureSignalRConString);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseFileServer();
            app.UseRouting();

            //Cors설정
            app.UseCors("AzureSignaRChatAppCORSPolicy");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                //모든 MVC컨트롤러 CORS 적용하기
                endpoints.MapControllers().RequireCors("AzureSignaRChatAppCORSPolicy");

                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}
