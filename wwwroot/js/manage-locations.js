function showCreateForm() {
    hideAllForms();
    $("#createFormDiv").css("display", "flex");

    // Refresh selectpickers for the create form dropdowns
    $('#createRegionId').selectpicker('refresh');
    $('#createCitySelect').selectpicker('refresh');
    $('#createSponsorId').selectpicker('refresh');
}

function showEditForm(locationId, locationName, streetName, cityId, regionId, branchName, districtName, sponsorId) {
    hideAllForms();
    $("#editFormDiv").css("display", "flex");

    $("#editLocationId").val(locationId);
    $("#editLocationName").val(locationName);
    $("#editStreetName").val(streetName);
    $("#editBranchName").val(branchName);
    $("#editDistrictName").val(districtName);

    // Set region and sponsor dropdown values
    $("#editRegionId").val(regionId).selectpicker('refresh');
    $("#editSponsorId").val(sponsorId).selectpicker('refresh');

    // Load cities for the selected region and set the city value after cities are loaded
    loadCities(regionId, cityId, 'edit'); // Pass cityId to select it later
}

function showDeleteForm(locationId, locationName, streetName, cityId, regionId, regionName, branchName, districtName, cityId, cityName, sponsorId, sponsorName) {
    hideAllForms();
    $("#deleteFormDiv").css("display", "flex");
    $("#deleteLocationId").val(locationId);
    $("#deleteLocationName").val(locationName);
    $("#deleteStreetName").val(streetName);
    $("#deleteBranchName").val(branchName);
    $("#deleteDistrictName").val(districtName);

    // Set region, city, and sponsor dropdowns and disable them
    $('#deleteRegionId').val(regionId).prop('disabled', true);
    $('#deleteRegionId').selectpicker('refresh');

    // Update the city dropdown with the provided cityId and cityName
    const cityDropdown = $('#deleteCitySelect');
    cityDropdown.empty(); // Clear existing options
    cityDropdown.append('<option value="" disabled>--Select City--</option>'); // Add default option
    cityDropdown.append(
        `<option value="${cityId}" selected>${cityName}</option>` // Add and select the city
    );
    cityDropdown.prop('disabled', true).selectpicker('refresh'); // Disable and refresh the dropdown

    $('#deleteSponsorId').val(sponsorId).prop('disabled', true);
    $('#deleteSponsorId').selectpicker('refresh');
}

function hideAllForms() {
    $("#createFormDiv").css("display", "none");
    $("#editFormDiv").css("display", "none");
    $("#deleteFormDiv").css("display", "none");
}

// Function to load cities and optionally select a city
function loadCities(regionId, selectedCityId = null, formType = "create") {
    // Determine which city select element to target based on form type
    var citySelectId = formType === "edit" ? '#editCitySelect' : '#createCitySelect';

    // Clear the current city options
    $(citySelectId).empty();

    // Add the default "Select City" option
    $(citySelectId).append('<option value="" disabled selected>--Select City--</option>');

    // Find the selected region's cities from `regionData`
    var selectedRegion = regionData.find(r => r.regionId === parseInt(regionId));

    if (selectedRegion && selectedRegion.cities) {
        // Populate cities in the dropdown
        selectedRegion.cities.forEach(function (city) {
            $(citySelectId).append(
                `<option value="${city.cityId}" ${city.cityId === parseInt(selectedCityId) ? 'selected' : ''}>
                    ${city.cityName}
                </option>`
            );
        });
    }

    // Refresh the selectpicker UI
    if ($(citySelectId).hasClass('selectpicker')) {
        $(citySelectId).selectpicker('refresh');
    }
}

$(document).ready(function () {
    // Initialize selectpicker for all select elements with the class `selectpicker`
    $('.selectpicker').selectpicker();

    const table = $('#locationsTable').DataTable({
        data: locationsData,
        columns: [
            { data: 'location', title: "Location", width: "150px" },
            { data: 'streetName', title: "Street Name", width: "150px" },
            { data: 'cityName', title: "City", width: "100px" },
            { data: 'regionName', title: "Region", width: "120px" },
            { data: 'sponsorName', title: "Sponsor", width: "150px" },
            {
                title: "Actions",
                data: null,
                width: "100px",
                className: "text-center",
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <button class="btn btn-success btn-sm me-1" onclick="showEditForm(${row.locationId}, '${row.location}', '${row.streetName}', ${row.cityId}, ${row.regionId}, '${row.branchName}', '${row.districtName}', ${row.sponsorId})">
                            <i class="material-icons">drive_file_rename_outline</i>
                        </button>
                        <button class="btn btn-danger btn-sm" onclick="showDeleteForm(${row.locationId}, '${row.location}', '${row.streetName}', ${row.cityId}, ${row.regionId}, '${row.regionName}', '${row.branchName}', '${row.districtName}', ${row.cityId}, '${row.cityName}', ${row.sponsorId}, '${row.sponsorName}')">
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
        dom: `
                <"row"<"col-sm-3"l><"col-sm-6 text-center"B><"col-sm-3"f>> 
                t
                <"row"<"col-sm-6"i><"col-sm-6"p>>
            `,
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
        ],
        noDataContent: "No Locations found.",
        initComplete: function () {
            // Adjust the dropdown select using jQuery
            var select = $('#locationsTable_length label select');
            select.addClass('form-control px-4'); // Add form-control class for styling
        }
    });
});