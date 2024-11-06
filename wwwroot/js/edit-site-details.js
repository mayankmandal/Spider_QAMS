// Initialize terminalMap globally if it's not already defined
let terminalMap; // Declare terminalMap
let terminalMarkerGroup; // Declare terminalMarkerGroup
let selectedSponsorId = 0;
let selectedRegionId = 0;
let selectedCityId = 0;
let selectedLocationId = 0;
let selectedContactId = 0;
let selectedSiteTypeId = 0;
let selectedBranchTypeId = 0;
const container = $('#uploadedImagesContainer');
const categorySelect = $('#sitePicCategory');
const validationMessage = $('#categoryValidation');


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

function loadInitialImages() {
    ImagesData.forEach((category, index) => {
        // Create the category section with file input but hide it if no images exist
        createCategorySection(category, index);

        // Only display images if they exist
        if (category.imagePaths.length > 0) {
            category.imagePaths.forEach((imagePath, imgIndex) => {
                displayImagePreview(category.picCatID, imagePath, index, imgIndex);
            });
        } else {
            // Initially hide the images and file input
            toggleCategoryVisibility(category.picCatID, false);
        }
    });
}

function addImageCategory() {
    const categorySelect = document.getElementById("sitePicCategory");
    const selectedCategory = categorySelect.options[categorySelect.selectedIndex];
    const description = selectedCategory.dataset.description;
    const categoryID = selectedCategory.value;

    if (categoryID) {
        createCategorySection({
            PicCatID: categoryID,
            Description: description,
            ImagePaths: []
        });
    } else {
        document.getElementById("categoryValidation").style.display = "block";
        document.getElementById("categoryValidation").innerText = "Please select a category first!";
    }
}

function createCategorySection(category, index) {
    const container = document.getElementById("uploadedImagesContainer");
    const categorySection = document.createElement("div");
    categorySection.className = `mb-4 category-${category.picCatID}`;
    categorySection.innerHTML = `
        <h5>${category.description}</h5>
        <input type="hidden" name="SitePicCategoryList[${index}].PicCatID" value="${category.picCatID}" />
        <input type="hidden" name="SitePicCategoryList[${index}].Description" value="${category.description}" />
        
        <input type="file" name="SitePicCategoryList[${index}].Images" class="form-control upload-image" accept="image/*" multiple 
               onchange="handleFileUpload(event, ${category.picCatID}, ${index})" />

        <div class="row image-preview mt-3 g-3" id="preview-${category.picCatID}"></div>
        
        <button type="button" class="btn btn-secondary mt-2 add-more-images" onclick="toggleCategoryVisibility(${category.picCatID}, true)" style="display:none;">Add Images</button>
        <button type="button" class="btn btn-danger mt-2 remove-category" onclick="removeCategory(${category.picCatID})">Remove ${category.description}</button>
    `;
    container.appendChild(categorySection);
}

function toggleCategoryVisibility(categoryID, show) {
    const categoryContainer = document.querySelector(`.category-${categoryID}`);

    // Check if categoryContainer exists before attempting to change its display style
    if (categoryContainer) {
        categoryContainer.style.display = show ? "block" : "none"; // Show or hide the entire category
    }
}

function handleFileUpload(event, categoryID, index) {
    const files = event.target.files;
    Array.from(files).forEach((file, imgIndex) => {
        const reader = new FileReader();
        reader.onload = function (e) {
            displayImagePreview(categoryID, e.target.result, index, imgIndex);
        };
        reader.readAsDataURL(file);
    });
}

function displayImagePreview(categoryID, imagePath, catIndex, imgIndex) {
    const previewContainer = document.getElementById(`preview-${categoryID}`);
    const imageElement = document.createElement("div");
    imageElement.className = "col-md-6 uploaded-image-container d-flex align-items-center mb-3";
    imageElement.innerHTML = `
        <span class="me-2">${imgIndex + 1}.</span>
        <img src="${imagePath}" class="img-thumbnail" style="max-width: 150px; margin-right: 20px;" />
        <div style="flex-grow: 1;">
            <label class="form-label">Description</label>
            <input type="text" name="SitePicCategoryList[${catIndex}].ImageComments[${imgIndex}]" class="form-control" placeholder="Add description" />
            <input type="hidden" name="SitePicCategoryList[${catIndex}].ImageDeletes[${imgIndex}]" value="false" />
            <button type="button" class="btn btn-danger btn-sm remove-individual-image mt-2" onclick="removeImage(this, ${catIndex}, ${imgIndex})">
                Remove Image
            </button>
        </div>
        <input type="hidden" name="SitePicCategoryList[${catIndex}].ImagePaths[${imgIndex}]" value="${imagePath}" />
    `;
    previewContainer.appendChild(imageElement);
}

function removeImage(button, catIndex, imgIndex) {
    const imageContainer = button.closest(".uploaded-image-container");
    imageContainer.classList.remove("d-flex"); // Remove the d-flex class
    imageContainer.style.display = "none";
    document.querySelector(`[name="SitePicCategoryList[${catIndex}].ImageDeletes[${imgIndex}]"]`).value = "true";
}

function addMoreImages(categoryID, index) {
    const fileInput = document.querySelector(`input[name="SitePicCategoryList[${index}].Images"]`);
    fileInput.click();
}

function removeCategory(categoryID) {
    const categorySection = document.querySelector(`.category-${categoryID}`);
    categorySection.style.display = "none";

    // Find the index of the category in ImagesData using the provided categoryID
    const categoryData = ImagesData.find(data => data.picCatID === categoryID);

    // If the category exists, set each imageDeletes entry to true
    if (categoryData && categoryData.imagePaths.length > 0) {
        categoryData.imageDeletes = categoryData.imagePaths.map(() => true);
    }

    // Update the DOM inputs based on ImageDeletes values in the categoryData
    categoryData.imageDeletes.forEach((_, index) => {
        const deleteInput = document.querySelector(`[name="SitePicCategoryList[${categoryData.picCatID}].ImageDeletes[${index}]"]`);
        if (deleteInput) {
            deleteInput.value = "true";
        }
    });
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
    populateSponsorDropdown();
    if (selectedSponsorId) $('#sponsorSelect').val(selectedSponsorId).change();

    // Initial setup: Hide all categories on load
    $('.category-container').hide();
});

document.addEventListener("DOMContentLoaded", function () {
    loadInitialImages();  // Load any existing images when the page loads
    document.getElementById("addImages").addEventListener("click", addImageCategory);
});

// Assuming your cancel button has an id of "cancelButton"
$('#cancelButton').on('click', function () {
    $('#mapModel').modal('hide'); // Hides the modal
});

// Populate Regions, SiteType, and Contacts based on selected Sponsor
$('#sponsorSelect').change(function () {
    selectedSponsorId = $(this).val();
    const sponsorData = groupedData.find(s => s.sponsorId == selectedSponsorId);

    if (sponsorData) {
        populateDropdown('#regionSelect', sponsorData.regions, 'regionName', 'regionId', selectedRegionId);
        populateDropdown('#siteTypeSelect', sponsorData.siteTypes, 'siteTypeDescription', 'siteTypeId', selectedSiteTypeId);
        populateDropdown('#contactSelect', sponsorData.contactsList, contactFormatter, 'contactId', selectedContactId);
    }

    if (!selectedRegionId) resetDropdown('#citySelect', 'Select City');
    if (!selectedCityId) resetDropdown('#locationSelect', 'Select Location');
    resetDropdown('#branchTypeSelect', 'Select Branch Type');
});

// Populate Cities based on selected Region
$('#regionSelect').change(function () {
    selectedRegionId = $(this).val();
    const sponsorData = groupedData.find(s => s.sponsorId == selectedSponsorId);
    const cities = sponsorData?.regions.find(r => r.regionId == selectedRegionId)?.cities || [];

    populateDropdown('#citySelect', cities, 'cityName', 'cityId', selectedCityId);
    if (!selectedCityId) resetDropdown('#locationSelect', 'Select Location');
});

// Populate Locations based on selected City
$('#citySelect').change(function () {
    selectedCityId = $(this).val();
    const sponsorData = groupedData.find(s => s.sponsorId == selectedSponsorId);
    const locations = sponsorData?.regions
        .find(r => r.regionId == selectedRegionId)?.cities
        .find(c => c.cityId == selectedCityId)?.locations || [];

    populateDropdown('#locationSelect', locations, locationFormatter, 'locationId', selectedLocationId);
});

// Populate Branch Type based on selected Site Type
$('#siteTypeSelect').change(function () {
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

// Handle image preview removal
$('.remove-individual-image').on('click', function () {
    $(this).closest('.uploaded-image-container').remove();
});

// Handle category selection and enable corresponding section
$('#addImages').on('click', function () {
    const selectedCategory = categorySelect.val();
    const selectedDescription = categorySelect.find(':selected').data('description');
    const existingCategoryDiv = container.find(`.category-${selectedCategory}`);

    if (selectedCategory) {
        validationMessage.hide();

        if (existingCategoryDiv.is(':visible')) {
            validationMessage.text(`Images for ${selectedDescription} already added.`).show();
            return;
        }

        // Enable and show the hidden category
        existingCategoryDiv.show();
    } else {
        validationMessage.text('Please select a category first.').show();
    }
});

// Handle file input change to generate image previews
$('.upload-image').on('change', function () {
    const previewContainer = $(this).siblings('.image-preview');
    previewContainer.empty();

    Array.from(files).forEach((file, index) => {
        const reader = new FileReader();
        reader.onload = function (e) {
            const imgPreview = `
                <div class="col-md-6 uploaded-image-container d-flex align-items-center mb-3">
                    <img src="${e.target.result}" class="img-thumbnail" style="max-width: 150px; margin-right: 20px;" />
                    <input type="hidden" name="SitePicCategoryList[${categoryIndex}].ImageDeletes[${index}]" value="true" /> <!-- Set to true on upload -->
                    <div style="flex-grow: 1;">
                        <label class="form-label">Description for ${file.name}</label>
                        <input type="text" name="SitePicCategoryList[${categoryIndex}].ImageComments[${index}]" class="form-control" placeholder="Add description" />
                        <button type="button" class="btn btn-danger btn-sm remove-individual-image mt-2" data-index="${index}">Remove Image</button>
                    </div>
                </div>`;
            previewContainer.append(imgPreview);

            attachRemoveImageEvent(previewContainer);
        };
        reader.readAsDataURL(file);
    });
});
// Remove category section when clicked
$('.remove-category').on('click', function () {
    const categoryDiv = $(this).closest('.mb-4');
    const catIndex = categoryDiv.data('index');

    categoryDiv.hide().find('.upload-image').val('');
    categoryDiv.find('.image-preview').empty();

    // Set ImageDeletes to false for all images in the category
    categoryDiv.find(`input[name="SitePicCategoryList[${catIndex}].ImageDeletes[]"]`).val("false");
});

// Auto-trigger file input when 'Add More Images' is clicked
$('.add-more-images').on('click', function () {
    $(this).siblings('.upload-image').click();
});