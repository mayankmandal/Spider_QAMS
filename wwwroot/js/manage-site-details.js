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
        pageLength: 10,
        lengthMenu: [10, 25, 50],
        searching: true,
        responsive: true,
        order: [[0, 'asc']],
        language: {
            search: "_INPUT_",
            searchPlaceholder: "Search records"
        },
        dom: 'Bfrtip',
        noDataContent: "No Sites found."
    });
});
