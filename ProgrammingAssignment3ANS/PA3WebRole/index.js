$("#searchBox").keyup(function () {
    var query = $("#searchBox").val();

    /* Show query suggestions */
});

function SearchQuery() {
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
                for (i = 0; i < msg.d.length; i += 2)
                {
                    results += msg.d[i] + "<br>" + msg.d[i + 1] + "<br><br>";
                }
            }

            $("#resultsDiv").html(results);
        },
        error: function (msg) {
            console.log("ajaxSearchQuery error");
        }
    });
}