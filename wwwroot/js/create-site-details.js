
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