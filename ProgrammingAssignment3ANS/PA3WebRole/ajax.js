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
        },
        error: function (msg) {
            console.log("ajaxGetStatus error");
        }
    });
}

$(document).ready(ajaxGetStatus());