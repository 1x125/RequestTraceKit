using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public static class TraceServiceCollectionExtensions
    {
        /// <summary>
        /// 默认注入
        /// </summary>
        /// <param name="services"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static RequestTraceBuilder AddRequestTraceService(this IServiceCollection services, Action<TraceRecordOption> opt)
        {
            services.AddSingleton<ITraceBehavior, TraceBehavior>();
            return AddBaseService(services, opt);
        }

        /// <summary>
        /// 自定义统计行为注入
        /// </summary>
        /// <typeparam name="T">ITraceBehavior实现</typeparam>
        /// <param name="services"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static RequestTraceBuilder AddRequestTraceService<T>(this IServiceCollection services, Action<TraceRecordOption> opt) where T : ITraceBehavior
        {
            services.AddSingleton(typeof(ITraceBehavior), typeof(T));
            return AddBaseService(services, opt);
        }

        /// <summary>
        /// 注入自定义存储方式
        /// </summary>
        /// <typeparam name="T">存储方式接口实现</typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static RequestTraceBuilder AddCustomStorageType<T>(this RequestTraceBuilder builder) where T : IRequestTraceHandler
        {
            var services = builder.Services;
            var options = builder.Options;
            services.AddSingleton(typeof(IRequestTraceHandler), typeof(T));
            var serviceProvider = services.BuildServiceProvider();
            var traceHandler = serviceProvider.GetService<IRequestTraceHandler>();
            TaskPool.Instance.RegisterQueue<RequestTraceRecord>(traceHandler.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
            TaskPool.Instance.RegisterQueue<PageStayRecord>(traceHandler.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
            TaskPool.Instance.RegisterQueue<ElementClickRecord>(traceHandler.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
            return builder;
        }

        public static IApplicationBuilder UseRequestTraceJS(this IApplicationBuilder builder)
        {
            builder.Map("/trace.js", app =>
            {
                app.Run(async context =>
                {
                    var url = context.Request.Headers["Referer"].ToString();
                    var tagNameList = new List<string>();
                    var rootUrl = context.Request.Scheme + "://" + context.Request.Host.Value;
                    var jsString = StatisticJsFile.STATISTICJS.Replace("<WEBROOTURL>", rootUrl);
                    context.Response.ContentType = "application/javascript";
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(jsString);
                });
            });
            return builder;
        }

        private static RequestTraceBuilder AddBaseService(IServiceCollection services, Action<TraceRecordOption> opt)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var options = new TraceRecordOption();
            if (opt != null)
            {
                opt.Invoke(options);
                services.Configure<TraceRecordOption>(opt);
            }
            else
            {
                services.Configure<TraceRecordOption>(opt => { opt = options; });
            }

            if (options.StorageDataBase == StorageDataBaseType.MongoDB)
            {
                var mongoSave = new MySQLRequestTraceHandler(options);
                TaskPool.Instance.RegisterQueue<RequestTraceRecord>(mongoSave.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
                TaskPool.Instance.RegisterQueue<PageStayRecord>(mongoSave.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
                TaskPool.Instance.RegisterQueue<ElementClickRecord>(mongoSave.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
            }
            else if (options.StorageDataBase == StorageDataBaseType.MySql)
            {
                var mysqlSave = new MySQLRequestTraceHandler(options);
                TaskPool.Instance.RegisterQueue<RequestTraceRecord>(mysqlSave.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
                TaskPool.Instance.RegisterQueue<PageStayRecord>(mysqlSave.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
                TaskPool.Instance.RegisterQueue<ElementClickRecord>(mysqlSave.BulkSaveTrace, options.QueueConfig.PollingIntervalSeconds, options.QueueConfig.QueueCapacity, options.QueueConfig.MaxBatchCount);
            }
            services.AddScoped<ICommQuery>(sp => new MySQLCommonQuery(options.ConnectionString));
            return new RequestTraceBuilder(services, options);
        }

    }
}
