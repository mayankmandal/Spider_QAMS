function populatePageCheckboxes(containerId, pages, selectedPageIds = [], disable = false) {
    const container = $(`#${containerId}`);
    container.empty();

    pages.forEach(page => {
        const isChecked = selectedPageIds.includes(page.pageId);
        const disabledAttr = disable ? 'disabled' : '';
        container.append(`
            <div class="form-check form-check-inline">
                <label class="form-check-label" for="${containerId}_page_${page.pageId}">
                    ${page.pageDesc}
                    <input 
                        type="checkbox" 
                        class="form-check-input" 
                        value="${page.pageId}" 
                        ${isChecked ? 'checked' : ''} 
                        ${disabledAttr} 
                        onchange="updateSelectedPages('${containerId}')"
                    >
                    <span class="form-check-sign">
                        <span class="check"></span>
                    </span>
                </label>
            </div>
        `);
    });

    // Sync selectedPagesLst with the current checkboxes
    updateSelectedPages(containerId);
}


function getPagesForProfile(profileId) {
    const availableProfile = availablePagesLst.find(item => item.profile.profileId === profileId);
    return availableProfile ? availableProfile.pages : [];
}

// Function to show the Create form
function showCreateForm() {
    hideAllForms();
    populatePageCheckboxes('createPageCheckboxes', pages); // Empty selection
    $('#createFormDiv').show();
}

// Function to show the Edit Form
function showEditForm(profileId, profileName) {
    hideAllForms();
    const selectedPages = getPagesForProfile(profileId);
    $('#editProfileId').val(profileId);
    $('#editProfileName').val(profileName);
    // Pre-select pages where isSelected is true
    const preSelectedPageIds = selectedPages
        .filter(page => page.isSelected)
        .map(page => page.pageId);
    populatePageCheckboxes('editPageCheckboxes', selectedPages, preSelectedPageIds || []);
    $('#editFormDiv').show();
}
// Function to show the Delete form
function showDeleteForm(profileId, profileName) {
    hideAllForms();
    const selectedPages = getPagesForProfile(profileId);
    $('#deleteProfileId').val(profileId);
    $('#deleteProfileName').val(profileName);
    $('#deleteProfilePName').val(profileName);
    // Pre-select pages where isSelected is true
    const preSelectedPageIds = selectedPages
        .filter(page => page.isSelected)
        .map(page => page.pageId);
    populatePageCheckboxes('deletePageCheckboxes', selectedPages, preSelectedPageIds || [], true);
    $('#deleteFormDiv').show();
}


// Function to hide All Forms
function hideAllForms() {
    $('#createFormDiv, #editFormDiv, #deleteFormDiv').hide();
    clearValidationMessages();
}

// Function to clear validation messages and reset form validation state
function clearValidationMessages() {
    // Clear validation messages and reset validation state for the create form
    var createForm = $('#createCityForm');
    if (createForm.length > 0) {
        createForm.validate().resetForm(); // Clear valdiation errors
        createForm[0].reset(); // Reset form fields
        $('#regionCreateSelect').selectpicker('refresh');  // Refresh selectpicker for create form
    }

    // Clear validation messages and reset validation state for the edit form
    var editForm = $('#editFormDiv form');
    if (editForm.length > 0) {
        editForm.validate().resetForm();  // Clear validation errors
        editForm[0].reset();  // Reset form fields
        $('#regionUpdateSelect').selectpicker('refresh');  // Refresh selectpicker for create form
    }

    // Clear validation messages and reset validation state for the delete form
    var deleteForm = $('#deleteFormDiv form');
    if (deleteForm.length > 0) {
        deleteForm.validate().resetForm();  // Clear validation errors
        deleteForm[0].reset();  // Reset form fields
        $('#regionDeleteSelect').selectpicker('refresh');  // Refresh selectpicker for create form
    }
}

// Function to update SelectedPagesJson hidden input field
function updateSelectedPages(containerId) {
    selectedPagesLst = []; // Reset
    $(`#${containerId} input[type="checkbox"]:checked`).each(function () {
        selectedPagesLst.push($(this).val());
    });

    // Update the hidden field
    $('#SelectedPagesJson').val(JSON.stringify(selectedPagesLst));
}
$(document).ready(function () {
    // For Create Form
    $('#createFormDiv').on('submit', function () {
        e.preventDefault(); // Prevent default form submission to handle custom logic

        $('#SelectedPagesJson').val(JSON.stringify(selectedPagesLst));
    });

    $('#editFormDiv').on('submit', function () {
        $('#SelectedPagesJson').val(JSON.stringify(selectedPagesLst));
    });

    $('#deleteFormDiv').on('submit', function () {
        $('#SelectedPagesJson').val(JSON.stringify(selectedPagesLst));
    });

    // Initialize DataTables
    $('#profilesTable').DataTable({
        data: profiles,
        columns: [
            { data: 'profileName', title: "Profile Name" },
            {
                data: null,
                title: "Actions",
                className: "text-right",
                orderable: false,
                render: function (data, type, row) {
                    return `<button class="btn btn-success btn-sm me-1" onclick="showEditForm(${row.profileId}, '${row.profileName}')">
                                                                        <i class="material-icons">drive_file_rename_outline</i>
                                                                    </button>
                                                                    <button class="btn btn-danger btn-sm" onclick="showDeleteForm(${row.profileId}, '${row.profileName}')">
                                                                        <i class="material-icons">delete_outline</i>
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
        responsive: true,
        language: {
            search: "Search:",
            searchPlaceholder: "Enter value..."
        },
        ordering: true,
        autoWidth: true,
        buttons: [
            {
                extend: 'copy',
                title: 'Profile Data Copy',
                filename: `Profile_Data_Copy_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                }
            },
            {
                extend: 'csv',
                title: 'Profile Data CSV',
                filename: `Profile_Data_CSV_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                }
            },
            {
                extend: 'excel',
                title: 'Profile Data Excel',
                filename: `Profile_Data_Excel_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                }
            },
            {
                extend: 'pdf',
                title: 'Profile Data PDF',
                filename: `Profile_Data_PDF_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                },
                customize: function (doc) {
                    doc.content[1].table.widths = ['*', '*']; // Adjust columns if needed
                    doc.pageSize = 'A4';
                }
            },
            {
                extend: 'print',
                title: 'Profile Data Print',
                filename: `Profile_Data_Print_${new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '')}`,
                exportOptions: {
                    columns: ':not(:last-child)' // Exclude last column in export
                }
            }
        ],
        columnDefs: [
            {
                targets: -1, // Exclude the last column (Actions) from export
                exportOptions: {
                    columns: ':not(:last-child)'
                }
            }
        ],
        dom: `
                <"row"<"col-sm-3"l><"col-sm-6 text-center"B><"col-sm-3"f>>
                t
                <"row"<"col-sm-6"i><"col-sm-6"p>>
            `,
        noDataContent: "No Profiles found.",
        initComplete: function () {
            // Adjust the dropdown select using jQuery
            var select = $('#profilesTable label select');
            select.addClass('form-control px-4'); // Add form-control class for styling
        }
    });
});