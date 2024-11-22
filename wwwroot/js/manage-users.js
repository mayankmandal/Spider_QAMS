$(document).ready(function () {
    $('#datatables').DataTable({
        data: usersData,
        columns: [
            { data: 'Designation' },
            { data: 'FullName' },
            {
                data: 'EmailID',
                render: function (data) {
                    return `<div style="word-break: break-word; white-space: normal;">${data}</div>`;
                },
                width: "10rem"
            },
            {
                data: 'UserName',
                render: function (data) {
                    return `<div style="word-break: break-word; white-space: normal;">${data}</div>`;
                },
                width: "10rem"
            },
            { data: 'ProfileSiteData.ProfileName' },
            { data: 'PhoneNumber' },
            {
                data: 'IsActive',
                render: function (data) {
                    return `<input type="checkbox" ${data ? "checked" : ""} disabled>`;
                }
            },
            {
                data: 'IsADUser',
                render: function (data) {
                    return `<input type="checkbox" ${data ? "checked" : ""} disabled>`;
                }
            },
            {
                data: null,
                className: "text-right",
                render: function (data, type, row) {
                    return `
                        <a href="/Users/ReadUser?userId=${row.UserId}" class="btn btn-link btn-info btn-just-icon like"><i class="material-icons">visibility</i></a>
                        <a href="/Users/UpdateUser?userId=${row.UserId}" class="btn btn-link btn-success btn-just-icon edit"><i class="material-icons">drive_file_rename_outline</i></a>
                        <a href="/Users/DeleteUser?userId=${row.UserId}" class="btn btn-link btn-danger btn-just-icon remove"><i class="material-icons">delete_outline</i></a>
                    `;
                }
            }
        ],
        pagingType: "full_numbers",
        pageLength: 10,
        lengthMenu: [
            [10, 25, 50, -1],
            [10, 25, 50, "All"]
        ],
        responsive: true,
        language: {
            search: "Search:",
            searchPlaceholder: "Enter value..."
        },
        paging: true,
        ordering: true,
        autoWidth: true,
        dom: 'Bfrtip',
        buttons: [
            {
                extend: 'copy',
                title: 'User Data Copy',
                filename: `User_Data_Copy_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                }
            },
            {
                extend: 'csv',
                title: 'User Data CSV',
                filename: `User_Data_CSV_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                }
            },
            {
                extend: 'excel',
                title: 'User Data Excel',
                filename: `User_Data_Excel_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                }
            },
            {
                extend: 'pdf',
                title: 'User Data PDF',
                filename: `User_Data_PDF_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)'  // Exclude the last column (Actions) from exports
                },
                customize: function (doc) {
                    // Set orientation and page size
                    doc.pageOrientation = 'landscape';
                    doc.pageSize = 'A4';

                    // Style table borders
                    doc.styles.tableBodyEven = { fillColor: '#FFFFFF' };
                    doc.styles.tableBodyOdd = { fillColor: '#F3F3F3' };

                    // Add table border style
                    doc.content[1].table.body.forEach(row => {
                        row.forEach(cell => {
                            cell.border = [true, true, true, true]; // Top, Left, Bottom, Right
                        });
                    });
                }
            },
            {
                extend: 'print',
                title: 'User Data Print',
                filename: `User_Data_Print_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                }
            }
        ],
        columnDefs: [
            {
                targets: -1,
                orderable: false, // Make the last column non-sortable
            }
        ]
    });
});
