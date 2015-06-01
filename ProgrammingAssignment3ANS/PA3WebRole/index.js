$("#searchBox").keyup(function ()
{
    var query = $("#searchBox").val();

    if (query.length > 0)
    {
        $.ajax({
            type: "POST",
            url: "admin.asmx/searchTrie",
            data: "{query: '" + query + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var result = "";

                for (var i =0; i < msg.d.length; i++)
                {
                    result += msg.d[i] + "<br>";
                }

                $("#suggestionsDiv").html(result);
            }
        });
    }
    else
    {
        $("#suggestionsDiv").html("");
    }
});

function SearchQuery()
{
    var query = $("#searchBox").val();
    
    $.ajax({
        type: "POST",
        url: "admin.asmx/SearchQuery",
        data: "{query: '" + query + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            console.log("ajaxSearchQuery success");

            var results = "";

            if (msg.d.length == 0)
            {
                results = "No results found."
            }
            else
            {
                for (var i = 0; i < msg.d.length; i++)
                {
                    results += msg.d[i].Item3 + "<br>" 
                        + msg.d[i].Item1 + "<br>"
                        + msg.d[i].Item4 + "<br><br>";
                }
            }

            $("#resultsDiv").html(results);
        },
        error: function (msg) {
            console.log("ajaxSearchQuery error");
        }
    });

    $.ajax({
        crossDomain: true,
        contentType: "application/json; charset=utf-8",
        url: "http://ec2-54-187-111-5.us-west-2.compute.amazonaws.com/nba.php",
        data: { name: query },
        dataType: "jsonp",
        jsonp: "callback",
        jsonpCallback: "jsonpCallback",
        success: function()
        {
            console.log("nbaSearch success");
        },
        error: function()
        {
            console.log("nbaSearch failure");
        }
    });
}

function jsonpCallback(msg)
{
    var result = "";

    var nameAndStats = msg.split(",");
    result += "<b>" + nameAndStats[0] + "</b><br>";

    var elements = nameAndStats[1].split(" ");

    for (var i = 0; i < elements.length; i += 2)
    {
        result += elements[i] + ": " + elements[i + 1] + ", ";
    }

    $("#nbaDiv").html(result);
}