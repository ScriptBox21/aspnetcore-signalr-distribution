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
            //CORS �߰�
            services.AddCors(options =>
            {
                options.AddPolicy("ChatRedisAppCORSPolicy",
                builder =>
                {
                    //���Ӵ�� URL CORS ���
                    //builder.WithOrigins("http://localhost:49398", "https://localhost:44322", "http://localhost:5005")
                    //       .AllowAnyMethod()
                    //       .AllowAnyHeader()
                    //       .AllowCredentials();

                    //��� ������ġ ���� CORS ���ó��
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddControllersWithViews();


            //�л꼭�� ȯ�濡�� �ε�뷱�� ������ ���� �����߰�:Nginx �ε�뷱�� ���� 
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });


            ///HttpContext DIó��
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

      
            //Redis ���Ṯ�ڿ� ��ȸ
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


            //���Ͻ� �ε�뷱�̼���
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


            //Cors����
            app.UseCors("ChatRedisAppCORSPolicy");


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                //��� MVC��Ʈ�ѷ� CORS �����ϱ�
                endpoints.MapControllers().RequireCors("ChatRedisAppCORSPolicy");

                //SignalR ��� Ŭ���� �߰�
                endpoints.MapHub<ChatHub>("/chatHub");
            });


        }


    }
}
