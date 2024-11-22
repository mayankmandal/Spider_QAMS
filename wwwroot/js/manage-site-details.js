$(document).ready(function () {
    $('#sitesTable').DataTable({
        data: sitesJson,
        columns: [
            { data: 'siteCode', title: 'Site Code', width: '150px' },
            { data: 'siteName', title: 'Site Name', width: '150px' },
            { data: 'siteCategory', title: 'Site Category', width: '100px' },
            { data: 'branchNo', title: 'Branch Number', width: '120px' },
            {
                data: 'contactInformation.emailAddress',
                title: 'Email Address',
                width: '150px',
                render: function (data, type, row) {
                    return `<div style="white-space: normal; word-wrap: break-word;">${data}</div>`;
                }
            },
            {
                data: null,
                title: 'Actions',
                className: 'text-center',
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    return `
                        <button class="btn btn-warning btn-sm me-1" onclick="window.location.href='/ViewSiteDetails?siteId=${row.siteID}'">
                            <i class="material-icons">visibility</i>
                        </button>
                        <button class="btn btn-success btn-sm me-1" onclick="window.location.href='/EditSiteDetails?siteId=${row.siteID}'">
                            <i class="material-icons">drive_file_rename_outline</i>
                        </button>
                        <button class="btn btn-danger btn-sm me-1" onclick="window.location.href='/DeleteSiteDetails?siteId=${row.siteID}'">
                            <i class="material-icons">delete_outline</i>
                        </button>
                        <button class="btn btn-primary btn-sm me-1" onclick="window.location.href='/SiteImageUploader?siteId=${row.siteID}'">
                            <i class="material-icons">photo_library</i>
                        </button>
                    `;
                }
            }
        ],
        paging: true,
        pagingType: "full_numbers",
        pageLength: 10,
        lengthMenu: [
            [5, 10, 25, 50, -1], // This controls the number of options shown in the dropdown
            [5, 10, 25, 50, 'All'] // Text shown next to each option
        ],
        searching: true,
        responsive: true,
        order: [[0, 'asc']],
        language: {
            search: "_INPUT_",
            searchPlaceholder: "Search records"
        },
        dom: `
                <"row"<"col-sm-3"l><"col-sm-6 text-center"B><"col-sm-3"f>> 
                t
                <"row"<"col-sm-6"i><"col-sm-6"p>>
            `,
        noDataContent: "No Sites found.",
        initComplete: function () {
            // Adjust the dropdown select using jQuery
            var select = $('#sitesTable_length label select');
            select.addClass('form-control px-4'); // Add form-control class for styling
        }
    });
});
