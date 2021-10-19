# DCA网站访问信息统计服务
[![standard-readme compliant](https://img.shields.io/badge/readme%20style-standard-brightgreen.svg?style=flat-square)](http://192.168.200.62:8050/lixinglun/wlyc.trace.git)

DCA网站访问信息统计服务（Dca.Service.Web）
Dca.Service.Web 是一款记录网站访问信息的Web服务，包含了页面访问统计、页面停留时间统计和埋点统计，通过引入JS脚本来使用。

## 目录

- [原理](#原理)
- [使用方法](#使用方法)


## 原理

前端注入JS文件用于向后台发起请求；
后台控制器Action接收前端请求和相关数据获取；
后台控制器Action配置相关路由映射到JS文件，用于返回JS脚本，实现了页面元素点击统计和停留时间统计。


## 使用方法

首先在DCA服务注册网站相关信息，并获得siteId（站点ID）。

然后在需要统计的页面引入一下javascript代码，代码如下：
```javascript
<script type="text/javascript">
    window.wext = window.wext || {};
    (function () {
        var qs = [];
        var siteId = 1;//站点Id由网站统计服务提供
        var u = 'http://192.168.200.170:6230/js/trace.js';
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