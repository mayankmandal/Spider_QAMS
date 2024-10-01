// Function to create a Leaflet map with a marker and popup
function createLeafletMap(terminalData) {
    terminalMap = L.map('mapContainer').setView([terminalData.latitude, terminalData.longitude], 18);
    terminalMap.zoomControl.setPosition('bottomright');
    // Create a layer group for markers
    terminalMarkerGroup = L.layerGroup().addTo(terminalMap);

    // Add a marker to the map
    var singleMarker = L.marker([terminalData.latitude, terminalData.longitude]);

    // Customize tooltip content with HTML
    var tooltipContent = `
    <div class="map-tooltip-content">
        <div class="map-bank-name"><i class="fas fa-university" style="margin-right: 5px;"></i>${terminalData.bankNameEn}</div>
        <div class="map-address-info">
            <div><i class="fas fa-city" style="margin-right: 5px;"></i>${terminalData.cityAr}</div>
            <div>&nbsp;|&nbsp;</div>
            <div><i class="fas fa-road" style="margin-right: 5px;"></i>${terminalData.streetAr}</div>
            <div>&nbsp;|&nbsp;</div>
            <div><i class="fas fa-building" style="margin-right: 5px;"></i>${terminalData.districtAr}</div>
        </div>
    </div>`;

    // Set an offset to position the tooltip correctly
    var tooltipOffset = L.point(0, -28); // Adjust the offset as needed

    // Create a red transparent circle around the marker
    var circle = L.circle([terminalData.latitude, terminalData.longitude], {
        radius: 10, 
        color: '#e83815',
        fillOpacity: 0.4,
    });

    // Add the circle to the layer group
    circle.addTo(terminalMarkerGroup);

    singleMarker.bindTooltip(tooltipContent, { direction: 'top', permanent: true, opacity: 0.7, offset: tooltipOffset }).openTooltip();

    // Add the marker to the layer group
    singleMarker.addTo(terminalMarkerGroup);

    // Google Map Streets Layer
    var googleStreets = L.tileLayer('https://{s}.google.com/vt?lyrs=m&x={x}&y={y}&z={z}', {
        maxZoom: 22,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
    });

    // Google Map Hybrid Layer
    var googleHybrid = L.tileLayer('https://{s}.google.com/vt?lyrs=s,h&x={x}&y={y}&z={z}', {
        maxZoom: 22,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
    });

    // Google Map Satellite Layer
    var googleSatellite = L.tileLayer('https://{s}.google.com/vt?lyrs=s&x={x}&y={y}&z={z}', {
        maxZoom: 22,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
    });

    // Google Map Terrain Layer
    var googleTerrain = L.tileLayer('https://{s}.google.com/vt?lyrs=p&x={x}&y={y}&z={z}', {
        maxZoom: 22,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
    });

    //Open Street Map Layer
    var openStreetMap = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 22,
    });

    // Layer Control
    var baseLayers = {
        "Google Street Map": googleStreets,
        "Google Satellite Map": googleSatellite,
        "Google Terrain Map": googleTerrain,
        "Google Hybrid Map": googleHybrid,
        "Open Street Map": openStreetMap,
    };

    var overLaysMarker = {
        "Marker": singleMarker
    };

    // Set Google Street Map as the default layer
    googleStreets.addTo(terminalMap);

    // Add layer control
    L.control.layers(baseLayers, overLaysMarker).addTo(terminalMap);
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
    }
}

//Function to retrieves and displays terminal data through an AJAX request.
function showTerminalData(terminalId) {
    $.ajax({
        url: '/api/SiteSelection/GetTerminalDetails/' + terminalId,
        headers: {
            'Authorization': 'Bearer ' + tokenC
        },
        type: 'GET',
        success: function (result) {
            renderTerminalDetails(result);
        },
        error: function (message) {
            console.log(message);
        }
    });
}

//Function rendering the details of a terminal in a modal.
function renderTerminalDetails(terminalResult) {
    // Check if the map container already has a map instance
    var existingMap = L.DomUtil.get('mapContainer');

    // If a map already exists, remove it before initializing a new one
    if (existingMap) {
        existingMap._leaflet_id = null;
    }

    // Initialize the map
    createLeafletMap(terminalResult);

    // Listen for the Bootstrap modal shown event and call invalidateSize when the modal is fully shown
    $('#terminalDetailsModal').on('shown.bs.modal', function () {
        terminalMap.invalidateSize();
    });
}

$(document).ready(function () {
    // Populate Sponsor Dropdown
    function populateSponsorDropdown() {
        const sponsorSelect = $('#sponsorSelect');
        sponsorSelect.empty().append('<option disabled selected>Select Sponsor</option>');
        groupedData.forEach(sponsor => {
            sponsorSelect.append(new Option(sponsor.sponsorName, sponsor.sponsorId));
        });
    }

    // Populate Regions, SiteType, and Contacts based on selected Sponsor
    $('#sponsorSelect').change(function () {
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

        // Reset City and Location Dropdowns
        const citySelect = $('#citySelect');
        citySelect.empty().append('<option disabled selected>Select City</option>');

        const locationSelect = $('#locationSelect');
        locationSelect.empty().append('<option disabled selected>Select Location</option>');

        // Populate SiteType Dropdown
        const siteTypeSelect = $('#siteTypeSelect');
        siteTypeSelect.empty().append('<option disabled selected>Select Site Type</option>');
        siteTypes.forEach(siteType => {
            siteTypeSelect.append(new Option(siteType.siteTypeDescription, siteType.siteTypeId));
        })

        // Populate Contact Dropdown
        const contactSelect = $('#contactSelect');
        contactSelect.empty().append('<option disabled selected>Select Contact</option>');;
        contacts.forEach(contact => {
            contactSelect.append(new Option(`${contact.name} (${contact.designation}) - ${contact.branchName}`, contact.contactId));
        });

        // Reset Branch Type
        const branchTypeSelect = $('#branchTypeSelect');
        branchTypeSelect.empty().append('<option disabled selected>Select Branch Type</option>');
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

        cities.forEach(city => {
            citySelect.append(new Option(city.cityName, city.cityId));
        });
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
            locationSelect.append(new Option(`${location.streetName} ${location.location} ${location.districtName} - ${location.branchName}` , location.locationId));
        });
    });

    // Populate BranchType based on selected SiteType
    $('#siteTypeSelect').change(function () {
        const selectedSponsorId = $('#sponsorSelect').val();
        const selectedSiteTypeId = $(this).val();

        // Assuming branchTypes are stored under the sponsor data and grouped by siteType
        const branchTypes = groupedData.find(s => s.sponsorId == selectedSponsorId)
            .siteTypes.find(st => st.siteTypeId == selectedSiteTypeId).branchTypes;

        const branchTypeSelect = $('#branchTypeSelect');
        branchTypeSelect.empty().append('<option disabled selected>Select Branch Type</option>');

        branchTypes.forEach(branchType => {
            branchTypeSelect.append(new Option(branchType.description, branchType.branchTypeId));
        });
    });

    // Initialize the Sponsor dropdown on page load
    populateSponsorDropdown();
});