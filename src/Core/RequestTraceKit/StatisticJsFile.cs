using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    internal class StatisticJsFile
    {
        public const string STATISTICJS = @"window.dca_trace_url = '<WEBROOTURL>';
window.init_page_stay = function (traceId) {
    window.page_stay_second = 0;
    var currentSec = sessionStorage.getItem(traceId);
    if (currentSec)
        window.page_stay_second = currentSec;
    window.setInterval(function () {
        window.page_stay_second++;
    },
        1000);
}
window.end_page_stay = function (traceId) {
    var data = JSON.stringify({
        Url: location.href,
        Time: window.page_stay_second,
        TraceId: traceId
    });
    navigator.sendBeacon(window.dca_trace_url + '/basestatistic/stay', data);
    sessionStorage.setItem(traceId, window.page_stay_second);
    console.log('page stay second:' + window.page_stay_second);
}

window.wtk = function (to, from, siteId) {
    var getJSON = function (url) {
        var promise = new Promise(function (resolve, reject) {
            var handler = function (evt) {
                if (this.readyState !== 4) {
                    return;
                }
                if (this.status === 200 || this.status === 0) {
                    resolve(this.response);
                } else {
                    reject(new Error(this.statusText));
                }
            };
            var client = new XMLHttpRequest();
            client.open('GET', url);
            client.responseType = 'json';
            client.setRequestHeader('Accept', 'application/json');
            client.send();
            client.onreadystatechange = handler;

        });
        return promise;
    };
    var traceId = guid();
    var pa = window.wext || {};
    var pageUrl = to;
    if (to.indexOf('http') <= -1) {
        pageUrl = location.origin + to;
    }
    //获得埋点统计选择器
    getJSON(window.dca_trace_url + '/basestatistic/clickelementselectors?siteId=' + siteId).then(function (result) {
        console.log('Data: ', result);
        var epoint = result;
        var doc = document;
        for (var i = 0; i < epoint.length; i++) {
            var elArr = doc.querySelectorAll(epoint[i]);
            for (var j = 0; j < elArr.length; j++) {
                var el = elArr[j];
                if (el) {
                    el.addEventListener('click',
                        function (event) {
                            console.log(event.currentTarget.outerHTML);
                            var data = JSON.stringify({
                                Url: location.href,
                                SiteId: siteId,
                                Element: event.currentTarget.outerHTML
                            });
                            navigator.sendBeacon(window.dca_trace_url + '/basestatistic/click?siteId=' + siteId, data);
                        },
                        true);
                }
            }
        }
    }, function (error) {
        console.error('err', error);
    });
    var traceData = JSON.stringify({
        ToUrl: pageUrl,
        FromUrl: from,
        TraceId: traceId,
        SiteId: siteId,
        ExtentionData: pa
    });
    navigator.sendBeacon(window.dca_trace_url + '/basestatistic/trace', traceData);

    function guid() {
        var d = new Date().getTime();
        var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g,
            function (c) {
                var r = (d + Math.random() * 16) % 16 | 0;
                d = Math.floor(d / 16);
                return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
            });
        return uuid;
    };

    return traceId;
};
(function () {
    var siteId = document.getElementById('wlyctracejs').getAttribute('siteId');
    var traceId = window.wtk(location.pathname, document.referrer, siteId);
    var second = 0;
    var currentSec = sessionStorage.getItem(traceId);
    if (currentSec)
        second = currentSec;
    window.setInterval(function () {
        second++;
    },
        1000);
    console.log(second);
    window.addEventListener('beforeunload',
        function (event) {
            var data = JSON.stringify({
                Url: location.href,
                Time: second,
                SiteId: siteId,
                TraceId: traceId
            });
            navigator.sendBeacon(window.dca_trace_url + '/basestatistic/stay', data);
            sessionStorage.setItem(traceId, second);
        },
        false);
})(window);";

    }
}
