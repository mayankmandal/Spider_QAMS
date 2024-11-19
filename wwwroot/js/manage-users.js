
$(function () {
    $("#jsGridUsers").jsGrid({
        width: "100%",
        height: "auto",
        inserting: false,
        editing: false,
        sorting: true,
        paging: true,
        autoload: true,
        pageSize: 10,
        pageButtonCount: 5,
        data: usersData,
        controller: {
            loadData: function (filter) {
                return $.grep(usersData, function (user) {
                    if (!filter.searchInput) return true;
                    let searchValue = filter.searchInput.toLowerCase();
                    return user[filter.searchBy]?.toString().toLowerCase().includes(searchValue);
                });
            }
        },
        fields: [
            { name: "Designation", title: "Designation", type: "text", width: 50 },
            {
                name: "FullName", title: "Full Name", type: "text", width: 100, css: { 'word-wrap': 'break-word' }, filtering: true, sorting: true, itemTemplate: function (value) {
                    return $("<div>").text(value).css({
                        'word-wrap': 'break-word',
                        'white-space': 'normal' // Ensures wrapping within the cell
                    });
                }
            },
            {
                name: "EmailID", title: "Email", type: "text", width: 100, css: { 'word-wrap': 'break-word' }, filtering: true, sorting: true, itemTemplate: function (value) {
                    return $("<div>").text(value).css({
                        'word-wrap': 'break-word',
                        'white-space': 'normal' // Ensures wrapping within the cell
                    });
                }
            },
            {
                name: "UserName", title: "Username", type: "text", width: 100, css: { 'word-wrap': 'break-word' }, filtering: true, sorting: true, itemTemplate: function (value) {
                    return $("<div>").text(value).css({
                        'word-wrap': 'break-word',
                        'white-space': 'normal' // Ensures wrapping within the cell
                    });
                }
            },
            { name: "ProfileSiteData.ProfileName", title: "Profile Name", type: "text", width: 80 },
            { name: "PhoneNumber", title: "Phone Number", type: "text", width: 90 },
            {
                name: "IsActive", title: "Is Active", type: "checkbox", sorting: false,
                itemTemplate: function (value) { return `<input type="checkbox" ${value ? "checked" : ""} disabled>`; }
            },
            {
                name: "IsADUser", title: "Is ADUser", type: "checkbox", sorting: false,
                itemTemplate: function (value) { return `<input type="checkbox" ${value ? "checked" : ""} disabled>`; }
            },
            {
                title: "Actions",
                width: 80,
                align: "center",
                itemTemplate: function (_, item) {
                    let readButton = $("<button>")
                        .addClass("btn btn-warning btn-sm me-1")
                        .html('<i class="fas fa-eye"></i>')
                        .on("click", function () {
                            window.location.href = `/Users/ReadUser?userId=${item.UserId}`
                        });
                    let editButton = $("<a>")
                        .addClass("btn btn-primary btn-sm me-1")
                        .attr("href", `/Users/UpdateUser?userId=${item.UserId}`)
                        .html('<i class="fas fa-edit"></i>');
                    let deleteButton = $("<a>")
                        .addClass("btn btn-danger btn-sm")
                        .attr("href", `/Users/DeleteUser?userId=${item.UserId}`)
                        .html('<i class="fas fa-trash-alt"></i>');
                    return $("<div>").append(readButton).append(editButton).append(deleteButton);
                }
            }
        ]
    });

    // Search button event handler
    $('#searchBtn').on('click', function () {
        $("#jsGridUsers").jsGrid("loadData", {
            searchBy: $('#searchBy').val(),
            searchInput: $('#searchInput').val()
        });
    });

    // Reset button event handler
    $('#resetBtn').on('click', function () {
        $('#searchInput').val('');
        $("#jsGridUsers").jsGrid("loadData", {});
    });

    $("#exportExcelBtn").on("click", function () {
        const gridData = $("#jsGridUsers").jsGrid("option", "data"); // Get Current Grid Data

        // Pre-process the data to replace nulls and handle ProfileSiteData
        const processedData = gridData.map(row => {
            // Replace `ProfileSiteData` with `ProfileSiteData.ProfileName` and handle nulls
            return {
                Designation: row.Designation || '',
                FullName: row.FullName || '',
                EmailID: row.EmailID || '',
                UserName: row.UserName || '',
                ProfileName: row.ProfileSiteData?.ProfileName || '',
                PhoneNumber: row.PhoneNumber || '',
                IsActive: row.IsActive ? "Yes" : "No",
                IsADUser: row.IsADUser ? "Yes" : "No"
            };
        });

        const worksheet = XLSX.utils.json_to_sheet(processedData);
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, "Users");

        // Export the workbook
        XLSX.writeFile(workbook, "UsersData.xlsx");
    });

    $("#exportPdfBtn").on("click", function () {
        const gridData = $("#jsGridUsers").jsGrid("option", "data");

        // Pre-process the data to replace nulls and handle ProfileSiteData
        const processedData = gridData.map(row => {
            // Replace `ProfileSiteData` with `ProfileSiteData.ProfileName` and handle nulls
            return {
                Designation: row.Designation || '',
                FullName: row.FullName || '',
                EmailID: row.EmailID || '',
                UserName: row.UserName || '',
                ProfileName: row.ProfileSiteData?.ProfileName || '',
                PhoneNumber: row.PhoneNumber || '',
                IsActive: row.IsActive ? "Yes" : "No",
                IsADUser: row.IsADUser ? "Yes" : "No"
            };
        });

        // Define the table headers and body for pdfMake
        const headers = ["Designation", "FullName", "EmailID", "Username", "ProfileName", "PhoneNumber", "Is Active", "Is ADUser"];
        const body = processedData.map(row => [
            row.Designation,
            row.FullName,
            row.EmailID,
            row.UserName,
            row.ProfileName,
            row.PhoneNumber,
            row.IsActive,
            row.IsADUser
        ]);

        // Define document structure for pdfmake
        const docDefinition = {
            pageOrientation: 'landscape',
            pageSize: 'B4',
            content: [
                { text: "Users Data", style: "header" },
                {
                    table: {
                        headerRows: 1,
                        widths: Array(headers.length).fill("auto"),
                        body: [
                            headers,
                            ...body
                        ]
                    }
                }
            ],
            styles: {
                header: { fontSize: 18, bold: true, margin: [0, 0, 0, 10]}
            }
        };

        // Generate PDF
        pdfMake.createPdf(docDefinition).download("UsersData.pdf");
    });
});