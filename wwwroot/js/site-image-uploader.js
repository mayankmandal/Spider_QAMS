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

    // Render existing categories and images
    existingImages.forEach(category => {
        // Call addCategorySection for each category that has existing images
        addCategorySection(category.picCatId, category.description);
    });

    // When category changes, add a new section for the selected category
    $('#categorySelect').on('change', function () {
        const selectedCatId = $(this).val();
        const selectedCatText = $("#categorySelect option:selected").text();

        // Prevent duplicate category sections
        if ($(`#category-${selectedCatId}`).length === 0) {
            addCategorySection(selectedCatId, selectedCatText);
        }
    });

    function addCategorySection(categoryId, categoryName) {
        // Create the category section card inside a Bootstrap column
        const sectionDiv = $(`
        <div class="col-md-6">
            <div class="category-section card p-3" id="category-${categoryId}">
                <h5>${categoryName} 
                    <button type="button" class="btn btn-danger btn-sm float-end remove-category" data-category-id="${categoryId}">
                        Remove
                    </button>
                </h5>
            </div>
        </div>
        `);

        // Add hidden inputs for PicCatId and Description
        const hiddenPicCatId = $(`<input type="hidden" name="SiteImageUploaderVM.SitePicCategoryList[${categoryId}].PicCatId" value="${categoryId}" />`);
        const hiddenDescription = $(`<input type="hidden" name="SiteImageUploaderVM.SitePicCategoryList[${categoryId}].Description" value="${categoryName}" />`);

        sectionDiv.find('.category-section').append(hiddenPicCatId, hiddenDescription);

        // Add file input for uploading images
        const fileInput = $(`<input type="file" multiple class="form-control mb-3 file-input" data-category-id="${categoryId}" />`);
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
        fileInput.on('change', function () {
            handleFileSelect(this.files, galleryDiv, categoryId);
        });
    }

    function createGalleryItem(filePath, description, fileName, categoryId, uniqueFileId = null) {
        const link = $(`
        <a href="${filePath}" data-lightbox="gallery-${categoryId}" data-title="${description}" class="image-link position-relative d-flex">
            <img src="${filePath}" class="img-thumbnail m-2" style="width: auto; height: 10rem;" alt="${description}" />
            <!-- Delete button overlay -->
            <button type="button" class="btn btn-sm delete-image-btn position-absolute">
                <i class="fas fa-times-circle"></i>
            </button>
        </a>
    `);

        const commentInput = $(`
        <input type="text" class="form-control mb-2" placeholder="Add a description" 
               value="${description || ''}" data-unique-file-id="${uniqueFileId}" 
               data-file-name="${fileName}" />
    `);

        const container = $('<div class="gallery-item me-2 mb-2 d-inline-block text-center"></div>');
        container.append(link).append(commentInput);

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

        // Hide the category section visually
        $(`#category-${categoryId}`).hide();

        // Mark all images in the category as deleted by setting data-is-deleted to true
        $(`#category-${categoryId} .gallery-item`).each(function () {
            $(this).find('input[type="text"]').attr('data-is-deleted','true');
        });
    });

    $(document).on('click', '.delete-image-btn', function (e) {
        // e.preventDefault(); // Prevent the link from triggering Lightbox
        // e.stopPropagation(); // Stop the event from bubbling up to parent elements

        $(this).closest('.gallery-item').remove();
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
            const description = $(this).find(`input[name="SiteImageUploaderVM.SitePicCategoryList[${catId}].Description"]`).val();

            galleryItems.each(function (galleryIndex) {
                const fileInput = $(this).find('input[type="file"]');
                const description = $(this).find('input[type="text"]').val();
                const uniqueFileId = $(this).children('input[type=text]').data('unique-file-id');
                const fileName = $(this).children('input[type=text]').data('file-name');

                // Check if the image is marked as deleted
                const isDeleted = $(this).find('input[type="text"]').data('is-deleted') || false;

                // Prepare field names as required by the model binding
                const baseFieldName = `SiteImageUploaderVM.SitePicCategoryList[${index}].Images[${galleryIndex}]`;

                // Handle FileName
                formData.append(`${baseFieldName}.FileName`, fileName || '');

                // Always append description (even if empty)
                formData.append(`${baseFieldName}.FileDescription`, description || '');

                let filePath = $(this).find('img').attr('src') || '';

                // Check if it's an existing file or a new file based on the URL format
                if (filePath.startsWith('data:')) {
                    // For new file
                    formData.append(`${baseFieldName}.FilePath`, '');
                    formData.append(`${baseFieldName}.ImageFile`, filePath);
                }
                else {
                    // For existing file 
                    formData.append(`${baseFieldName}.FilePath`, filePath);
                    formData.append(`${baseFieldName}.ImageFile`, '');
                }

                // Handle SitePicID
                const sitePicID = uniqueFileId ? uniqueFileId : -1; // Use uniqueFileId if available, default to -1
                formData.append(`${baseFieldName}.SitePicID`, sitePicID);

                // Append IsDeleted based on the data-is-deleted attribute
                formData.append(`${baseFieldName}.IsDeleted`, isDeleted ? 'true' : 'false');
            });
            // Append PicCatId and Description for the category
            formData.append(`SiteImageUploaderVM.SitePicCategoryList[${index}].PicCatId`, picCatId);
            formData.append(`SiteImageUploaderVM.SitePicCategoryList[${index}].Description`, description);
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

        console.log(formDataObject);

        // Add token to request headers
        $.ajax({
            url: uploadUrl,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val()); // Anti - forgery token for the page
            },
            success: function () {
                toastr.success('Images uploaded successfully!');
            },
            error: function (xhr) {
                toastr.error(`An error occurred during upload. Status: ${xhr.status}`);
            }
        });
    });
});
