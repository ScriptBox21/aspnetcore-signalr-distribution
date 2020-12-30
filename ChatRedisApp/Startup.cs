using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Reflection;
using System.IO;
using System.Net;

using System.Reflection.Emit;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.Extensions.Options;
using MessagePack;
using StackExchange.Redis;

using ChatRedisApp.Hubs;
using ChatRedisApp.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading;

namespace ChatRedisApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            //CORS 추가
            services.AddCors(options =>
            {
                options.AddPolicy("ChatRedisAppCORSPolicy",
                builder =>
                {
                    //접속대상 URL CORS 허용
                    //builder.WithOrigins("http://localhost:49398", "https://localhost:44322", "http://localhost:5005")
                    //       .AllowAnyMethod()
                    //       .AllowAnyHeader()
                    //       .AllowCredentials();

                    //모든 접근위치 접속 CORS 허용처리
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddControllersWithViews();


            //분산서버 환경에서 로드밸런싱 지원을 위한 설정추가:Nginx 로드밸런싱 지원 
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });


            ///HttpContext DI처리
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

      
            //Redis 연결문자열 조회
            string redisConString = Configuration.GetConnectionString("RedisNoSQLConnection");

            //
            services.AddSignalR().AddStackExchangeRedis(redisConString, options =>
            {
                options.Configuration.ChannelPrefix = "ChatRedisApp";
            });

            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                var config = ConfigurationOptions.Parse(redisConString, true);
                return ConnectionMultiplexer.Connect(config);
            });

        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            //프록시 로드밸런싱설정
            app.UseForwardedHeaders();



            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();


            //Cors설정
            app.UseCors("ChatRedisAppCORSPolicy");


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                //모든 MVC컨트롤러 CORS 적용하기
                endpoints.MapControllers().RequireCors("ChatRedisAppCORSPolicy");

                //SignalR 허브 클래스 추가
                endpoints.MapHub<ChatHub>("/chatHub");
            });


        }


    }
}
