function showCreateForm() {
    hideAllForms();
    $("#createFormDiv").css("display", "flex");
}

function showEditForm(locationId, locationName, streetName, cityId, regionId, branchName, districtName, sponsorId) {
    hideAllForms();
    $("#editFormDiv").css("display", "flex");

    $("#editLocationId").val(locationId);
    $("#editLocationName").val(locationName);
    $("#editStreetName").val(streetName);
    $("#editBranchName").val(branchName);
    $("#editDistrictName").val(districtName);

    // Set region dropdown and trigger city load
    $("#editRegionId").val(regionId);

    $("#editSponsorId").val(sponsorId);

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

    // Set region dropdown and trigger city load
    $('#deleteRegionId').empty().append(new Option(regionName, regionId)).prop('disabled', true); // Set and disable the dropdown
    $('#deleteCitySelect').empty().append(new Option(cityName, cityId)).prop('disabled', true); // Set and disable the dropdown
    $('#deleteSponsorId').empty().append(new Option(sponsorName, sponsorId)).prop('disabled', true); // Set and disable the dropdown
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
    $(citySelectId).append('<option value="" disabled selected>--Select City--</option>');

    // Find the selected region's cities from regionData
    var selectedRegion = regionData.find(r => r.regionId == parseInt(regionId));

    // Check if selectedRegion has cities
    if (selectedRegion && selectedRegion.cities) {
        // Add cities to the city dropdown
        selectedRegion.cities.forEach(function (city) {
            $(citySelectId).append(`<option value="${city.cityId}">${city.cityName}</option>`);
        });
    }
    // If there's a city to be selected, set it after cities are loaded
    if (selectedCityId) {
        $(citySelectId).val(selectedCityId);
    }
}

$("#jsGrid").jsGrid({
    width: "100%",
    height: "200px",

    inserting: false,
    editing: false,
    sorting: true,
    paging: true,
    data: locationsData,

    fields: [
        { name: "location", type: "text", title: "Location", width: 150 },
        { name: "streetName", type: "text", title: "Street Name", width: 150 },
        { name: "cityName", type: "text", title: "City", width: 100 },
        { name: "regionName", type: "text", title: "Region", width: 120 },
        { name: "sponsorName", type: "text", title: "Sponsor", width: 150 },
        {
            title: "Actions",
            width: 100,
            align: "center",
            itemTemplate: function (_, item) {
                var editButton = $("<button>")
                    .addClass("btn btn-primary btn-sm")
                    .html('<i class="fas fa-edit"></i> Edit')
                    .on("click", function () {
                        alert("Edit location: " + item.locationId);
                        // Implement your edit logic here
                    });
                var deleteButton = $("<button>")
                    .addClass("btn btn-danger btn-sm")
                    .html('<i class="fas fa-trash-alt"></i> Delete')
                    .on("click", function () {
                        var index = locationsData.indexOf(item);
                        if (index > -1) {
                            locationsData.splice(index, 1);
                            $("#jsGrid").jsGrid("loadData");
                            alert("Deleted location: " + item.locationId);
                        }
                    });

                return $("<div>").append(editButton).append(deleteButton);
            }
        }
    ],
    noDataContent: "No Locations found.",
});