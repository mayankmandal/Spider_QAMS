// Initialize terminalMap globally if it's not already defined
let terminalMap; // Declare terminalMap
let terminalMarkerGroup; // Declare terminalMarkerGroup

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

// Populate Sponsor Dropdown
function populateSponsorDropdown(selectedSponsorId) {
    const sponsorSelect = $('#sponsorSelect');
    sponsorSelect.empty().append('<option disabled selected>Select Sponsor</option>');

    groupedData.forEach(sponsor => {
        const option = new Option(sponsor.sponsorName, sponsor.sponsorId);
        if (String(sponsor.sponsorId) === selectedSponsorId) {
            option.selected = true; // Set as selected if the IDs match
        }
        sponsorSelect.append(option);
    });
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
}

// Reset Dropdown
function resetDropdown(selector, placeholder) {
    $(selector).empty().append(`<option disabled selected>${placeholder}</option>`);
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

    checkAndRenderMap();

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

    // Cancel button for map modal
    $('#cancelButton').on('click', function () {
        $('#mapModel').modal('hide'); // Hides the modal
    });
});

// Populate Regions, SiteType, and Contacts based on selected Sponsor
$('#sponsorSelect').change(function () {
    // Initially hide the Branch Type dropdown
    resetDropdown('#branchTypeSelect', 'Select Branch Type');
    $('#branchTypeDiv').hide();

    resetDropdown('#regionSelect', 'Select Region');
    resetDropdown('#citySelect', 'Select City');
    resetDropdown('#locationSelect', 'Select Location');

    selectedSponsorId = $(this).val();
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