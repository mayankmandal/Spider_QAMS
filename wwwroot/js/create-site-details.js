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
function populateSponsorDropdown() {
    const sponsorSelect = $('#sponsorSelect');
    sponsorSelect.empty().append('<option disabled selected>Select Sponsor</option>');
    groupedData.forEach(sponsor => {
        sponsorSelect.append(new Option(sponsor.sponsorName, sponsor.sponsorId));
    });
    sponsorSelect.selectpicker('refresh'); // Refresh select picker after populating
}

$(document).ready(function () {
    // Initialize selectpicker for all select elements with the class `selectpicker`
    $('.selectpicker').selectpicker();

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

    // Populate Regions, SiteType, and Contacts based on selected Sponsor
    $('#sponsorSelect').change(function () {
        // Initially hide the Branch Type dropdown
        $('#branchTypeDiv').hide();

        const selectedSponsorId = $(this).val();
        const sponsorData = groupedData.find(s => s.sponsorId == selectedSponsorId);
        const regions = sponsorData.regions; // regions exists in sponsorData
        const siteTypes = sponsorData.siteTypes; // siteTypes exists in sponsorData
        const contacts = sponsorData.contactsList; // contacts exists in sponsorData

        // Populate Region Dropdown
        const regionSelect = $('#regionSelect');
        regionSelect.empty().append('<option disabled selected>Select Region</option>');
        regions.forEach(region => {
            regionSelect.append(new Option(region.regionName, region.regionId));
        });
        regionSelect.selectpicker('refresh'); // Refresh select picker after populating

        // Reset City and Location Dropdowns
        const citySelect = $('#citySelect');
        citySelect.empty().append('<option disabled selected>Select City</option>');
        citySelect.selectpicker('refresh'); // Refresh select picker

        const locationSelect = $('#locationSelect');
        locationSelect.empty().append('<option disabled selected>Select Location</option>');

        // Populate SiteType Dropdown
        const siteTypeSelect = $('#siteTypeSelect');
        siteTypeSelect.empty().append('<option disabled selected>Select Site Type</option>');
        siteTypes.forEach(siteType => {
            siteTypeSelect.append(new Option(siteType.siteTypeDescription, siteType.siteTypeId));
        });
        siteTypeSelect.selectpicker('refresh'); // Refresh select picker after populating

        // Populate Contact Dropdown
        const contactSelect = $('#contactSelect');
        contactSelect.empty().append('<option disabled selected>Select Contact</option>');;
        contacts.forEach(contact => {
            contactSelect.append(new Option(`${contact.name} (${contact.designation}) - ${contact.branchName}`, contact.contactId));
        });
        contactSelect.selectpicker('refresh'); // Refresh select picker after populating

        // Reset Branch Type
        const branchTypeSelect = $('#branchTypeSelect');
        branchTypeSelect.empty().append('<option disabled selected>Select Branch Type</option>');
        branchTypeSelect.selectpicker('refresh'); // Refresh select picker
    });

    // Populate Cities based on selected Region
    $('#regionSelect').change(function () {
        const selectedSponsorId = $('#sponsorSelect').val();
        const selectedRegionId = $(this).val();

        const cities = groupedData.find(s => s.sponsorId == selectedSponsorId)
            .regions.find(r => r.regionId == selectedRegionId).cities;

        const citySelect = $('#citySelect');
        citySelect.empty().append('<option disabled selected>Select City</option>');

        const locationSelect = $('#locationSelect');
        locationSelect.empty().append('<option disabled selected>Select Location</option>');
        locationSelect.selectpicker('refresh'); // Refresh location select picker after clearing

        cities.forEach(city => {
            citySelect.append(new Option(city.cityName, city.cityId));
        });
        citySelect.selectpicker('refresh'); // Refresh city select picker after populating
    });

    // Populate Locations based on selected City
    $('#citySelect').change(function () {
        const selectedSponsorId = $('#sponsorSelect').val();
        const selectedRegionId = $('#regionSelect').val();
        const selectedCityId = $(this).val();

        const locations = groupedData.find(s => s.sponsorId == selectedSponsorId)
            .regions.find(r => r.regionId == selectedRegionId)
            .cities.find(c => c.cityId == selectedCityId).locations;

        const locationSelect = $('#locationSelect');
        locationSelect.empty().append('<option disabled selected>Select Location</option>');

        locations.forEach(location => {
            const optionLocationHtml = `
                <option value="${location.locationId}">
                    <div class="d-block text-left">
                        <span class="h5 d-block">${location.streetName}</span> <!-- First line large -->
                        <span class="small d-block">${location.location}</span> <!-- Second line small -->
                        <span class="d-block">${location.districtName} - ${location.branchName}</span> <!-- Third line medium -->
                    </div>
                </option>
            `;
            locationSelect.append(optionLocationHtml);
        });
        locationSelect.selectpicker('refresh'); // Refresh location select picker after populating
    });

    // Populate BranchType based on selected SiteType
    $('#siteTypeSelect').change(function () {
        const selectedSponsorId = $('#sponsorSelect').val();
        const selectedSiteTypeId = $(this).val();

        // Get the selected site type object
        const selectedSiteType = groupedData.find(s => s.sponsorId == selectedSponsorId)
            .siteTypes.find(st => st.siteTypeId == selectedSiteTypeId);

        $('#branchTypeSelect').empty().append('<option disabled selected>Select Branch Type</option>');
        $('#branchTypeSelect').selectpicker('refresh'); // Refresh branch type select picker after clearing

        if (selectedSiteType && selectedSiteType.branchTypes && selectedSiteType.branchTypes.length > 0) {
            // If the selected site type has branch types, show and populate the Branch Type dropdown
            $('#branchTypeDiv').show(); // Show the branch type div

            const branchTypes = selectedSiteType.branchTypes;
            branchTypes.forEach(branchType => {
                $('#branchTypeSelect').append(new Option(branchType.description, branchType.branchTypeId));
            });
            $('#branchTypeSelect').selectpicker('refresh'); // Refresh branch type select picker after populating
        } else {
            // If there are no branch types, hide the Branch Type dropdown
            $('#branchTypeDiv').hide(); // Hide the branch type div
            $('#branchTypeSelect').empty().append('<option disabled selected>Select Branch Type</option>'); // Reset the dropdown
        }
    });

    // Initialize the Sponsor dropdown on page load
    populateSponsorDropdown();
});