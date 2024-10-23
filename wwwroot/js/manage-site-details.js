// Initialize terminalMap globally if it's not already defined
let terminalMap; // Declare terminalMap
let terminalMarkerGroup; // Declare terminalMarkerGroup
var apiResultData = []; // Global variable to store the result set

// Modify your createLeafletMap function to accept a dynamic map container
function createLeafletMap(terminalData, containerId = 'mapContainer') {
    const map = L.map(containerId).setView([terminalData.latitude, terminalData.longitude], 10);
    map.zoomControl.setPosition('bottomright');

    // Create a layer group for markers
    terminalMarkerGroup = L.layerGroup().addTo(map);

    // Add a marker to the map
    const marker = L.marker([terminalData.latitude, terminalData.longitude]);

    // Create a red transparent circle around the marker
    const circle = L.circle([terminalData.latitude, terminalData.longitude], {
        radius: 10,
        color: '#e83815',
        fillOpacity: 0.4,
    }).addTo(terminalMarkerGroup);

    // Customize tooltip content with HTML
    const tooltipContent = `<div class="map-tooltip-content"></div>`;
    const tooltipOffset = L.point(0, -28); // Adjust the offset as needed

    marker.bindTooltip(tooltipContent, { direction: 'top', permanent: true, opacity: 0.7, offset: tooltipOffset }).openTooltip();
    marker.addTo(terminalMarkerGroup);

    // Google Map Layers
    const googleLayers = {
        "Google Street Map": L.tileLayer('https://{s}.google.com/vt?lyrs=m&x={x}&y={y}&z={z}', { maxZoom: 22, subdomains: ['mt0', 'mt1', 'mt2', 'mt3'] }),
        "Google Satellite Map": L.tileLayer('https://{s}.google.com/vt?lyrs=s&x={x}&y={y}&z={z}', { maxZoom: 22, subdomains: ['mt0', 'mt1', 'mt2', 'mt3'] }),
        "Google Terrain Map": L.tileLayer('https://{s}.google.com/vt?lyrs=p&x={x}&y={y}&z={z}', { maxZoom: 22, subdomains: ['mt0', 'mt1', 'mt2', 'mt3'] }),
        "Open Street Map": L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 22 }),
    };

    // Set Google Street Map as the default layer
    googleLayers["Google Street Map"].addTo(map);

    // Add layer control
    L.control.layers(googleLayers).addTo(map);

    // Add a FontAwesome button on the map
    const mapButton = L.control({ position: 'bottomleft' });
    mapButton.onAdd = function () {
        const div = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-custom');
        div.innerHTML = '<i id="enlargeMapButton" class="fas fa-expand-arrows-alt px-1"></i>';
        div.style.backgroundColor = 'white';
        div.style.padding = '5px';
        return div;
    };
    mapButton.addTo(map);

    // Event listener for clicking to enlarge the map
    $('#enlargeMapButton').on('click', function (event) {
        event.preventDefault();
        $('#mapModel').modal('show');

        // Re-render the map in the modal once the modal is shown
        $('#mapModel').on('shown.bs.modal', function () {
            const longitude = $('input[id="SiteDetailVM_GPSLong"]').val();
            const latitude = $('input[id="SiteDetailVM_GPSLatt"]').val();
            if (longitude && latitude) {
                renderLargeMapInModal(longitude, latitude);
            }
        });
    });

    return map;
}

// Function to remove all previous markers
function clearMarkers() {
    // Clear previous markers if they exist
    if (terminalMarkerGroup) {
        terminalMarkerGroup.clearLayers();
    }

    // Remove the map instance
    if (terminalMap) {
        terminalMap.remove();
        terminalMap = null; // Reset terminalMap to null
    }
}

//Function rendering the details of a terminal in a modal.
function renderTerminalDetails(longitude, latitude) {
    clearMarkers(); // Clear previous markers

    const terminalResult = { longitude, latitude };

    // Initialize the map
    terminalMap = createLeafletMap(terminalResult); // Assign the returned map to terminalMap

    // Listen for the Bootstrap modal shown event
    $('#mapModel').on('shown.bs.modal', function () {
        terminalMap.invalidateSize(); // This will ensure the map is rendered correctly in the modal
    });
}

// Function to initialize the full-size map in the modal
function renderLargeMapInModal(longitude, latitude) {

    const terminalResult = { longitude, latitude };

    // Initialize the full-screen map
    terminalMap = createLeafletMap(terminalResult, 'fullMapContainer');

    // Ensure the map is redrawn when the modal is shown
    $('#mapModel').on('shown.bs.modal', function () {
        terminalMap.invalidateSize(); // This will ensure the map is rendered correctly in the modal
    });
}

function prepareFormSubmission() {
    // Get the selected profile ID
    var selectedProfileId = $('#profileSelect option:selected').attr('data-profileid');

    // Set the hidden fields with ProfileId and ProfileName
    $('#profileIdHidden').val(selectedProfileId);     // Set ProfileName to selected profile id
}
function handleResultItemClick(item, resultItem) {
    
    // Deselect all items
    $('.search-result-item').removeClass('selected');

    // Select the clicked item
    resultItem.addClass('selected');

    // Populate top-level fields
    $('#SiteDetailVM_SiteCode').val(item.siteCode);
    $('#SiteDetailVM_SiteName').val(item.siteName);
    $('#SiteDetailVM_SiteCategory').val(item.siteCategory);
    $('#SiteDetailVM_GPSLong').val(item.gpsLatt);
    $('#SiteDetailVM_GPSLatt').val(item.gpsLong);
    $('#SiteDetailVM_BranchNo').val(item.branchNo);
    $('#SiteDetailVM_AtmClass').val(item.atmClass);

    // Populate Contact Information
    $('#SiteDetailVM_SiteContactInformation_BranchTelephoneNumber').val(item.contactInformation.branchTelephoneNumber);
    $('#SiteDetailVM_SiteContactInformation_BranchFaxNumber').val(item.contactInformation.branchFaxNumber);
    $('#SiteDetailVM_SiteContactInformation_EmailAddress').val(item.contactInformation.emailAddress);

    // Populate Geographical Details
    $('#SiteDetailVM_GeographicalDetails_NearestLandmark').val(item.geographicalDetails.nearestLandmark);
    $('#SiteDetailVM_GeographicalDetails_NumberOfKmNearestCity').val(item.geographicalDetails.numberOfKmNearestCity);
    $('#SiteDetailVM_GeographicalDetails_BranchConstructionType').val(item.geographicalDetails.branchConstructionType);
    $('#SiteDetailVM_GeographicalDetails_BranchIsLocatedAt').val(item.geographicalDetails.branchIsLocatedAt);
    $('#SiteDetailVM_GeographicalDetails_HowToReachThere').val(item.geographicalDetails.howToReachThere);
    $('input[id="SiteDetailVM_GeographicalDetails_SiteIsOnServiceRoad"]').prop('checked', item.geographicalDetails.siteIsOnServiceRoad === true);
    $('#SiteDetailVM_GeographicalDetails_HowToGetThere').val(item.geographicalDetails.howToGetThere);

    // Populate Branch Facilities
    $('input[id="SiteDetailVM_SiteBranchFacilities_Parking"]').prop('checked', item.branchFacilities.parking === true);
    $('input[id="SiteDetailVM_SiteBranchFacilities_Landscape"]').prop('checked', item.branchFacilities.landscape === true);
    $('input[id="SiteDetailVM_SiteBranchFacilities_Elevator"]').prop('checked', item.branchFacilities.elevator === true);
    $('input[id="SiteDetailVM_SiteBranchFacilities_VIPSection"]').prop('checked', item.branchFacilities.vipSection === true);
    $('input[id="SiteDetailVM_SiteBranchFacilities_SafeBox"]').prop('checked', item.branchFacilities.safeBox === true);
    $('input[id="SiteDetailVM_SiteBranchFacilities_ICAP"]').prop('checked', item.branchFacilities.icap === true);

    // Populate Data Center
    $('#SiteDetailVM_SiteDataCenter_UPSBrand').val(item.dataCenter.upsBrand);
    $('#SiteDetailVM_SiteDataCenter_UPSCapacity').val(item.dataCenter.upsCapacity);
    $('#SiteDetailVM_SiteDataCenter_PABXBrand').val(item.dataCenter.pabxBrand);
    $('#SiteDetailVM_SiteDataCenter_StabilizerBrand').val(item.dataCenter.stabilizerBrand);
    $('#SiteDetailVM_SiteDataCenter_StabilizerCapacity').val(item.dataCenter.stabilizerCapacity);
    $('#SiteDetailVM_SiteDataCenter_SecurityAccessSystemBrand').val(item.dataCenter.securityAccessSystemBrand);

    // Populate Data Center
    $('input[id="SiteDetailVM_SignBoardType_Cylinder"]').prop('checked', item.signBoard.cylinder === true);
    $('input[id="SiteDetailVM_SignBoardType_StraightOrTotem"]').prop('checked', item.signBoard.straightOrTotem === true);

    // Populate Site Miscellaneous Information
    $('#SiteDetailVM_SiteMiscInformation_TypeOfATMLocation').val(item.miscSiteInfo.typeOfATMLocation);
    $('#SiteDetailVM_SiteMiscInformation_NoOfExternalCameras').val(item.miscSiteInfo.noOfExternalCameras);
    $('#SiteDetailVM_SiteMiscInformation_NoOfInternalCameras').val(item.miscSiteInfo.noOfInternalCameras);
    $('#SiteDetailVM_SiteMiscInformation_TrackingSystem').val(item.miscSiteInfo.trackingSystem);

    // Populate Miscellaneous Branch Information
    $('#SiteDetailVM_BranchMiscInformation_NoOfCleaners').val(item.miscBranchInfo.noOfCleaners);
    $('#SiteDetailVM_BranchMiscInformation_FrequencyOfDailyMailingService').val(item.miscBranchInfo.frequencyOfDailyMailingService);
    $('#SiteDetailVM_BranchMiscInformation_ElectricSupply').val(item.miscBranchInfo.electricSupply);
    $('#SiteDetailVM_BranchMiscInformation_WaterSupply').val(item.miscBranchInfo.waterSupply);
    $('#SiteDetailVM_BranchMiscInformation_BranchOpenDate').val(item.miscBranchInfo.branchOpenDate);
    $('#SiteDetailVM_BranchMiscInformation_TellersCounter').val(item.miscBranchInfo.tellersCounter);
    $('#SiteDetailVM_BranchMiscInformation_NoOfSalesManagerOffices').val(item.miscBranchInfo.noOfSalesManagerOffices);
    $('#SiteDetailVM_BranchMiscInformation_ExistVIPSection').prop('checked', item.miscBranchInfo.existVIPSection === true);
    $('#SiteDetailVM_BranchMiscInformation_ContractStartDate').val(item.miscBranchInfo.contractStartDate);
    $('#SiteDetailVM_BranchMiscInformation_NoOfRenovationRetouchTime').val(item.miscBranchInfo.noOfRenovationRetouchTime);
    $('#SiteDetailVM_BranchMiscInformation_LeasedOwBuilding').val(item.miscBranchInfo.leasedOwBuilding === true);
    $('#SiteDetailVM_BranchMiscInformation_NoOfTeaBoys').val(item.miscBranchInfo.noOfTeaBoys);
    $('#SiteDetailVM_BranchMiscInformation_FrequencyOfMonthlyCleaningService').val(item.miscBranchInfo.frequencyOfMonthlyCleaningService);
    $('#SiteDetailVM_BranchMiscInformation_DrainSewerage').val(item.miscBranchInfo.drainSewerage);
    $('#SiteDetailVM_BranchMiscInformation_CentralAC').prop('checked', item.miscBranchInfo.centralAC === true);
    $('#SiteDetailVM_BranchMiscInformation_SplitAC').prop('checked', item.miscBranchInfo.splitAC === true);
    $('#SiteDetailVM_BranchMiscInformation_WindowAC').prop('checked', item.miscBranchInfo.windowAC === true);
    $('#SiteDetailVM_BranchMiscInformation_CashCounterType').val(item.miscBranchInfo.cashCounterType);
    $('#SiteDetailVM_BranchMiscInformation_NoOfTellerCounters').val(item.miscBranchInfo.noOfTellerCounters);
    $('#SiteDetailVM_BranchMiscInformation_NoOfAffluentRelationshipManagerOffices').val(item.miscBranchInfo.noOfAffluentRelationshipManagerOffices);
    $('#SiteDetailVM_BranchMiscInformation_SeparateVIPSection').prop('checked', item.miscBranchInfo.seperateVIPSection === true);
    $('#SiteDetailVM_BranchMiscInformation_ContractEndDate').val(item.miscBranchInfo.contractEndDate);
    $('#SiteDetailVM_BranchMiscInformation_RenovationRetouchDate').val(item.miscBranchInfo.renovationRetouchDate);
    $('#SiteDetailVM_BranchMiscInformation_NoOfTCRMachines').val(item.miscBranchInfo.noOfTCRMachines);
    $('#SiteDetailVM_BranchMiscInformation_NoOfTotem').val(item.miscBranchInfo.noOfTotem);

    const longitude = item.gpsLong;
    const latitude = item.gpsLatt;

    // Check if both the longitude and latitude are provided
    if (longitude && latitude) {
        renderTerminalDetails(longitude, latitude);
    }

    // Handle site pictures (if needed, dynamically add image elements)
    if (item.sitePicturesLst && item.sitePicturesLst.length > 0) {
        const picturesContainer = $('#sitePicturesContainer');
        picturesContainer.empty(); // Clear any existing pictures
        item.sitePicturesLst.forEach(pic => {
            const imgElement = `<img src="${pic.picPath}" alt="${pic.description}" class="img-thumbnail" />`;
            picturesContainer.append(imgElement);
        });
    }
}

function handleResultOnError(item) {
    // Populate form with selected user data
    $('#ProfileUsersData_UserId').val(item.UserId);
    // Load user image
    $('#loadedProfilePicture').attr('src', item.Userimgpath);
}

function hideAllForms() {
    $("#createFormDiv").css("display", "none");
    $("#editFormDiv").css("display", "none");
    $("#deleteFormDiv").css("display", "none");
}

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
                    .addClass("btn btn-primary btn-sm")
                    .html('<i class="fas fa-edit"></i> Edit')
                    .on("click", function () {
                        alert("Edit Site: " + item.siteID);
                        // Implement your edit logic here
                    });
                var deleteButton = $("<button>")
                    .addClass("btn btn-danger btn-sm")
                    .html('<i class="fas fa-trash-alt"></i> Delete')
                    .on("click", function () {
                        var index = sitesJson.indexOf(item);
                        if (index > -1) {
                            sitesJson.splice(index, 1);
                            $("#jsGrid").jsGrid("loadData");
                            alert("Deleted Site: " + item.siteID);
                        }
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

$(document).ready(function () {
    // Select the longitude and latitude input fields
    const gpsLongInput = $('input[id="SiteDetailVM_GPSLong"]');
    const gpsLattInput = $('input[id="SiteDetailVM_GPSLatt"]');

    // Function to check if both fields have values and trigger the map rendering
    function checkAndRenderMap() {
        const longitude = gpsLongInput.val();
        const latitude = gpsLattInput.val();

        // Check if both the longitude and latitude are provided
        if (longitude && latitude) {
            renderTerminalDetails(longitude, latitude);
        }
    }

    // Add event listeners to both inputs to listen for changes
    gpsLongInput.on('input', checkAndRenderMap);
    gpsLattInput.on('input', checkAndRenderMap);

    // Assuming your cancel button has an id of "cancelButton"
    $('#cancelButton').on('click', function () {
        $('#mapModel').modal('hide'); // Hides the modal
    });

    // Check if ProfileUsersData.UserId has a value
    if (siteDetailsDataJson) {
        handleResultOnError(siteDetailsDataJson);
        $('#updateSiteDetailsSection').show();
    }
    $('#searchButton').on('click', function () {
        $('.validation-summary-errors').empty();
        $('.field-validation-error').empty();

        var searchCriteria = $('#searchCriteria').val();
        var searchInput = $('#searchInput').val();

        if (searchInput === null || searchInput === undefined || searchInput === "" || searchInput === 0) {
            $('#searchInput').addClass("is-invalid border-danger");
            $('#searchButtonIcon').addClass("border-danger");
            return;
        }
        $('#searchInput').removeClass("is-invalid border-danger");
        $('#searchButtonIcon').removeClass("border-danger");

        $.ajax({
            url: '/api/Navigation/FetchRecord',
            type: 'POST',
            headers: {
                'Authorization': 'Bearer ' + tokenC
            },
            contentType: 'application/json',
            data: JSON.stringify({
                RecordId: 0,
                RecordType: searchCriteria, // Provide a valid type here (int)
                RecordText: searchInput
            }),
            success: function (data) {
                apiResultData = data;
                var results = $('#searchResults');
                var resultsCount = $('#resultsCount');
                results.empty();
                if (data.length > 0) {
                    resultsCount.text(`${data.length} results present`);
                    results.show();
                    resultsCount.show();
                    data.forEach(function (item,index) {
                        var resultItem = $('<div></div>').text(item).addClass('search-result-item').css({
                            border: '1px solid #ccc',
                            padding: '10px',
                            margin: '5px',
                            flex: '0 1 45%', // Two-column layout
                            cursor: 'pointer'
                        });

                        var itemDetails = `
                        <p>Site ID: ${item.siteID}</p>
                        <p>Site Code: ${item.siteCode}</p>
                        <p>Site Name: ${item.siteName}</p>
                        <p>Site Category: ${item.siteCategory}</p>
                        <p>Email Address: ${item.contactInformation.emailAddress}</p>
                        <p>Telephone Number: ${item.contactInformation.branchTelephoneNumber}</p>
                        `;
                        resultItem.html(itemDetails);

                        // Store the item index as data attribute
                        resultItem.data('itemIndex', index);

                        resultItem.on('click', function () {
                            handleResultItemClick(item, resultItem);
                        });

                        results.append(resultItem);
                    });
                    $('#selectButton').show();
                } else {
                    results.show();
                    resultsCount.show();
                    resultsCount.text(`0 results present`);
                    results.append($('<div class="p-2" style="border:1px solid #ccc; height:5rem; width: 45%;">No results found</div>'));
                    $('#selectButton').hide();
                }
            },
            error: function (xhr, status, error) {
                console.error('Error ' + error);
            }
        });
    });
    $('#clearButton').on('click', function () {
        // Clear all inputs fields and hide updateSiteDetailsSection
        $('#searchInput').val('');
        $('#searchResults').empty();
        $('#searchResults').hide();
        $('#resultsCount').text('');
        $('#resultsCount').hide();
        $('#updateSiteDetailsSection').hide();
        $('.validation-summary-errors').empty();
        $('.field-validation-error').empty();
        $('#selectButton').hide();
    })
    $('#selectButton').on('click', function () {
        // Find the selected result item
        var selectedResultItem = $('.search-result-item.selected');
        if (selectedResultItem.length > 0) {
            var itemIndex = selectedResultItem.data('itemIndex');
            var selectedItemData = apiResultData[itemIndex];
            handleResultItemClick(selectedItemData, selectedResultItem);
            $('#updateSiteDetailsSection').show();
            // Scroll to the updateSiteDetailsSection
            $('html, body').animate({
                scrollTop: $('#updateSiteDetailsSection').offset().top
            }, 'fast');
        }
    });
});

