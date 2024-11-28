let isInitializing = true; // Flag to prevent triggering change event on initial load

let terminalMap; // Main map instance
let terminalMarkerGroup;
const originalMapContainer = 'mapContainer'; // Original map container ID
const modalMapContainer = 'fullMapContainer'; // Modal map container ID

// Initialize the map with a given container ID
function initializeMap(latitude, longitude, containerId) {
    const map = L.map(containerId).setView([latitude, longitude], 10);
    map.zoomControl.setPosition('bottomright');

    // Add Google Map layers
    const googleLayers = {
        "Google Street Map": L.tileLayer('https://{s}.google.com/vt?lyrs=m&x={x}&y={y}&z={z}', { maxZoom: 22, subdomains: ['mt0', 'mt1', 'mt2', 'mt3'] }).addTo(map),
        "Google Satellite Map": L.tileLayer('https://{s}.google.com/vt?lyrs=s&x={x}&y={y}&z={z}', { maxZoom: 22, subdomains: ['mt0', 'mt1', 'mt2', 'mt3'] }),
        "Google Terrain Map": L.tileLayer('https://{s}.google.com/vt?lyrs=p&x={x}&y={y}&z={z}', { maxZoom: 22, subdomains: ['mt0', 'mt1', 'mt2', 'mt3'] }),
        "Open Street Map": L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 22 }),
    };

    // Layer control
    L.control.layers(googleLayers).addTo(map);

    // Add a marker and circle
    terminalMarkerGroup = L.layerGroup().addTo(map);
    const marker = L.marker([latitude, longitude]);
    const circle = L.circle([latitude, longitude], { radius: 10, color: '#e83815', fillOpacity: 0.4 }).addTo(terminalMarkerGroup);
    marker.addTo(terminalMarkerGroup);

    return map;
}

// Clear and reset map
function clearAndResetMap() {
    if (terminalMarkerGroup) terminalMarkerGroup.clearLayers();
    if (terminalMap) {
        terminalMap.remove();
        terminalMap = null;
    }
}

// Render map in the original container
function renderMapInOriginalContainer(latitude, longitude) {
    clearAndResetMap();
    terminalMap = initializeMap(latitude, longitude, originalMapContainer);
}

// Render map in the modal
function renderMapInModal(latitude, longitude) {
    clearAndResetMap();
    terminalMap = initializeMap(latitude, longitude, modalMapContainer);

    // Adjust map size when modal is displayed
    $('#fullMapModal').on('shown.bs.modal', function () {
        if (terminalMap) {
            terminalMap.invalidateSize();
        }
    });
}

// Populate Sponsor Dropdown
function populateSponsorDropdown(selectedSponsorId) {
    const sponsorSelect = $('#sponsorSelect');

    // Clear current options and add default "Select Sponsor" option
    sponsorSelect.empty().append('<option disabled>Select Sponsor</option>');

    // Populate dropdown with options from groupedData
    groupedData.forEach(sponsor => {
        const option = new Option(sponsor.sponsorName, sponsor.sponsorId);
        sponsorSelect.append(option);
    });

    // Refresh selectpicker to update the dropdown UI
    sponsorSelect.selectpicker('refresh');

    // Set the selected option if selectedSponsorId is provided
    if (selectedSponsorId) {
        sponsorSelect.selectpicker('val', selectedSponsorId); // Select the option by value
    }
}

// Helper to Populate Dropdown with selected value if available
function populateDropdown(selector, items, labelKey, valueKey, selectedValue = null) {
    const dropdown = $(selector);
    dropdown.empty().append('<option disabled selected>Select</option>');
    items.forEach(item => {
        const label = typeof labelKey === 'function' ? labelKey(item) : item[labelKey];
        const option = new Option(label, item[valueKey]);
        if (item[valueKey] == selectedValue) option.selected = true;
        dropdown.append(option);
    });
    dropdown.selectpicker('refresh'); // Refresh selectpicker after populating
}

// Reset Dropdown
function resetDropdown(selector, placeholder) {
    $(selector).empty().append(`<option disabled selected>${placeholder}</option>`);
    $(selector).selectpicker('refresh'); // Refresh selectpicker after reset
}

// Formatter for Contact Options
function contactFormatter(contact) {
    return `${contact.name} (${contact.designation}) - ${contact.branchName}`;
}

// Formatter for Location Options
function locationFormatter(location) {
    return `${location.streetName}, ${location.location}, ${location.districtName} - ${location.branchName}`;
}

$(document).ready(function () {

    $('.selectpicker').selectpicker(); // Initialize selectpicker for all elements with `selectpicker` class

    // Select the longitude and latitude input fields
    const gpsLongInput = $('input[id="SiteDetailVM_GPSLong"]');
    const gpsLattInput = $('input[id="SiteDetailVM_GPSLatt"]');

    // Function to check inputs and render the map
    function checkAndRenderMap() {
        const longitude = gpsLongInput.val();
        const latitude = gpsLattInput.val();

        if (longitude && latitude) {
            renderMapInOriginalContainer(latitude, longitude);
        }
    }

    // Initialize map on input changes
    gpsLongInput.on('input', checkAndRenderMap);
    gpsLattInput.on('input', checkAndRenderMap);

    // Initial map rendering on page load
    checkAndRenderMap();

    // Enlarge map button click listener
    $('#mapContainer').on('click', function () {
        const longitude = gpsLongInput.val();
        const latitude = gpsLattInput.val();

        if (longitude && latitude) {
            $('#fullMapModal').modal('show');
            renderMapInModal(latitude, longitude);
        }
    });

    // Restore map to original container when modal is closed
    $('#fullMapModal').on('hidden.bs.modal', function () {
        const longitude = gpsLongInput.val();
        const latitude = gpsLattInput.val();

        if (longitude && latitude) {
            renderMapInOriginalContainer(latitude, longitude);
        }
    });

    selectedSponsorId = $('#sponsorSelect').val() || null;
    selectedRegionId = $('#regionSelect').val() || null;
    selectedCityId = $('#citySelect').val() || null;
    selectedLocationId = $('#locationSelect').val() || null;
    selectedContactId = $('#contactSelect').val() || null;
    selectedSiteTypeId = $('#siteTypeSelect').val() || null;
    selectedBranchTypeId = $('#branchTypeSelect').val() || null;

    // Populate Sponsor Dropdown and trigger change event to load dependent data
    populateSponsorDropdown(selectedSponsorId);

    if (!selectedBranchTypeId) {
        $('#branchTypeDiv').hide();
    }

    // Enlarge image button functionality
    $('#enlargesiteProfileImageBtn').on('click', function () {
        $('#siteImageModal').modal('show'); // Shows the image modal
    });

    // Cancel button for image modal
    $('#cancelImageButton').on('click', function () {
        $('#siteImageModal').modal('hide'); // Hides the image modal
    });

    // Remove the initializing flag after initial population
    isInitializing = false;
});

// Populate Regions, SiteType, and Contacts based on selected Sponsor
$('#sponsorSelect').change(function () {
    // Initialize selectedSponsorId with the current selection before returning if still initializing
    selectedSponsorId = $(this).val();

    // Skip if still initializing (initial page load)
    if (isInitializing) {
        return;
    }

    // Initially hide the Branch Type dropdown
    resetDropdown('#branchTypeSelect', 'Select Branch Type');
    $('#branchTypeDiv').hide();

    resetDropdown('#regionSelect', 'Select Region');
    resetDropdown('#citySelect', 'Select City');
    resetDropdown('#locationSelect', 'Select Location');

    const sponsorData = groupedData.find(s => s.sponsorId == selectedSponsorId);

    if (sponsorData) {
        populateDropdown('#regionSelect', sponsorData.regions, 'regionName', 'regionId', selectedRegionId);
        populateDropdown('#siteTypeSelect', sponsorData.siteTypes, 'siteTypeDescription', 'siteTypeId', selectedSiteTypeId);
        populateDropdown('#contactSelect', sponsorData.contactsList, contactFormatter, 'contactId', selectedContactId);
    }

    if (!selectedRegionId) resetDropdown('#citySelect', 'Select City');
    if (!selectedCityId) resetDropdown('#locationSelect', 'Select Location');
});

// Populate Cities based on selected Region
$('#regionSelect').change(function () {
    resetDropdown('#citySelect', 'Select City');
    resetDropdown('#locationSelect', 'Select Location');

    selectedRegionId = $(this).val();
    const sponsorData = groupedData.find(s => s.sponsorId == selectedSponsorId);
    const cities = sponsorData?.regions.find(r => r.regionId == selectedRegionId)?.cities || [];

    populateDropdown('#citySelect', cities, 'cityName', 'cityId', selectedCityId);
    if (!selectedCityId) resetDropdown('#locationSelect', 'Select Location');
});

// Populate Locations based on selected City
$('#citySelect').change(function () {
    resetDropdown('#locationSelect', 'Select Location');

    selectedCityId = $(this).val();
    const sponsorData = groupedData.find(s => s.sponsorId == selectedSponsorId);
    const locations = sponsorData?.regions
        .find(r => r.regionId == selectedRegionId)?.cities
        .find(c => c.cityId == selectedCityId)?.locations || [];

    populateDropdown('#locationSelect', locations, locationFormatter, 'locationId', selectedLocationId);
});

// Populate Branch Type based on selected Site Type
$('#siteTypeSelect').change(function () {
    resetDropdown('#branchTypeSelect', 'Select Branch Type');

    selectedSiteTypeId = $(this).val();
    const sponsorData = groupedData.find(s => s.sponsorId == selectedSponsorId);
    const siteType = sponsorData?.siteTypes.find(st => st.siteTypeId == selectedSiteTypeId);

    if (siteType && siteType.branchTypes.length) {
        $('#branchTypeDiv').show();
        populateDropdown('#branchTypeSelect', siteType.branchTypes, 'description', 'branchTypeId', selectedBranchTypeId);
    } else {
        $('#branchTypeDiv').hide();
        resetDropdown('#branchTypeSelect', 'Select Branch Type');
    }
});