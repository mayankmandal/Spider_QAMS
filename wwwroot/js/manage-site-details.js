function hideAllForms() {
    $("#editFormDiv").css("display", "none");
    $("#deleteFormDiv").css("display", "none");
}

$(document).ready(function () {

});

$("#jsGrid").jsGrid({
    width: "100%",
    height: "400px",

    inserting: false,
    editing: false,
    sorting: true,   // Enable sorting
    paging: true,    // Enable pagination
    filtering: true, // Enable search/filtering
    autoload: true,  // Automatically load data
    pageSize: 10,    // Items per page
    pageButtonCount: 5,  // Pagination button count

    data: sitesJson, // Use your JSON data

    fields: [
        { name: "siteCode", type: "text", title: "Site Code", width: 150, filtering: true, sorting: true },
        { name: "siteName", type: "text", title: "Site Name", width: 150, filtering: true, sorting: true },
        { name: "siteCategory", type: "text", title: "Site Category", width: 100, filtering: true, sorting: true },
        { name: "branchNo", type: "text", title: "Branch Number", width: 120, filtering: true, sorting: true },
        { name: "contactInformation.emailAddress", type: "text", title: "Email Address", width: 150, css: { "word-wrap": "break-word" }, filtering: true, sorting: true },
        {
            title: "Actions",
            width: 100,
            align: "center",
            itemTemplate: function (_, item) {
                var editButton = $("<button>")
                    .addClass("btn btn-primary btn-sm me-1")
                    .html('<i class="fas fa-edit"></i> Edit')
                    .on("click", function () {
                        // Navigate to the EditSiteDetails page with the siteId as a query parameter
                        window.location.href = `/EditSiteDetails?siteId=${item.siteID}`
                    });
                var deleteButton = $("<button>")
                    .addClass("btn btn-danger btn-sm")
                    .html('<i class="fas fa-trash-alt"></i> Delete')
                    .on("click", function () {
                        // Navigate to the EditSiteDetails page with the siteId as a query parameter
                        window.location.href = `/DeleteSiteDetails?siteId=${item.siteID}`
                    });

                return $("<div>").append(editButton).append(deleteButton);
            }
        }
    ],
    // No data message when no data is available
    noDataContent: "No Sites found.",

    // Load Data on-demand (Optional if you want to load dynamically)
    controller: {
        loadData: function (filter) {
            return $.Deferred(function (dfd) {
                var filteredData = $.grep(sitesJson, function (item) {
                    return (!filter.siteCode || item.siteCode.includes(filter.siteCode)) &&
                        (!filter.siteName || item.siteName.includes(filter.siteName)) &&
                        (!filter.siteCategory || item.siteCategory.includes(filter.siteCategory)) &&
                        (!filter.branchNo || item.branchNo.includes(filter.branchNo)) &&
                        (!filter["contactInformation.emailAddress"] ||
                            item.contactInformation.emailAddress.includes(filter["contactInformation.emailAddress"]));
                });

                dfd.resolve(filteredData); // Resolve the filtered data to render the grid
            }).promise();
        }
    }
});

$(".jsgrid-filter-row input").on("input", function () {
    if ($(this).val() === "") {
        $("#jsGrid").jsGrid("loadData"); // Reload data when filter is cleared
    }
});

