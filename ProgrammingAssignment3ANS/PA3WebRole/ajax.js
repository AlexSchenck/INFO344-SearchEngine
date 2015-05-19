function ajaxStartCrawl()
{
    $.ajax({
        type: "POST",
        url: "admin.asmx/StartCrawling",
        data: "{url: 'http://www.cnn.com/robots.txt'}",
        contentType: "application/json; charset=utf-8",
        success: function (msg) {
            console.log("ajaxStartCrawl success");
        },
        error: function (msg) {
            console.log("ajaxStartCrawl error");
        }
    });
}

function ajaxStopCrawl()
{
    $.ajax({
        type: "POST",
        url: "admin.asmx/StopCrawling",
        contentType: "application/json; charset=utf-8",
        success: function (msg) {
            console.log("ajaxStopCrawl success");
        },
        error: function (msg) {
            console.log("ajaxStopCrawl error");
        }
    });
}

function ajaxClearIndex()
{
    $.ajax({
        type: "POST",
        url: "admin.asmx/ClearIndex",
        contentType: "application/json; charset=utf-8",
        success: function (msg) {
            console.log("ajaxClearIndex success");
        },
        error: function (msg) {
            console.log("ajaxClearIndex error");
        }
    });
}

function ajaxSearchURL()
{
    var url = $("searchURL").val();

    $.ajax({
        type: "POST",
        url: "admin.asmx/GetPageTitle",
        data: "{url: '" + url + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log("ajaxSearchURL success");

            var result;
            result = "Placeholder text";
            $("#titleSearchResult").html("<br><b>" + result + "</b>");
        },
        error: function (msg) {
            console.log("ajaxSearchURL error");
        }
    });
}

function ajaxGetStatus()
{
    $.ajax({
        type: "POST",
        url: "admin.asmx/GetStatus",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log("ajaxGetStatus success");

            var results;
            results = "Worker role is currently: <b>" + msg.d[0] + "</b><br>";
            results += "CPU Utilization Percentage: <b>" + msg.d[1] + "</b><br>";
            results += "RAM Available: <b>" + msg.d[2] + "</b><br>";
            results += "Total URL's Crawled: <b>" + msg.d[3] + "</b><br>";
            results += "Size of URL Queue: <b>" + msg.d[4] + "</b><br>";
            results += "Size of Index: <b>" + msg.d[5] + "</b><br>";
            results += "Last 10 URLs Crawled: <br>";
            results += "Error URLs: <br>";

            $("#metrics").html(results);
        },
        error: function (msg) {
            console.log("ajaxGetStatus error");
        }
    });
}

$(document).ready(ajaxGetStatus());