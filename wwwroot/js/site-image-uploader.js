$(document).ready(function () {
    // Initialize Lightbox2 with all options enabled
    lightbox.option({
        'alwaysShowNavOnTouchDevices': true,
        'albumLabel': 'Image %1 of %2',
        'disableScrolling': true,
        'fadeDuration': 600,
        'fitImagesInViewport': true,
        'imageFadeDuration': 600,
        'positionFromTop': 50,
        'resizeDuration': 700,
        'showImageNumberLabel': true,
        'wrapAround': true
    });

    // Render categories dropdown
    sitePicCategories.forEach(category => {
        $('#categorySelect').append(`<option value="${category.picCatID}">${category.description}</option>`);
    });

    $('.selectpicker').selectpicker('refresh'); // Refresh select picker

    // Render existing categories and images
    existingImages.forEach(category => {
        // Call addCategorySection for each category that has existing images
        addCategorySection(category.picCatId, category.description);
    });

    // When category changes, add a new section for the selected category
    $('#categorySelect').on('change', function () {
        const selectedCatId = $(this).val();
        const selectedCatText = $("#categorySelect option:selected").text();

        const categorySection = $(`#category-${selectedCatId}`);

        if (categorySection.length > 0) {
            // If the section exists and is hidden, show it instead of re-adding
            if (categorySection.is(':hidden')) {
                categorySection.closest('.col-md-6').show();  // Show the entire column
            }
        } else {
            // If the section doesn't exist, add a new one
            addCategorySection(selectedCatId, selectedCatText);
        }
    });

    function addCategorySection(categoryId, categoryName) {
        // Create the category section card inside a Bootstrap column
        const sectionDiv = $(`
            <div class="col-md-6" id="col-${categoryId}">
                <div class="card" id="category-${categoryId}">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <h5>${categoryName}</h5>
                        <button type="button" class="btn btn-danger btn-sm btn-round remove-category" data-category-id="${categoryId}">Remove</button>
                    </div>
                    <div class="card-body category-section" id="category-${categoryId}">
                    </div>
                </div>
            </div>
            `);

        // Add hidden inputs for PicCatId and Description
        const hiddenPicCatId = $(`<input type="hidden" name="SiteImageUploaderVM.SitePicCategoryList[${categoryId}].PicCatId" value="${categoryId}" />`);
        const hiddenDescription = $(`<input type="hidden" name="SiteImageUploaderVM.SitePicCategoryList[${categoryId}].Description" value="${categoryName}" />`);

        sectionDiv.find('.category-section').append(hiddenPicCatId, hiddenDescription);

        // Add file input for uploading images
        const fileInput = $(`
            <label class="file-input-label border border-default rounded p-2 d-flex align-items-center" style="box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2) !important;">
                <div class="material-icons" style="font-size: 36px;">attach_file</div>
                <span style="font-size: 16px; margin-left: 8px;">Choose Files</span>
                <input type="file" multiple class="d-none file-input" data-category-id="${categoryId}" />
            </label>
            `);

        const galleryDiv = $(`
        <div class="gallery d-flex flex-wrap" 
             style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px;">
        </div>
        `);

        // Load existing images into the gallery if any
        const existingImagesForCategory = existingImages.find(c => c.picCatId == categoryId);
        if (existingImagesForCategory) {
            existingImagesForCategory.images.forEach(image => {
                galleryDiv.append(createGalleryItem(image.filePath, image.fileDescription, image.fileName, categoryId, image.sitePicID));
            });
            // Reinitialize Lightbox to make it aware of the new image
            lightbox.init();
        }

        // Append elements to the category card
        sectionDiv.find('.category-section').append(fileInput, galleryDiv);

        // Ensure that #imageUploadSection has a row wrapper for Bootstrap columns
        $('#imageUploadSection').addClass('row').append(sectionDiv);

        // Handle file input change event
        fileInput.find('input[type="file"]').on('change', function () {
            handleFileSelect(this.files, galleryDiv, categoryId);
        });
    }

    function createGalleryItem(filePath, description, fileName, categoryId, uniqueFileId = null) {
        const link = $(`
        <a href="${filePath}" data-lightbox="gallery-${categoryId}" data-title="${description}" class="position-relative d-flex">
            <img src="${filePath}" class="img-thumbnail m-2" style="width: auto; height: 10rem;" alt="${description}" />
        </a>
    `);

        const commentInput = $(`
        <input type="text" class="form-control mb-2" placeholder="Add a description" 
               value="${description || ''}" data-unique-file-id="${uniqueFileId}" 
               data-file-name="${fileName}" />
    `);

        const deleteButton = $(`<!-- Delete button overlay -->
            <i class="material-icons text-danger position-absolute delete-image-btn" data-unique-file-id="${uniqueFileId}" style="top: 1px; right: 1px; cursor: pointer;">cancel</i>
            `);

        const container = $('<div class="gallery-item position-relative text-center"></div>');
        container.append(link).append(commentInput).append(deleteButton);

        return container;
    }

    function handleFileSelect(files, galleryDiv, categoryId) {
        $.each(files, (i, file) => {
            const reader = new FileReader();
            reader.onload = function (e) {
                const imagePath = e.target.result;

                // Create gallery item with Lightbox and add to gallery
                const newGalleryItem = createGalleryItem(imagePath, '', file.name, categoryId);
                galleryDiv.append(newGalleryItem);

                // Reinitialize Lightbox to make it aware of the new image
                lightbox.init();
            };
            reader.readAsDataURL(file);
        });
    }

    // Remove category section
    $(document).on('click', '.remove-category', function () {
        const categoryId = $(this).data('category-id');

        // Trigger delete for each image in the category section
        $(`#category-${categoryId} .gallery-item .delete-image-btn`).each(function () {
            $(this).trigger('click');
        });

        // Remove the entire column div
        $(`#col-${categoryId}`).hide();
    });

    $(document).on('click', '.delete-image-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        const galleryItem = $(this).closest('.gallery-item');
        galleryItem.find('input[type="text"]').attr('data-is-deleted', 'true').hide();
        galleryItem.find('img').hide();
        $(this).hide();
    });

    // Submit button event
    $('#submitButton').on('click', function (event) {
        event.preventDefault();

        const formData = new FormData();
        formData.append("SiteImageUploaderVM.SiteId", siteId);

        // Add each category and its images
        $('.category-section').each(function (index) {
            const catId = $(this).attr('id').split('-')[1];
            const galleryItems = $(this).find('.gallery-item');

            // Collect PicCatId and Description (from hidden inputs)
            const picCatId = $(this).find(`input[name="SiteImageUploaderVM.SitePicCategoryList[${catId}].PicCatId"]`).val();
            const categoryDescription = $(this).find(`input[name="SiteImageUploaderVM.SitePicCategoryList[${catId}].Description"]`).val();

            galleryItems.each(function (galleryIndex) {
                // Check if the image is marked as deleted
                const isDeleted = $(this).find('input[type="text"]').data('is-deleted') || false;
                const uniqueFileId = $(this).find('input[type=text]').data('unique-file-id');
                const fileName = $(this).find('input[type=text]').data('file-name');
                let filePath = $(this).find('img').attr('src') || '';
                let imageFile = null;
                const imageDescription = $(this).find('input[type="text"]').val();

                // Check if it's an existing file or a new file based on the URL format
                if (filePath.startsWith('data:')) {
                    // For new file
                    imageFile = filePath;
                    filePath = '';
                } else {
                    // For existing file 
                    // filePath = filePath; No change in filePath
                    imageFile = '';
                }
                if (isDeleted && (uniqueFileId === null && filePath === ''))
                    return;

                // Prepare field names as required by the model binding
                const baseFieldName = `SiteImageUploaderVM.SitePicCategoryList[${index}].Images[${galleryIndex}]`;

                // Handle FileName
                formData.append(`${baseFieldName}.FileName`, fileName || '');

                // Always append description (even if empty)
                formData.append(`${baseFieldName}.FileDescription`, imageDescription || '');

                formData.append(`${baseFieldName}.FilePath`, filePath);
                formData.append(`${baseFieldName}.ImageFile`, imageFile);

                // Handle SitePicID
                const sitePicID = uniqueFileId ? uniqueFileId : -1; // Use uniqueFileId if available, default to -1
                formData.append(`${baseFieldName}.SitePicID`, sitePicID);

                // Append IsDeleted based on the data-is-deleted attribute
                formData.append(`${baseFieldName}.IsDeleted`, isDeleted ? 'true' : 'false');
            });
            // Append PicCatId and Description for the category
            formData.append(`SiteImageUploaderVM.SitePicCategoryList[${index}].PicCatId`, picCatId);
            formData.append(`SiteImageUploaderVM.SitePicCategoryList[${index}].Description`, categoryDescription);
        });

        const formDataObject = {};
        formData.forEach((value, key) => {
            // If key already exists, make it an array to handle multiple values for the same key
            if (formDataObject[key]) {
                if (!Array.isArray(formDataObject[key])) {
                    formDataObject[key] = [formDataObject[key]];
                }
                formDataObject[key].push(value);
            } else {
                formDataObject[key] = value;
            }
        });

        // Add token to request headers
        $.ajax({
            url: uploadUrl,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());
            },
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message || 'Images uploaded successfully!');
                    // Reload the page after short delay for success message
                    setTimeout(function () {
                        location.reload();
                    }, 2000); // 2 seconds delay
                } else {
                    toastr.error(data.message || 'An error occurred while uploading images.');
                }
            },
            error: function (xhr) {
                toastr.error(`An error occurred during upload. Status: ${xhr.status}`);
            }
        });
    });
});
