// Function to update selected pages when category checkbox changes
function updateSelectedCategories(containerId) {
    const categoryCheckboxes = $(`#${containerId} .category-checkbox`);
    categoryCheckboxes.each(function () {
        const categoryCheckbox = $(this);
        const categoryId = categoryCheckbox.val();
        const isChecked = categoryCheckbox.is(":checked");

        $(`#${containerId} .category-${categoryId} .page-checkbox`).each(function () {
            $(this).prop("checked", isChecked);
            const pageId = $(this).val();
            const page = selectedPagesLst.find(p => p.pageId == pageId);
            if (page) {
                page.isSelected = isChecked;
            }
        });
    });
}

function updateSelectedPages(containerId) {
    const checkboxes = $(`#${containerId} .page-checkbox`);
    selectedPagesLst = selectedPagesLst.map(page => {
        const checkbox = checkboxes.filter(`[value="${page.pageId}"]`);
        if (checkbox.length) {
            page.isSelected = checkbox.is(":checked");
        }
        return page;
    });

    const categoryCheckboxes = $(`#${containerId} .category-checkbox`);
    categoryCheckboxes.each(function () {
        const categoryCheckbox = $(this);
        const categoryId = categoryCheckbox.val();
        const allPages = $(`#${containerId} .category-${categoryId} .page-checkbox`);
        const selectedPages = allPages.filter(":checked");

        categoryCheckbox.prop("checked", allPages.length === selectedPages.length);
    });
}

function populateCategoryCheckboxes(containerId, pagesLst, categories, selectedPageIds = [], disable = false) {
    const container = $(`#${containerId}`);
    container.empty();

    // Group pages by category
    const categorizedPages = {};
    pagesLst.forEach(page => {
        if (!categorizedPages[page.pageCatId]) {
            categorizedPages[page.pageCatId] = [];
        }
        categorizedPages[page.pageCatId].push(page);
    });

    categories.forEach(category => {
        const categoryId = category.pageCatId;
        const categoryName = category.categoryName;
        const categoryPages = categorizedPages[categoryId] || [];
        const isCategoryChecked = categoryPages.every(page => selectedPageIds.includes(page.pageId));
        const disabledAttr = disable ? 'disabled' : '';
        const totalPages = categoryPages.length;

        container.append(`
            <div class="col">
                <div class="card">
                    <div class="card-header card-header-text card-header-info">
                        <div class="card-text">
                            <div class="form-check">
                                <label class="card-title form-check-label">
                                    <input
                                        type="checkbox"
                                        class="form-check-input category-checkbox"
                                        value="${categoryId}" 
                                        ${isCategoryChecked ? 'checked' : ''} 
                                        ${disabledAttr} 
                                        onchange="updateSelectedCategories('${containerId}')"
                                    /> ${categoryName}
                                    <span class="form-check-sign">
                                        <span class="check"></span>
                                    </span>
                                </label>
                            </div>
                            <div class="card-category">(${totalPages} pages)</div>
                        </div>
                    </div>
                    <div class="card-body category-pages category-${categoryId}">
                        ${categoryPages.map(page => `
                            <div class="form-check">
                                <label class="form-check-label">
                                    <input 
                                        type="checkbox" 
                                        class="form-check-input page-checkbox me-2" 
                                        value="${page.pageId}" 
                                        ${selectedPageIds.includes(page.pageId) ? 'checked' : ''} 
                                        ${disabledAttr} 
                                        onchange="updateSelectedPages('${containerId}')"
                                    >
                                    <span class="form-check-sign">
                                        <span class="check"></span>
                                    </span>
                                    ${page.pageDesc}
                                </label>
                            </div>
                        `).join("")}
                    </div>
                </div>
            </div>
        `);
    });
}


function getPagesForProfile(profileId) {
    selectedPagesLst = pages.map(page => {
        const isSelected = profilespages.some(pp =>
            pp.profile.profileId === profileId &&
            pp.pages.some(p => p.pageId === page.pageId)
        );
        return { ...page, isSelected };
    });
    return selectedPagesLst;
}
function showCreateForm() {
    hideAllForms();
    selectedPagesLst = pages.map(page => ({ ...page, isSelected: false }));
    populateCategoryCheckboxes('createcategoriesContainer', selectedPagesLst, categories);
    $('#createFormDiv').show();
}

function showEditForm(profileId, profileName) {
    hideAllForms();
    selectedPagesLst = getPagesForProfile(profileId);

    $('#editProfileId').val(profileId);
    $('#editProfileName').val(profileName);

    const selectedPageIds = selectedPagesLst.filter(page => page.isSelected).map(page => page.pageId);
    populateCategoryCheckboxes('editcategoriesContainer', pages, categories, selectedPageIds);
    $('#editFormDiv').show();
}

function showDeleteForm(profileId, profileName) {
    hideAllForms();
    selectedPagesLst = getPagesForProfile(profileId);

    $('#deleteProfileId').val(profileId);
    $('#deleteProfileName').val(profileName);
    $('#deleteProfilePName').text(profileName);

    const selectedPageIds = selectedPagesLst.filter(page => page.isSelected).map(page => page.pageId);
    populateCategoryCheckboxes('deletecategoriesContainer', pages, categories, selectedPageIds, true);
    $('#deleteFormDiv').show();
}

function hideAllForms() {
    $('#createFormDiv, #editFormDiv, #deleteFormDiv').hide();
}

$(document).ready(function () {
    $('#createFormDiv').on('submit', function (e) {
        e.preventDefault();
        $('#createFormDiv input[name="SelectedPagesJson"]').val(JSON.stringify(selectedPagesLst));
        this.submit();
    });

    $('#editFormDiv').on('submit', function (e) {
        e.preventDefault();
        $('#editFormDiv input[name="SelectedPagesJson"]').val(JSON.stringify(selectedPagesLst));
        this.submit();
    });

    $('#deleteFormDiv').on('submit', function (e) {
        e.preventDefault();
        $('#deleteFormDiv input[name="SelectedPagesJson"]').val(JSON.stringify(selectedPagesLst));
        this.submit();
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
