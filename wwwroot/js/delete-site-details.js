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

    // Enlarge image button functionality
    $('#enlargesiteProfileImageBtn').on('click', function () {
        $('#siteImageModal').modal('show'); // Shows the image modal
    });

    // Cancel button for image modal
    $('#cancelImageButton').on('click', function () {
        $('#siteImageModal').modal('hide'); // Hides the image modal
    });
});


