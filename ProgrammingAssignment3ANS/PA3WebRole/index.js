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
}