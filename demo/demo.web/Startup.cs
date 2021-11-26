using demo.web.Behavior;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RequestTraceKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace demo.web
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
            services.AddControllers();
            services.AddRazorPages();
            //services.AddRequestTraceService(options =>
            //{
            //    //数据库链接
            //    options.ConnectionString = "Server=192.168.200.170;Port=3306;Initial Catalog = Trace;Uid = root;Pwd =123456;Allow User Variables=true;";
            //    options.PageStayTimeRules = new List<string> { "/" };//需要统计停留时间的页面，模糊匹配
            //});            
            services.AddRequestTraceService<MyTraceBehavior>(options =>
            {
                //数据库链接
                options.ConnectionString = "Server=192.168.200.170;Port=3306;Initial Catalog = Trace;Uid = root;Pwd =123456;Allow User Variables=true;";
                options.PageStayTimeRules = new List<string> { "/" };//需要统计停留时间的页面，模糊匹配
            });
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
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            //配置trace.js中间件
            app.UseRequestTraceJS();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
