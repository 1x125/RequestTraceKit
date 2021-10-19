# RequestTraceKit--WebAPI请求数据记录和网站数据统计

[![standard-readme compliant](https://img.shields.io/badge/readme%20style-standard-brightgreen.svg?style=flat-square)](https://github.com/1x125/RequestTraceKit.git)

WebAPI请求数据记录和网站数据统计工具
RequestTraceKit 是一种针对webApi请求数据记录和网站数据统计，数据类型包含了IP、访问地址、设备信息、浏览器信息等，还支持对网站页面埋点和停留时间统计。
目前支持MySQL和MongoDB两种存储方式，并且可以自定义存储。



## 目录

- [原理](#原理)
- [安装](#安装)
- [基本使用](#基本使用)
- [扩展使用](#扩展使用)
- [自定义存储](#自定义存储)
- [网站统计](#网站统计)



## 原理

使用MVC过滤器拦截请求，构造访问数据

## 安装

使用Nuget进行安装，命令如下：
```sh
 Install-Package RequestTraceKit
```
## 基本使用

在Startup.cs注入服务和相关配置：
```csharp
services.AddRequestTraceService(options =>
{
	//数据库链接
	options.ConnectionString = "Server=localhost;Port=3306;Initial Catalog = Trace;Uid = root;Pwd =123456;Allow User Variables=true;";
	options.PageStayTimeRules = new List<string> { "/"};//需要统计停留时间的页面，模糊匹配
});

```

在Startup.cs 全局注入RequestTrace特性，代码如下：
```csharp
services.AddMvc(op => { op.Filters.Add<RequestTraceAttribute>(); });
```

或者在需要统计的Controller或Action添加RequestTrace特性，代码如下：
```csharp
[Route("Home")]  
[RequestTrace]  
public class HomeController : Controller  
{  
	[HttpGet("Index")]  
	public IActionResult Index()  
	{  
		return View();  
	}  
}

```

## 扩展使用

如果需要统计其他自定义字段信息，则需要增加如下操作：

修改注入方式，例如：MyTraceBehavior 为自定义扩展类，实现接口ITraceBehavior  
```csharp
services.AddRequestTraceService<MyTraceBehavior>(options =>
{
	options.ConnectionString = "Server=192.168.200.170;Port=3306;Initial Catalog = Trace;Uid = root;Pwd =123456;Allow User Variables=true;";
	options.PageStayTimeRules = new List<string> { "/"};
});  

```

继承ITraceBehavior接口， TraceTraker示例代码如下：
```csharp
public class MyTraceBehavior : ITraceBehavior  
{  
	private static bool Disabled = false;  
	public MyTraceBehavior(IOptions<TraceRecordOption> options)  
	{  
		if (options.Value.Disable.HasValue)  
			Disabled = options.Value.Disable.Value;  
	}  


	/// <summary>  
	/// 检测是否需要统计  
	/// </summary>  
	/// <param name="httpContext"></param>  
	/// <returns></returns>  
	public bool CheckNeedTrace(HttpContext httpContext)  
	{  
		return Disabled == false;  
	}  

	/// <summary>  
	/// 获得客户端类型  
	/// </summary>  
	/// <param name="httpContext"></param>  
	/// <returns></returns>  
	public string DetectRequestClientType(HttpContext httpContext)  
	{  
		string clientType = "Customer";  
		return clientType;  
	}  

	/// <summary>  
	/// 自定义设置扩展参数  
	/// </summary>  
	/// <param name="httpContext"></param>  
	/// <param name="msg"></param>  
	public void SetExtentionData(HttpContext httpContext, RequestTraceMessage msg)  
	{  
		 msg.ExtentionData.Add("ExtProductId",httpContext.Items["ExtProductId"]?.ToString());//ExtProductId对应扩展字段名称  
	}  
}  

```

## 自定义存储

如果需要其它方式进行存储（如SQLServer），则需要进行如下操作：

修改注入方式，实现IRequestTraceHandler接口，如MyRequestTraceHandler
```csharp
services.AddRequestTraceService(options =>
{
	options.ConnectionString = "Server=192.168.200.170;Port=3306;Initial Catalog = Trace;Uid = root;Pwd =123456;Allow User Variables=true;";
	options.PageStayTimeRules = new List<string> { "/"};
}).AddCustomStorageType<MyRequestTraceHandler>();//注入自定义存储实现  

```
需要实现三个方法，分别用于存储请求数据、页面停留时间和埋点统计数据
```csharp
public class MyRequestTraceHandler : IRequestTraceHandler
{  
	private TraceRecordOption _option;
	public MySQLRequestTraceHandler(TraceRecordOption option)
	{
		_option = option;
	}

    public void BulkSaveTrace(IEnumerable<RequestTraceRecord> traces)
    {
			//批量保存请求数据
    }

    public void BulkSaveTrace(IEnumerable<PageStayRecord> traces)
    {
			//批量更新页面停留时间数据
    }

    public void BulkSaveTrace(IEnumerable<ElementClickRecord> traces)
    {
			//批量保存埋点数据
    } 
}  

```

## 网站统计

网站统计包含了页面停留时间和埋点统计的实现方式，参考RequestTrace.Web项目，可以作为独立的统计服务进行部署，操作步骤如下：
```csharp
//新建一个控制器用于继承BaseStatisticController的基本统计方法
[Route("basestatistic")]
public class StatisticController : BaseStatisticController
{
	//目前埋点配置数据都保存在MySQL，通过ICommQuery来查询
    public StatisticController(IOptions<TraceRecordOption> options, ICommQuery commonQuery) : base(options, commonQuery)
    {
    }
}
```

然后在需要统计的页面引入一下javascript代码，代码如下：
```javascript
<script type="text/javascript">
    window.wext = window.wext || {};
    (function () {
        var qs = [];
        var siteId = 1;//站点Id，可以统计多个站点，在表website中配置
        var u = 'http://localhost/trace.js';
        var d = document, g = d.createElement('script'), s = d.getElementsByTagName('script')[0];
        g.id = 'wlyctracejs'; g.type = 'text/javascript'; g.async = true; g.defer = true; g.src = u;
        g.setAttribute("siteId",siteId);
        var hg = d.getElementById(g.id);
        console.log(hg);
        if (hg) {
            hg.remove();
        }
        s.parentNode.insertBefore(g, s);
    })();
 <script>
```